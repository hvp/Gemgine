using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gem.Common;
using Gem.Renderer;
using GeometryGeneration;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class ModelComponent : Component, Renderer.IRenderable
    {
        private SpacialComponent _spacial = null;
        private CompiledModel model = null;

        public ModelComponent(CompiledModel model)
        {
            this.model = model;
        }

        public override void AssociateSiblingComponents(IEnumerable<Component> siblings)
        {
            foreach (var sibling in siblings)
                if (sibling is SpacialComponent) _spacial = sibling as SpacialComponent;
            if (_spacial != null && model != null)
                _spacial.BoundingVolume = model.boundingSphere;
        }

        public Matrix World { get { return Matrix.Multiply(Matrix.CreateFromQuaternion(_spacial.Orientation),
            Matrix.CreateTranslation(_spacial.Position)); } }

        public void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice)
        {
            model.Draw(GraphicsDevice);
        }

    }
}
