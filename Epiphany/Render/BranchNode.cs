using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Epiphany.Render
{
    public class BranchNode : ISceneNode
    {
        internal BranchNode parent = null;

        public Euler Orientation = new Euler();
        private List<ISceneNode> children = new List<ISceneNode>();

        public void Add(ISceneNode child) { children.Add(child); }
        public void Remove(ISceneNode child) { children.Remove(child); }
        public IEnumerator<ISceneNode> GetEnumerator() { return children.GetEnumerator(); }

        public void UpdateWorldTransform(Matrix m)
        {
            foreach (var child in this)
                child.UpdateWorldTransform(m * Orientation.Transform);
        }

        public virtual void Draw(Renderer renderer)
        {
            foreach (var child in this)
                child.Draw(renderer);
        }
    }
}
