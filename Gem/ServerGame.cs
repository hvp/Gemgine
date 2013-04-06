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
    public class ServerGame : IGame
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        public Network.ServerModule serverModule;
        public Simulation simulation;
        private String episodeName;
        private int port;
        public IGame clientGame;

        public ServerGame(String episodeName, int port)
        {
            this.port = port;
            serverModule = new Network.ServerModule(port);
            this.episodeName = episodeName;
        }

        public void Begin()
        {
            simulation = new Simulation(Main.Content, new MISP.GenericScriptObject("episode-name", episodeName,
                "server", true));
            Main.Write("Started server simulation with episode " + episodeName + "\n");
            simulation.modules.Add(serverModule);
            simulation.beginSimulation();

            clientGame = new ClientGame(System.Net.IPAddress.Loopback, port);
            clientGame.Input = Input;
            clientGame.Main = Main;
            clientGame.Begin();

            Main.ScriptEngine.PrepareEnvironment(simulation.scriptEngine);
            Main.ScriptEngine.AddEnvironment("server", simulation.scriptEngine, simulation.scriptContext);

        }

        public void End()
        {
            simulation.endSimulation();
            clientGame.End();
            Main.ScriptEngine.RemoveEnvironment("server");
        }

        public void Update(float elapsedSeconds)
        {
            simulation.update(elapsedSeconds);
            clientGame.Update(elapsedSeconds);
        }

        public void Draw(float elapsedSeconds)
        {
            clientGame.Draw(elapsedSeconds);
        }
    }
}
