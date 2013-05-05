using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        private IGame activeGame = null;
        private IGame nextGame = null;
        public IGame Game { get { return activeGame; } set { nextGame = value; } }

        public MISP.Console ScriptEngine;
        public Console.VirtualConsole ScriptConsole;

        GraphicsDeviceManager graphics;

        public Input Input { get; private set; }

        private Common.BufferedList<Action> injectedActionQueue = new Common.BufferedList<Action>();
        private System.Threading.Mutex injectedActionQueueLock = new System.Threading.Mutex();

        bool consoleOpen = false;
        private string startupCommand;

        public void ReportException(Exception e)
        {
            consoleOpen = true;
            ScriptConsole.WriteLine(e.Message);
            ScriptConsole.WriteLine(e.StackTrace);
        }

        public void ReportError(String msg)
        {
            consoleOpen = true;
            ScriptConsole.WriteLine(msg);
        }

        public void Write(String msg)
        {
            ScriptConsole.Write(msg);
        }

        public Main(String startupCommand)
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Input = new Input(Window.Handle);

            this.startupCommand = startupCommand;
        }

        protected override void LoadContent()
        {
            ScriptEngine = new MISP.Console(s => this.ScriptConsole.Write(s), null);
            ScriptConsole = new Console.VirtualConsole(command =>
            {
                InjectAction(() =>
                {
                    try
                    {
                        ScriptEngine.ExecuteCommand(command);
                    }
                    catch (Exception e)
                    {
                        ScriptConsole.Write(e.Message);
                    }
                });
            },
            GraphicsDevice, new ContentManager(Services, "Content"));

            Write("Created MISP environment.\n");


            ScriptEngine.environment.engine.AddGlobalVariable("game", (context) => { return this.activeGame; });

            ScriptEngine.environment.engine.AddFunction("start-server-session", "Start a server session on the specified port with the specified episode.",
                (context, arguments) =>
                {
                    var port = arguments[0] as int?;
                    if (port == null || !port.HasValue) throw new InvalidOperationException("Port must be an integer.");
                    Game = new ServerGame(arguments[1].ToString(), port.Value);
                    return Game;
                }, MISP.Arguments.Arg("port"), MISP.Arguments.Arg("episode"));

            ScriptEngine.environment.engine.AddFunction("start-client-session", "Connect to a server at a specified address and port.",
                (context, arguments) =>
                {
                    var address = arguments[0].ToString();
                    var port = arguments[1] as int?;
                    if (port == null || !port.HasValue) throw new InvalidOperationException("Port must be an integer.");
                    Game = new ClientGame(System.Net.IPAddress.Parse(address), port.Value);
                    return Game;
                }, MISP.Arguments.Arg("address"), MISP.Arguments.Arg("port"));

            if (!String.IsNullOrEmpty(startupCommand))
            {
                System.Console.WriteLine(startupCommand);
                InjectAction(() => { ScriptEngine.ExecuteCommand(startupCommand); });
            }

            Input.textHook.KeyDown += (hook, args) =>
                {
                    if (consoleOpen)
                        ScriptConsole.KeyDown(args.KeyCode, args.KeyValue);
                };

            Input.textHook.KeyUp += (hook, args) =>
                {
                    //if (consoleOpen)
                    ScriptConsole.KeyUp(args.KeyCode, args.KeyValue);
                };

            Input.textHook.KeyPress += (hook, args) =>
                {
                    if (args.KeyChar == '~')
                        consoleOpen = !consoleOpen;
                    else if (consoleOpen)
                        ScriptConsole.KeyPress(args.KeyChar);
                };

            ScriptEngine.environment.engine.AddFunction("clear", "Clear the console",
                (context, arguments) =>
                {
                    ScriptConsole.Clear();
                    return null;
                });

            this.Game = new LocalGame();
        }

        protected override void UnloadContent()
        {
            if (activeGame != null)
                activeGame.End();
            activeGame = null;
        }

        public void InjectAction(Action e)
        {
            injectedActionQueueLock.WaitOne();
            injectedActionQueue.Add(e);
            injectedActionQueueLock.ReleaseMutex();
        }

        protected override void Update(GameTime gameTime)
        {
            if (nextGame != null)
            {
                var saveActive = activeGame;
                if (activeGame != null) activeGame.End();
                activeGame = nextGame;
                activeGame.Main = this;
                activeGame.Input = Input;
                try
                {
                    activeGame.Begin();
                }
                catch (Exception e)
                {
                    activeGame = saveActive;
                    if (activeGame != null) activeGame.Begin();
                    ReportException(e);
                }
                nextGame = null;
            }

            try
            {
                Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (activeGame != null) activeGame.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                injectedActionQueueLock.WaitOne();
                injectedActionQueue.Swap();
                foreach (var action in injectedActionQueue) action();
                injectedActionQueue.ClearFront();
                injectedActionQueueLock.ReleaseMutex();
            }
            catch (Exception e)
            {
                ReportException(e);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (activeGame != null) activeGame.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (consoleOpen)
                ScriptConsole.Draw();
            base.Draw(gameTime);
        }
    }
}
