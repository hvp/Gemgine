using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Epiphany.Render
{
    public class LeafNode : ISceneNode
    {
        internal BranchNode parent = null;

        public Euler Orientation = new Euler();
        public Mesh Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;

        public LeafNode(Mesh mesh) { this.Mesh = mesh; }

        private Matrix worldTransformation = Matrix.Identity;

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m * Orientation.Transform;
        }

        public virtual void Draw(Renderer renderer)
        {
            renderer.Effect.DiffuseColor = Color;
            if (Texture != null) renderer.Effect.Texture = Texture;
            else renderer.Effect.Texture = renderer.White;
            renderer.ApplyEffect(worldTransformation);
            renderer.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                Mesh.verticies, 0, Mesh.verticies.Length, Mesh.indicies, 0, Mesh.indicies.Length / 3);
        }
    }
}
