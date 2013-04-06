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
using GeometryGeneration;

namespace XNAConsole
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        TextDisplay display;
        MISP.Console console;
        DynamicConsoleBuffer dynamicConsole;
        List<String> commandRecallBuffer = new List<String>();
        int recallBufferPlace = 0;
        internal BufferedList<MISP.ScriptObject> events = new BufferedList<MISP.ScriptObject>();
        System.Threading.Mutex handlerMutex = new System.Threading.Mutex();
        InputModule inputModule;
        MouseInput input;
        RenderModule renderer;
        public Scene scene;
        public Vector3 clearColor;

        XnaTextInput.TextInputHandler handler;
        String[] args;
        
        public Main(String[] args)
        {
            this.args = args;

            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = false;
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;// 1280;
            graphics.PreferredBackBufferHeight = 600;// 800;
            graphics.IsFullScreen = false;

            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 240.0f);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            display = new TextDisplay(80, 25, /*128, 33,*/ GraphicsDevice, Content);
            display.viewport = new Viewport(0, 0, 800, 600);
            dynamicConsole = new DynamicConsoleBuffer(2048, display);

            input = new MouseInput();
            inputModule = new InputModule(this);
            renderer = new RenderModule(GraphicsDevice, Content);

            //scene = new Scene(new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100)), 5.0f);
            //scene.AddModel(new ModelComponent(GeometryGeneration.CompiledModel.CompileModel(
            //    new RawModel(Gen.FacetCopy(Gen.ColorizeCopy(
            //        Gen.CreateLine(new Vector3(0,0,0), new Vector3(0,5,5), new Vector3(1,0,0), 0.1f),
            //        Vector4.One))),
            //    GraphicsDevice)));

            dynamicConsole.Write("MISP Console\nKeyboard Controls:\nctrl+enter: send command\nctrl+up/down: scroll output\nshift+up/down: recall commands\n\n");
            
            console = new MISP.Console(
                (str) =>
                {
                    dynamicConsole.Write(str);//, Color.White, Color.Black);
                },
                (engine) =>
                {
                    engine.AddGlobalVariable("@host", (context) => { return this; });
                    inputModule.BindScript(engine);
                    var math = Gem.Math.MispBinding.BindXNAMath();
                    engine.AddGlobalVariable("xna", c => math );
                    renderer.BindScript(engine);

                    //var osm = StreetData.MispBinding.GenerateStaticBinding();
                    //engine.AddGlobalVariable("osm", c => osm);

                   engine.AddFunction("recall", "Recall a function definition into the input box so it can be modified.",
                        (context, arguments) =>
                        {
                            console.noEcho = true;
                            dynamicConsole.activeInput.cursor = 0;
                            var writer = new System.IO.StringWriter();
                            engine.EmitFunction(arguments[0] as MISP.ScriptObject, "defun", writer, true);
                            dynamicConsole.activeInput.input = writer.ToString();
                            return null;
                        }, MISP.Arguments.Arg("function"));

                   engine.AddFunction("video-mode", "Set the video mode",
                       (context, arguments) =>
                       {
                           console.noEcho = true;
                           graphics.IsFullScreen = (arguments[0] != null);
                           graphics.PreferredBackBufferWidth = MISP.AutoBind.IntArgument(arguments[1]);
                           graphics.PreferredBackBufferHeight = MISP.AutoBind.IntArgument(arguments[2]);
                           graphics.ApplyChanges();
                           display.viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                           return null;
                       }, MISP.Arguments.Arg("fullscreen"), MISP.Arguments.Arg("width"), MISP.Arguments.Arg("height"));
                        
                   engine.AddFunction("console-mode", "Set the console mode",
                        (context, arguments) =>
                        {
                            console.noEcho = true;
                            var viewport = display.viewport;
                            display = new TextDisplay(
                                MISP.AutoBind.IntArgument(arguments[0]),
                                MISP.AutoBind.IntArgument(arguments[1]),
                                GraphicsDevice, Content);
                            dynamicConsole.Reset(display);
                            display.viewport = viewport;

                            return null;
                        },
                        MISP.Arguments.Arg("columns"), MISP.Arguments.Arg("rows"));

                    engine.AddFunction("console-viewport", "Set the console viewport.",
                        (context, arguments) =>
                            {
                                display.viewport = new Viewport(
                                    MISP.AutoBind.IntArgument(arguments[0]),
                                    MISP.AutoBind.IntArgument(arguments[1]),
                                    MISP.AutoBind.IntArgument(arguments[2]),
                                    MISP.AutoBind.IntArgument(arguments[3]));
                                return null;
                            },
                            MISP.Arguments.Arg("x"), MISP.Arguments.Arg("y"),
                            MISP.Arguments.Arg("w"), MISP.Arguments.Arg("h"));

                   engine.AddFunction("clear", "Clear the screen.",
                       (context, arguments) =>
                       {
                           console.noEcho = true;
                           dynamicConsole.Clear();
                           return null;
                       });

                   engine.AddFunction("new-scene", "Create a new scene", (context, arguments) =>
                       {
                           return new Scene(new BoundingBox(new Vector3(-100000, -100000, -100000), new Vector3(100000, 100000, 100000)), 25);
                       });

                   engine.AddFunction("video-viewport", "Set the 3d viewport.",
                       (context, arguments) =>
                       {
                           renderer.viewport = new Viewport(
                               MISP.AutoBind.IntArgument(arguments[0]),
                               MISP.AutoBind.IntArgument(arguments[1]),
                               MISP.AutoBind.IntArgument(arguments[2]),
                               MISP.AutoBind.IntArgument(arguments[3]));
                           return null;
                       },
                           MISP.Arguments.Arg("x"), MISP.Arguments.Arg("y"),
                           MISP.Arguments.Arg("w"), MISP.Arguments.Arg("h"));

                   engine.AddFunction("video-clear", "Set the video clear color.",
                       (context, arguments) =>
                       {
                           if (arguments.Count == 3)
                               clearColor = new Vector3(MISP.AutoBind.NumericArgument(arguments[0]),
                                   MISP.AutoBind.NumericArgument(arguments[1]),
                                   MISP.AutoBind.NumericArgument(arguments[2]));
                           else
                               clearColor = Gem.Math.MispBinding.Vector3Argument(arguments[0]);
                           return null;
                       },
                           MISP.Arguments.Arg("r"), MISP.Arguments.Optional("g"), MISP.Arguments.Optional("b"));
                   
                });

            foreach (var arg in args)
                console.ExecuteCommand(arg);

            handler = new XnaTextInput.TextInputHandler(Window.Handle);
            int ctrlModifier = 0;
            int shiftModifier = 0;

            handler.KeyDown += (_handler, key) =>
                {
                    handlerMutex.WaitOne();

                    if (key.KeyValue == (int)System.Windows.Forms.Keys.ControlKey)
                        ctrlModifier = 1;
                    else if (key.KeyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                        shiftModifier = 1;
                    else if (key.KeyCode == System.Windows.Forms.Keys.Right && ctrlModifier > 0)
                    {
                        if (dynamicConsole.activeInput == dynamicConsole.inputs[0])
                            dynamicConsole.activeInput = dynamicConsole.inputs[1];
                        else
                            dynamicConsole.activeInput = dynamicConsole.inputs[0];
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Up && ctrlModifier > 0)
                    {
                        dynamicConsole.outputScrollPoint += 1;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Down && ctrlModifier > 0)
                    {
                        dynamicConsole.outputScrollPoint -= 1;
                        if (dynamicConsole.outputScrollPoint < 0) dynamicConsole.outputScrollPoint = 0;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Up && shiftModifier > 0)
                    {
                        if (commandRecallBuffer.Count != 0)
                        {
                            recallBufferPlace -= 1;
                            if (recallBufferPlace < 0) recallBufferPlace = commandRecallBuffer.Count - 1;
                            dynamicConsole.activeInput.cursor = 0;
                            dynamicConsole.activeInput.input = commandRecallBuffer[recallBufferPlace];
                        }
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Down && shiftModifier > 0)
                    {
                        if (commandRecallBuffer.Count != 0)
                        {
                            recallBufferPlace += 1;
                            if (recallBufferPlace >= commandRecallBuffer.Count) recallBufferPlace = 0;
                            dynamicConsole.activeInput.cursor = 0;
                            dynamicConsole.activeInput.input = commandRecallBuffer[recallBufferPlace];
                        }
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Up && ctrlModifier == 0)
                    {
                        dynamicConsole.activeInput.cursor -= display.width;
                        if (dynamicConsole.activeInput.cursor < 0) dynamicConsole.activeInput.cursor += display.width;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Down && ctrlModifier == 0)
                    {
                        dynamicConsole.activeInput.cursor += display.width;
                        if (dynamicConsole.activeInput.cursor > dynamicConsole.activeInput.input.Length)
                            dynamicConsole.activeInput.cursor = dynamicConsole.activeInput.input.Length;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Left && ctrlModifier == 0)
                    {
                        dynamicConsole.activeInput.cursor -= 1;
                        if (dynamicConsole.activeInput.cursor < 0) dynamicConsole.activeInput.cursor = 0;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Right && ctrlModifier == 0)
                    {
                        dynamicConsole.activeInput.cursor += 1;
                        if (dynamicConsole.activeInput.cursor > dynamicConsole.activeInput.input.Length)
                            dynamicConsole.activeInput.cursor = dynamicConsole.activeInput.input.Length;
                    }
                    else if (key.KeyCode == System.Windows.Forms.Keys.Delete && ctrlModifier == 0)
                    {
                        var front = dynamicConsole.activeInput.cursor;
                        var sofar = dynamicConsole.activeInput.input.Substring(0, front);
                        var back = dynamicConsole.activeInput.input.Length - dynamicConsole.activeInput.cursor - 1;
                        if (back > 0) sofar += dynamicConsole.activeInput.input.Substring(dynamicConsole.activeInput.cursor + 1, back);
                        dynamicConsole.activeInput.input = sofar;
                    }

                    handlerMutex.ReleaseMutex();
                };

            handler.KeyUp += (_handler, key) =>
                {
                    handlerMutex.WaitOne();

                    if (key.KeyValue == (int)System.Windows.Forms.Keys.ControlKey)
                        ctrlModifier = 0;
                    else if (key.KeyValue == (int)System.Windows.Forms.Keys.ShiftKey)
                        shiftModifier = 0;

                    handlerMutex.ReleaseMutex();
                };

            handler.KeyPress += (_handler, key) =>
                {
                    handlerMutex.WaitOne();

                    if (ctrlModifier > 0)
                    {
                        if (key.KeyChar == '\n')
                        {
                            dynamicConsole.Write(dynamicConsole.activeInput.input + "\n");
                            var s = dynamicConsole.activeInput.input;
                            dynamicConsole.activeInput.input = "";
                            dynamicConsole.outputScrollPoint = 0;
                            dynamicConsole.activeInput.cursor = 0;
                            dynamicConsole.activeInput.scroll = 0;
                            commandRecallBuffer.Add(s);
                            recallBufferPlace = commandRecallBuffer.Count;
                            console.ExecuteCommand(s);
                        }
                    }
                    else
                    {
                        if (key.KeyChar == (char)System.Windows.Forms.Keys.Enter)
                        {
                            var newPosition = (int)Math.Ceiling((float)(dynamicConsole.activeInput.cursor + 1) / display.width) 
                                * display.width;
                            if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                                dynamicConsole.activeInput.input =
                                    dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor,
                                    new String(' ', newPosition - dynamicConsole.activeInput.cursor));
                            else
                                dynamicConsole.activeInput.input += new String(' ', newPosition - dynamicConsole.activeInput.cursor);
                            dynamicConsole.activeInput.cursor = newPosition;
                        }
                        else if (key.KeyChar == (char)System.Windows.Forms.Keys.Tab)
                        {
                            var newPosition = (int)Math.Ceiling((float)(dynamicConsole.activeInput.cursor + 1) / 4) * 4;
                            if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                                dynamicConsole.activeInput.input =
                                    dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor,
                                    new String(' ', newPosition - dynamicConsole.activeInput.cursor));
                            else
                                dynamicConsole.activeInput.input += new String(' ', newPosition - dynamicConsole.activeInput.cursor);
                            dynamicConsole.activeInput.cursor = newPosition;
                        }
                        else if (key.KeyChar == (char)System.Windows.Forms.Keys.Back)
                        {
                            if (dynamicConsole.activeInput.cursor > 0)
                            {
                                var front = dynamicConsole.activeInput.cursor - 1;
                                var sofar = dynamicConsole.activeInput.input.Substring(0, front);
                                var back = dynamicConsole.activeInput.input.Length - dynamicConsole.activeInput.cursor;
                                if (back > 0) sofar += 
                                    dynamicConsole.activeInput.input.Substring(dynamicConsole.activeInput.cursor, back);
                                dynamicConsole.activeInput.input = sofar;
                                dynamicConsole.activeInput.cursor -= 1;
                            }
                        }
                        else
                        {
                            if (dynamicConsole.activeInput.cursor < dynamicConsole.activeInput.input.Length)
                                dynamicConsole.activeInput.input =
                                    dynamicConsole.activeInput.input.Insert(dynamicConsole.activeInput.cursor, new String(key.KeyChar, 1));
                            else
                                dynamicConsole.activeInput.input += key.KeyChar;
                            dynamicConsole.activeInput.cursor += 1;
                        }
                    }

                    handlerMutex.ReleaseMutex();
                };
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) this.Exit();

            input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            inputModule.HandleInput(input);
            events.Swap();
            foreach (var obj in events)
            {
                handlerMutex.WaitOne();
                console.ExecuteCode(obj);
                handlerMutex.ReleaseMutex();
            }
            events.ClearFront();

            handlerMutex.WaitOne();
            if (scene != null) scene.Update();
            handlerMutex.ReleaseMutex();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            handlerMutex.WaitOne();

            if (scene != null)
                input.MouseObject = renderer.MousePick(scene, new Vector2(input.MouseX, input.MouseY));

            GraphicsDevice.Clear(new Color(clearColor));

            dynamicConsole.PopulateDisplay();
            display.Draw(GraphicsDevice);

            if (scene != null)
                renderer.Draw(scene);

            handlerMutex.ReleaseMutex();
            base.Draw(gameTime);
        }
    }
}
