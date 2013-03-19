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
    public class GuiModule : IModule
    {
        private OrthographicCamera uiCamera = null;
        private RenderContext2D uiRenderer = null;
        internal Gui.UIItem uiRoot = null;
        private Input Input = null;
        private MISP.ScriptObject defaultRootSettings = null;

        public GuiModule(GraphicsDevice device, Input input)
        {
            uiCamera = new OrthographicCamera(device.Viewport);
            uiRenderer = new RenderContext2D(device);
            uiRoot = new UIItem(device.Viewport.Bounds);
            uiRoot.defaults = new MISP.GenericScriptObject();
            
            uiCamera.focus = new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2);

            defaultRootSettings = new MISP.GenericScriptObject(
                "bg-color", new Vector3(0, 0, 0),
                "hidden-container", true
                );

            var loadingLabel = new UIItem(Gui.Layout.CenterItem(new Rectangle(0, 0, 256, 32), uiRoot.rect));
            loadingLabel.settings = new MISP.GenericScriptObject(
                "bg-color", new Vector3(0, 1, 0),
                "label", "Loading...");
            //uiRoot.AddChild(loadingLabel);
            uiRoot.settings = defaultRootSettings;

            this.Input = input;
        }

        public void ClearUI() { uiRoot.children.Clear(); }

        public void HandleInput(Input input)
        {
            uiRoot.Update(input);
        }

#region IModule members
        void IModule.BeginSimulation(Simulation sim)
        {
            uiRoot.defaults.SetProperty("font", new BitmapFont(sim.Content.Load<Texture2D>("font"), 16, 16, 10));
            uiRoot.defaults.SetProperty("text-color", new Vector3(0, 0, 0));
        }

        void IModule.EndSimulation()
        {
        }

        void IModule.AddComponents(List<Component> components)
        {
        }

        void IModule.RemoveEntities(List<UInt32> entities)
        {
        }

        void IModule.Update(float elapsedSeconds)
        {
        }

        void IModule.BindScript(MISP.Engine scriptEngine)
        {
            scriptEngine.AddGlobalVariable("ui", (context) => { return uiRoot; });
        }
#endregion

        public void Draw(GraphicsDevice device)
        {
            uiRenderer.Camera = uiCamera;
            uiRenderer.BeginScene();
            uiRoot.Render(uiRenderer);
        }
    }
}
