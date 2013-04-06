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
    public class ClientGame : IGame
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        Simulation simulation;
        
        private System.Net.IPAddress host;
        private int port;

        Network.ClientModule clientModule = null;
        Renderer.RenderModule renderModule = null;
        OctTreeModule octTreeModule = null;
        GuiModule guiModule = null;
        InputModule inputModule = null;

        UIItem mouseLabel;
        UInt32 mouseClickID;

        public ClientGame(System.Net.IPAddress host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Begin()
        {
            clientModule = new Network.ClientModule(host, port, this);
            guiModule = new GuiModule(Main.GraphicsDevice, Main.Input);
            inputModule = new InputModule(Main.Input);
        }

        internal void StartSimulation(String episodeName, uint version)
        {
            //System.Threading.Thread.Sleep(10000);
            if (simulation != null) throw new InvalidProgramException();
            simulation = new Simulation(Main.Content, new MISP.GenericScriptObject(
                "episode-name", episodeName, "server", null));
            simulation.Content.FrontLoadTextures();
            Main.Write("Started client simulation with episode " + episodeName + "\n");
            Main.ScriptEngine.PrepareEnvironment(simulation.scriptEngine);
            Main.ScriptEngine.AddEnvironment("client", simulation.scriptEngine, simulation.scriptContext);
            //simulation.scriptEngine.AddGlobalVariable("ui", (context) => { return uiRoot; });
            //uiRoot.children.Clear();

            renderModule = new RenderModule(Main.GraphicsDevice, simulation.Content);
            simulation.modules.Add(renderModule);
            octTreeModule = new OctTreeModule(new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100)), 5.0f);
            simulation.modules.Add(octTreeModule);
            simulation.modules.Add(clientModule);
            simulation.modules.Add(inputModule);
            simulation.modules.Add(guiModule);

            simulation.beginSimulation();

            mouseLabel = new UIItem(new Rectangle(0, 0, 128, 64));
            mouseLabel.settings = new MISP.GenericScriptObject("bg-color", new Vector3(1, 1, 1));
            guiModule.uiRoot.AddChild(mouseLabel);
        }

        public void End()
        {
            if (simulation != null) simulation.endSimulation();
            Main.ScriptEngine.RemoveEnvironment("client");
        }

        public void Update(float elapsedSeconds)
        {
            if (simulation != null)
            {
                guiModule.HandleInput(Main.Input);
                inputModule.HandleInput(Main.Input);
                simulation.update(elapsedSeconds);
            }
            else (clientModule as IModule).Update(elapsedSeconds);
        }

        public void Draw(float elapsedSeconds)
        {
            if (renderModule != null)
            {
                mouseClickID = renderModule.MousePick(octTreeModule, new Vector2(Input.MouseX, Input.MouseY));
                mouseLabel.settings.SetProperty("label", String.Format("M:{0:X8}", mouseClickID));
                renderModule.Draw(octTreeModule);
                Input.MouseObject = mouseClickID;
            }
            if (guiModule != null) guiModule.Draw(Main.GraphicsDevice);
        }
    }
}
