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
    public class MenuGame : IGame
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        Simulation simulation;

        Renderer.RenderModule renderModule = null;
        OctTreeModule octTreeModule = null;
        GuiModule guiModule = null;
        InputModule inputModule = null;

        public MenuGame()
        {
        }

        public void Begin()
        {
            guiModule = new GuiModule(Main.GraphicsDevice, Main.Input);
            inputModule = new InputModule(Main.Input);
        
            simulation = new Simulation(Main.Content, new MISP.GenericScriptObject(
                "episode-name", "main-menu", "server", null));
            simulation.debugOutput += (s) => { Main.Write(s); };
            simulation.Content.FrontLoadTextures();
            Main.Write("Started menu simulation\n");
            Main.ScriptEngine.PrepareEnvironment(simulation.scriptEngine);
            Main.ScriptEngine.AddEnvironment("menu", simulation.scriptEngine, simulation.scriptContext);

            renderModule = new RenderModule(Main.GraphicsDevice, simulation.Content);
            simulation.modules.Add(renderModule);
            octTreeModule = new OctTreeModule(new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100)), 5.0f);
            simulation.modules.Add(octTreeModule);
            simulation.modules.Add(inputModule);
            simulation.modules.Add(guiModule);

            simulation.beginSimulation();

            var labelString = "Jemgine";

            var label = new UIItem(Layout.CenterItem(new Rectangle(0, 0, 10 * labelString.Length, 16),
                Main.GraphicsDevice.Viewport.Bounds));
            label.settings = new MISP.GenericScriptObject(
                "bg-color", new Vector3(0, 0, 0),
                "text-color", new Vector3(1,1,1),
                "label", labelString);
            guiModule.uiRoot.AddChild(label);
        }

        public void End()
        {
            simulation.endSimulation();
            Main.ScriptEngine.RemoveEnvironment("menu");
        }

        public void Update(float elapsedSeconds)
        {
                guiModule.HandleInput(Main.Input);
                inputModule.HandleInput(Main.Input);
                simulation.update(elapsedSeconds);
        }

        public void Draw(float elapsedSeconds)
        {
            Input.MouseObject = renderModule.MousePick(octTreeModule, new Vector2(Input.MouseX, Input.MouseY));
            renderModule.Draw(octTreeModule);
            guiModule.Draw(Main.GraphicsDevice);
        }
    }
}
