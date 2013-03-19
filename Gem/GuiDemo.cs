using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Renderer;
using Gem.Common;
using Gem.Gui;

namespace Gem
{
    public class GuiDemo : IGame
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        OrthographicCamera Camera;// = new OrthographicCamera(new Viewport(0, 0, 200, 150));
        RenderContext2D RenderContext;

        public UIItem gui;

        public void Begin()
        {
            RenderContext = new RenderContext2D(Main.GraphicsDevice);
            Camera = new OrthographicCamera(Main.GraphicsDevice.Viewport);
            gui = new UIItem(new Rectangle(32, 32, 128, 64));
            gui.settings = new MISP.GenericScriptObject("bg-color", new Vector3(1, 0, 0));
            gui.hoverSettings = new MISP.GenericScriptObject("bg-color", new Vector3(0, 1, 0));
            Camera.focus = new Vector2(Main.GraphicsDevice.Viewport.Width / 2, Main.GraphicsDevice.Viewport.Height / 2);
        }

        public void End()
        {
            //NativeMethods.FreeConsole();
        }

        public void Update(float elapsedSeconds)
        {
            if (gui != null)
                gui.Update(Main.Input);
        }

        public void Draw(float elapsedSeconds)
        {
            Main.GraphicsDevice.Clear(Color.Black);
            Main.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            RenderContext.Camera = Camera;
            RenderContext.BeginScene();

            if (gui != null) gui.Render(RenderContext);
        }
    }
}
