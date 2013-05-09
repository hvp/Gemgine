using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Epiphany
{
    public class Simulation
    {
        private BufferedList<Component> created = new BufferedList<Component>();
        private BufferedList<UInt32> destroyed = new BufferedList<UInt32>();

        private class Module
        {
            internal IModule module;
            internal double timeSinceUpdate;
        }

        private List<Module> modules = new List<Module>();
        
        private class QueuedEvent
        {
            internal String eventName;
            internal ObjectList payload;
        }

        private BufferedList<QueuedEvent> eventQueue = new BufferedList<QueuedEvent>();
        private Dictionary<String, Action<ObjectList>> eventHandlers = new Dictionary<string, Action<ObjectList>>();

        private Action<String> debugOutput = null;
        private void debug(String str) { if (debugOutput != null) debugOutput(str); }
        public ContentManager Content { get; private set; }

        public Simulation(ContentManager Content, PropertySet settings)
        {
            this.Content = Content;
        }

        public void SetDebugHandler(Action<String> handler)
        {
            debugOutput = handler;
        }

        public void EnqueueEvent(String name, ObjectList payload)
        {
            eventQueue.Add(new QueuedEvent { eventName = name, payload = payload });
        }

        public void RegisterEventHandler(String name, Action<ObjectList> handler)
        {
            if (eventHandlers.ContainsKey(name))
                eventHandlers[name] += handler;
            else
                eventHandlers.Add(name, handler);
        }

        public void AddModule(IModule module)
        {
            modules.Add(new Module { module = module, timeSinceUpdate = module.UpdateInterval });
        }

        public T FindModule<T>() where T : class, IModule
        {
            foreach (var module in modules)
                if (module.module is T) return module.module as T;
            return null;
        }

        public void BeginSimulation()
        {
            foreach (var module in modules) module.module.BeginSimulation(this);
        }

        public void EndSimulation()
        {
            foreach (var module in modules) module.module.EndSimulation();
        }

        public void Update(GameTime time)
        {
            created.Swap();
            foreach (var module in modules)
            {
                try
                {
                    module.module.AddComponents(created.Front);
                }
                catch (Exception exp)
                {
                    debug("Exception raised by module " + module.GetType().Name + " : " + exp.Message);
                }
            }
            created.ClearFront();

            destroyed.Swap();
            foreach (var module in modules)
            {
                try
                {
                    module.module.RemoveEntities(destroyed.Front);
                }
                catch (Exception exp)
                {
                    debug("Exception raised by module " + module.GetType().Name + " : " + exp.Message);
                }
            }
            destroyed.ClearFront();

            foreach (var module in modules)
            {
                try
                {
                    if (module.module.UpdateInterval <= 0) module.module.Update(time);
                    else
                    {
                        module.timeSinceUpdate += time.ElapsedGameTime.TotalSeconds;
                        if (module.timeSinceUpdate >= module.module.UpdateInterval)
                        {
                            module.module.Update(time);
                            module.timeSinceUpdate -= module.module.UpdateInterval;
                        }
                    }
                }
                catch (Exception exp)
                {
                    debug("Exception raised by module " + module.GetType().Name + " : " + exp.Message);
                }
            }

            eventQueue.Swap();
            foreach (var e in eventQueue)
            {
                try
                {
                    if (eventHandlers.ContainsKey(e.eventName) && eventHandlers[e.eventName] != null)
                        eventHandlers[e.eventName].Invoke(e.payload);
                }
                catch (Exception exp)
                {
                    debug("Exception raised while handling event " + e.eventName + " : " + exp.Message);
                }
            }
            eventQueue.ClearFront();
        }

        public void AddComponents(params Component[] components)
        {
            created.AddRange(components);
        }

        public void RemoveEntities(params UInt32[] id)
        {
            destroyed.AddRange(id);
        }
    }
}
