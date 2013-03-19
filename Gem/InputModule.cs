using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISP;
using System.Windows.Forms;

namespace Gem
{
    public class InputModule : IModule
    {
        enum BindingType
        {
            DOWN,
            UP,
            HELD
        }

        public Input Input;
        private Simulation sim;
        private Dictionary<Keys, List<Tuple<BindingType, ScriptObject>>> eventBindings =
            new Dictionary<Keys, List<Tuple<BindingType, ScriptObject>>>();
        private List<Keys> keysWithHeldEvent = new List<Keys>();
        private Common.BufferedList<Tuple<Keys, BindingType>> eventQueue = new Common.BufferedList<Tuple<Keys, BindingType>>();
        private Dictionary<UInt32, ScriptObject> clickBindings = new Dictionary<uint, ScriptObject>();

        public InputModule(Input input)
        {
            this.Input = input;
        }

        private void handleKeyDown(Object hook, EventArgs args)
        {
            eventQueue.Add(new Tuple<Keys, BindingType>((args as KeyEventArgs).KeyCode, BindingType.DOWN));
        }

        private void handleKeyUp(Object hook, EventArgs args)
        {
            eventQueue.Add(new Tuple<Keys, BindingType>((args as KeyEventArgs).KeyCode, BindingType.UP));
        }

        public void HandleInput(Input Input)
        {
            foreach (var key in keysWithHeldEvent)
                if (Input.Check((Microsoft.Xna.Framework.Input.Keys)key))
                    eventQueue.Add(new Tuple<Keys, BindingType>(key, BindingType.HELD));

            eventQueue.Swap();
            foreach (var keyEvent in eventQueue.Front)
                if (eventBindings.ContainsKey(keyEvent.Item1))
                    foreach (var binding in eventBindings[keyEvent.Item1])
                        if (binding.Item1 == keyEvent.Item2)
                            sim.EnqueueEvent("@raw-input-event", new ScriptList(binding.Item2));
            eventQueue.ClearFront();

            if (Input.MousePressed())
                if (clickBindings.ContainsKey(Input.MouseObject))
                    sim.EnqueueEvent("@raw-input-event", new ScriptList(clickBindings[Input.MouseObject]));
        }

        void IModule.BeginSimulation(Simulation sim)
        {
            this.sim = sim;
            Input.textHook.KeyDown += handleKeyDown;
            Input.textHook.KeyUp += handleKeyUp;
        }

        void IModule.EndSimulation()
        {
            Input.textHook.KeyDown -= handleKeyDown;
            Input.textHook.KeyUp -= handleKeyUp;
        }

        void IModule.AddComponents(List<Component> components)
        {
        }

        void IModule.RemoveEntities(List<uint> entities)
        {
        }

        void IModule.Update(float elapsedSeconds)
        {
        }

        void IModule.BindScript(Engine scriptEngine)
        {
            scriptEngine.AddFunction("bind-key", "Bind an event handler to a key.", (context, arguments) =>
                {
                    var keyNames = AutoBind.StringArgument(arguments[0]);
                    var bindingType = AutoBind.StringArgument(arguments[1]).ToUpperInvariant();
                    var body = arguments[2];
                    var lambda = Function.MakeFunction("key-binding lambda", new ScriptList(), "",
                        body as ScriptObject, context.Scope, true);
                    BindingType parsedBindType;
                    if (!Enum.TryParse(bindingType, out parsedBindType))
                    {
                        context.RaiseNewError("Unknown binding type", context.currentNode);
                        return null;
                    }
                    foreach (var c in keyNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Keys key;
                        if (Enum.TryParse(c, out key))
                        {
                            if (parsedBindType == BindingType.HELD)
                                keysWithHeldEvent.Add(key);
                            if (!eventBindings.ContainsKey(key))
                                eventBindings.Add(key, new List<Tuple<BindingType, ScriptObject>>());
                            eventBindings[key].Add(new Tuple<BindingType, ScriptObject>(parsedBindType, lambda));
                        }
                    }
                    return true;
                }, 
                Arguments.Arg("key-names", "A string containing key-names separated by spaces."),
                Arguments.Arg("binding-type", "Any of DOWN, UP, or HELD."),
                Arguments.Lazy("code"));

            scriptEngine.AddFunction("unbind-key", "Clear event handlers associated with a key/event combo",
                (context, arguments) =>
                {
                    var keyNames = AutoBind.StringArgument(arguments[0]);
                    var bindingType = AutoBind.StringArgument(arguments[1]).ToUpperInvariant();
                    BindingType parsedBindType;
                    if (!Enum.TryParse(bindingType, out parsedBindType))
                    {
                        context.RaiseNewError("Unknown binding type", context.currentNode);
                        return null;
                    }
                    foreach (var c in keyNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Keys key;
                        if (Enum.TryParse(c, out key))
                        {
                            if (parsedBindType == BindingType.HELD)
                                keysWithHeldEvent.Remove(key);
                            if (eventBindings.ContainsKey(key))
                                eventBindings[key].RemoveAll(p => p.Item1 != parsedBindType);
                        }
                    }
                    return true;
                }, 
                Arguments.Arg("key-names", "A string containing key-names separated by spaces."),
                Arguments.Arg("binding-type", "Any of DOWN, UP, or HELD."));

            scriptEngine.AddFunction("on-click", "Create an on-click event handler for an entity.",
                (context, arguments) =>
                    {
                        var objectID = AutoBind.UIntArgument(arguments[0]);
                        var body = arguments[1];
                        var lambda = Function.MakeFunction("on-click lambda", new ScriptList(), "",
                            body as ScriptObject, context.Scope, true);
                        clickBindings.Upsert(objectID, lambda);
                        return true;
                    },
                Arguments.Arg("object-id", "id of the clicked object."),
                Arguments.Lazy("code"));
        }
    }
}
