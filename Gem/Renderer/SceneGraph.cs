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
    public class SceneGraphRoot : Component, Renderer.IRenderable
    {
        public SceneNode rootNode;

        public SceneGraphRoot() { rootNode = new SceneNode(); }

        private SpacialComponent _spacial = null;

        public override void AssociateSiblingComponents(IEnumerable<Component> siblings)
        {
            foreach (var sibling in siblings)
                if (sibling is SpacialComponent) _spacial = sibling as SpacialComponent;
        }

        public void PreDraw(GraphicsDevice device, RenderContext context)
        {
            var worldTransform = _spacial.Transform;
            rootNode.UpdateWorldTransform(worldTransform);
            rootNode.PreDraw(device, context);
        }

        public void DrawEx(RenderContext context)
        {
            rootNode.DrawEx(context);
        }

        public void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor, VertexPositionColor> debug)
        {
            //var iT = Matrix.Transpose(Matrix.Invert(_spacial.Transform));
            //var newRay = new Ray(
            //    Vector3.Transform(mouseRay.Position, iT),
            //    Vector3.TransformNormal(mouseRay.Direction, iT));
            rootNode.UpdateWorldTransform(_spacial.Transform);
            rootNode.CalculateLocalMouse(mouseRay, debug);
        }
    }
}
