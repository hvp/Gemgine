/*using System;
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

namespace Epiphany
{
    public class GuiModule : IModule
    {
        private RenderContext2D uiRenderer = null;
        private Input Input = null;
        public MISP.ScriptObject defaultSettings = null;
        private Simulation sim;
        private List<GuiSceneNode> activeGuis = new List<GuiSceneNode>();

        public GuiModule(GraphicsDevice device, Input input)
        {
            uiRenderer = new RenderContext2D(device);

            defaultSettings = new MISP.GenericScriptObject(
                "bg-color", new Vector3(0, 0, 0),
                "text-color", new Vector3(1,1,1),
                "fg-color", new Vector3(1,1,1),
                "hidden-container", null
                );

            this.Input = input;
        }

        public void ButtonEvent(Object handler, UIItem button)
        {
            sim.EnqueueEvent("@raw-input-event", new MISP.ScriptList(handler, button));
        }

        public void HandleInput(Input input)
        {
            foreach (var gui in activeGuis)
            {
                gui.uiRoot.HandleMouse(gui.MouseHover, gui.LocalMouseX, gui.LocalMouseY, input, this);
                gui.MouseHover = false;
            }
        }

#region IModule members
        void IModule.BeginSimulation(Simulation sim)
        {
            defaultSettings.SetProperty("font", new BitmapFont(sim.Content.Load<Texture2D>("small-font"), 16, 16, 10));
            this.sim = sim;
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
            //scriptEngine.AddGlobalVariable("ui-root", c => uiRoot);
            var guiBinding = Gem.Gui.MispBinding.GenerateBinding();
            scriptEngine.AddGlobalVariable("ui", c => guiBinding);
            scriptEngine.AddFunction("create-ui-scene-node", "",
                (context, arguments) =>
                {
                    var r = new GuiSceneNode(MISP.AutoBind.IntArgument(arguments[0]),
                        MISP.AutoBind.IntArgument(arguments[1]), this);
                    activeGuis.Add(r);
                    return r;
                },
                    MISP.Arguments.Arg("width"), MISP.Arguments.Arg("height"));
        }
#endregion

        public void DrawRoot(UIItem root, OrthographicCamera camera, RenderTarget2D target)
        {
            uiRenderer.Camera = camera;
            uiRenderer.BeginScene(target);
            root.Render(uiRenderer);
        }
    }
}
*/