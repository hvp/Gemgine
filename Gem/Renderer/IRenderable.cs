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
        //Matrix World { get; }
        //void Draw(GraphicsDevice GraphicsDevice);
        void DrawEx(RenderContext context);
    }
}
