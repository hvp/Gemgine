using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class Simulation
    {
        private Common.BufferedList<Component> created = new Common.BufferedList<Component>();
        private Common.BufferedList<UInt32> destroyed = new Common.BufferedList<UInt32>();
        internal MISP.Engine scriptEngine;
        internal MISP.Context scriptContext;
        internal List<IModule> modules = new List<IModule>();
        private UInt32 nextGlobalEntityID = 1;
        private UInt32 nextLocalEntityID = 0xFFFFFF00;
        internal Action<byte[]> sendMessageHandler = null;
        private MISP.GenericScriptObject eventHandlers = new MISP.GenericScriptObject();
        private Common.BufferedList<Tuple<String, MISP.ScriptList>> eventQueue = 
            new Common.BufferedList<Tuple<String, MISP.ScriptList>>();
        private float cachedElapsedSeconds;
        
        public bool IsLocalEntity(UInt32 id)
        {
            return id >= 0x80000000;
        }

        public ContentManifestExtensions.EpisodeContentManager Content;

        public Action<String> debugOutput = null;
        public void debug(String str) { if (debugOutput != null) debugOutput(str); }
        public MISP.ScriptObject settings;

        public Simulation(Microsoft.Xna.Framework.Content.ContentManager rawContent, MISP.ScriptObject settings)
        {
            Content = new ContentManifestExtensions.EpisodeContentManager(rawContent.ServiceProvider,
                rawContent.RootDirectory, settings.gsp("episode-name"));
            scriptEngine = new MISP.Engine();
            scriptContext = new MISP.Context(settings);


            #region MISP Bindings
            #region System
            Gui.MispBinding.Bind(scriptEngine);
            scriptEngine.AddGlobalVariable("sim", (context) => { return this; });
            var xna = Gem.Math.MispBinding.BindXNAMath();
            scriptEngine.AddGlobalVariable("xna", (context) => { return xna; });
            scriptEngine.AddGlobalVariable("delta", (context) => { return cachedElapsedSeconds; });
            #endregion

            #region Entity Creation
            scriptEngine.AddFunction("alloc-global-entity-id", "Allocate a global entity id.", (context, arguments) =>
                {
                    if (settings["server"] == null)
                    {
                        context.RaiseNewError("Can't allocate global ids on clients.", null);
                        return null;
                    }

                    var r = nextGlobalEntityID;
                    nextGlobalEntityID += 1;
                    return r;
                });

            scriptEngine.AddFunction("alloc-local-entity-id", "Allocate a global entity id.", (context, arguments) =>
            {
                var r = nextLocalEntityID;
                nextLocalEntityID += 1;
                return r;
            });

            scriptEngine.AddFunction("create-entity", "Create an entity.", (context, arguments) =>
                {
                    UInt32 id = MISP.AutoBind.UIntArgument(arguments[0]);
                    addComponents(id, MISP.AutoBind.ListArgument(arguments[1]).Where(o => o is Component)
                        .Select(o => o as Component));
                    return id;
                }, MISP.Arguments.Arg("id"), MISP.Arguments.Repeat("component"));
            #endregion

            #region Basic Components
            scriptEngine.AddFunction("c-spacial", "Create a spacial component.", (context, arguments) =>
                {
                    var r = new SpacialComponent();
                    r.Position = Math.MispBinding.Vector3Argument(arguments[0]);
                    return r;
                }, 
                MISP.Arguments.Arg("position", "A vector3"));
            #endregion

            #region Events and Messages
            scriptEngine.AddFunction("message", "Send a message to the server or clients.", (context, arguments) =>
                {
                    var id = MISP.AutoBind.StringArgument(arguments[0]);
                    var data = MISP.AutoBind.ListArgument(arguments[1]);
                    if (sendMessageHandler != null)
                        sendMessageHandler(Network.ScriptMessage.EncodeMessage(id, data));
                    return null;
                }, MISP.Arguments.Arg("message-id"), MISP.Arguments.Repeat("data"));

            scriptEngine.AddFunction("local-message", "Send a message to yourself.", (context, arguments) =>
            {
                var id = MISP.AutoBind.StringArgument(arguments[0]);
                var data = MISP.AutoBind.ListArgument(arguments[1]);
                EnqueueEvent(id, data);
                return null;
            }, MISP.Arguments.Arg("message-id"), MISP.Arguments.Repeat("data"));

            scriptEngine.AddFunction("set-handler", "Set a message handler.", (context, arguments) =>
                {
                    var id = MISP.AutoBind.StringArgument(arguments[0]);
                    var handler = MISP.AutoBind.LazyArgument(arguments[1]);
                    if (!MISP.Function.IsFunction(handler))
                        handler = MISP.Function.MakeFunction("generated-function", MISP.Arguments.Args(),
                            "Generated by set-handler", handler, context.Scope, true);
                    eventHandlers.SetProperty(id, handler);
                    return handler;
                }, MISP.Arguments.Arg("message-id"), MISP.Arguments.Lazy("code", "Code or an existing function."));
            #endregion

            #endregion

            this.settings = settings;
        }

        public void EnqueueEvent(String id, MISP.ScriptList data)
        {
            eventQueue.Add(new Tuple<String, MISP.ScriptList>(id, data));
        }

        public T FindModule<T>() where T : class, IModule
        {
            foreach (var module in modules)
                if (module is T) return module as T;
            return null;
        }

        public String tryGetScript(String name)
        {
            try
            {
                var r = Content.OpenUnbuiltTextStream(name + ".msp");
                return r.ReadToEnd();
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                return "";
            }
        }

        public void PrepareContext()
        {
            var scope = scriptContext.Scope;
            scriptContext.Reset();
            scriptContext.ReplaceScope(scope);
        }

        public Object runScript(String name)
        {
            PrepareContext();
            return scriptEngine.EvaluateString(scriptContext, tryGetScript(name), name, true);
            if (scriptContext.evaluationState == MISP.EvaluationState.UnwindingError)
            {
                Console.Write("Error:\n");
                Console.Write(MISP.Console.PrettyPrint2(scriptContext.errorObject, 0));
            }
        }

        public void beginSimulation()
        {
            foreach (var module in modules) module.BeginSimulation(this);
            foreach (var module in modules) module.BindScript(scriptEngine);
            runScript("startup");
        }

        public void endSimulation()
        {
            foreach (var module in modules) module.EndSimulation();
        }

        public void update(float ElapsedSeconds)
        {
            cachedElapsedSeconds = ElapsedSeconds;

            created.Swap();
            foreach (var m in modules) m.AddComponents(created.Front);
            created.ClearFront();

            destroyed.Swap();
            foreach (var m in modules) m.RemoveEntities(destroyed.Front); 
            destroyed.ClearFront();

            foreach (var module in modules) module.Update(ElapsedSeconds);

            eventQueue.Swap();
            foreach (var e in eventQueue)
            {
                Object handler = null;
                MISP.ScriptList args = e.Item2;
                if (e.Item1 == "@raw-input-event")
                {
                    if (args.Count > 0)
                    {
                        handler = args[0];
                        args.RemoveAt(0);
                    }
                    else
                        Console.WriteLine("Invalid input event.");
                }
                else
                    handler = eventHandlers.GetLocalProperty(e.Item1);
                if (handler == null) Console.Write("Invalid queued event.");
                if (handler is MISP.ScriptObject && MISP.Function.IsFunction(handler as MISP.ScriptObject))
                {
                    PrepareContext();
                    MISP.Function.Invoke(handler as MISP.ScriptObject, scriptEngine, scriptContext, e.Item2);
                    if (scriptContext.evaluationState == MISP.EvaluationState.UnwindingError)
                    {
                        Console.Write("Error:\n");
                        Console.Write(MISP.Console.PrettyPrint2(scriptContext.errorObject, 0));
                    }
                }
                else
                    Console.WriteLine("Invalid event handler.");
            }
            eventQueue.ClearFront();
        }

        private void addComponents(UInt32 entityID, IEnumerable<Component> components)
        {
            UInt32 syncID = 0;
            foreach (var component in components)
            {
                component.EntityID = entityID;
                component.SyncID = syncID;
                syncID += 1;
                created.Add(component);
                component.AssociateSiblingComponents(components);
            }
        }
    }
}
