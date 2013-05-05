using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem.Common;
using Gem.Renderer;
using GeometryGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem
{
    public class SceneNode : List<SceneNode>, IRenderable
    {
        public Vector3 Scale = Vector3.One;
        public Vector3 Offset;
        public Vector3 EulerOrientation;
        public bool Lighted = true;

        private Matrix CalculateLocalTransformation()
        {
            return Matrix.CreateTranslation(Offset)
                * Matrix.CreateFromYawPitchRoll(EulerOrientation.X, EulerOrientation.Y, EulerOrientation.Z)
                * Matrix.CreateScale(Scale);
        }

        protected Matrix localTransformation = Matrix.Identity;
        private Matrix worldTransformation = Matrix.Identity;
        public CompiledModel leaf = null;

        public void UpdateWorldTransform(Matrix m)
        {
            worldTransformation = m;
            localTransformation = worldTransformation * CalculateLocalTransformation();
            foreach (var child in this)
                child.UpdateWorldTransform(localTransformation);
        }

        public virtual void PreDraw(GraphicsDevice device, RenderContext context)
        {
            foreach (var child in this) child.PreDraw(device, context);
        }

        public virtual void DrawEx(RenderContext context)
        {
            if (leaf != null)
            {
                context.World = localTransformation;
                context.Draw(leaf);
            }

            foreach (var child in this)
                child.DrawEx(context);
        }

        public virtual void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            foreach (var child in this)
                child.CalculateLocalMouse(mouseRay, debug);
        }
    }
}
