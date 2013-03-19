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
            try
            {
                simulation = new Simulation(Main.Content, new MISP.GenericScriptObject("episode-name", episodeName,
                    "server", true));
                System.Console.WriteLine("Started server simulation with episode " + episodeName);
                simulation.modules.Add(serverModule);
                simulation.beginSimulation();

                clientGame = new ClientGame(System.Net.IPAddress.Loopback, port);
                clientGame.Input = Input;
                clientGame.Main = Main;
                clientGame.Begin();

                Main.ScriptEngine.AddEnvironment("server", simulation.scriptEngine, simulation.scriptContext);

            }
            catch (Exception e)
            {
                
                System.Console.WriteLine("While trying to create a server game, " + e.Message);
                throw e;
            }
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
