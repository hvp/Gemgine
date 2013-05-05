using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Renderer
{
    public interface IRenderable
    {
        void PreDraw(GraphicsDevice device, RenderContext context);
        void DrawEx(RenderContext context);
        void CalculateLocalMouse(Ray mouseRay, Action<VertexPositionColor,VertexPositionColor> debug);
    }
}
