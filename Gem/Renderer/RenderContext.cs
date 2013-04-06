using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Gem;
using Gem.Renderer;
using Gem.Common;
using GeometryGeneration;

namespace Gem.Renderer
{
    public class RenderContext
    {
        protected Effect effect;
        protected GraphicsDevice device;

        public void BeginScene(Effect effect, GraphicsDevice device)
        {
            this.effect = effect;
            this.device = device;
        }

        public void Apply()
        {
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public virtual Matrix World
        {
            set
            {
                effect.Parameters["World"].SetValue(value);
                effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(value)));
            }
        }

        public void Draw(CompiledModel model)
        {
            Apply();

            device.SetVertexBuffer(model.verticies);
            device.Indices = model.indicies;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.verticies.VertexCount,
                0, System.Math.Min(model.primitiveCount, 65535));
        }


    }

    public class RenderIDContext : RenderContext
    {
        public override Matrix World
        {
            set
            {
                effect.Parameters["World"].SetValue(value);
            }
        }
    }
}
