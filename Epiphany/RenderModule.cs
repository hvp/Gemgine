using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Epiphany
{
    public class RenderComponent : Component
    {
        public Render.ISceneNode sceneNode;
    }

    public class RenderModule : IModule
    {
        private Render.ISceneNode sceneRoot;
        private Render.Renderer renderer = null;

        public RenderModule(GraphicsDevice device)
        {
            renderer = new Render.Renderer(device);
        }

        public double UpdateInterval
        {
            get { return -1; }
        }

        public void BeginSimulation(Simulation sim)
        {
        }

        public void EndSimulation()
        {
        }

        public void AddComponents(List<Component> components)
        {
            foreach (var component in components)
            {
             
            {


            }
            }
        }

        public void RemoveEntities(List<uint> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(Microsoft.Xna.Framework.GameTime time)
        {
            throw new NotImplementedException();
        }
    }
}
