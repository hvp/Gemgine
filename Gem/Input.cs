using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Gem
{
    public class Input
    {
        private KeyboardState previousState;
        private KeyboardState currentState;

        private Dictionary<String, List<Keys>> bindings = new Dictionary<String, List<Keys>>();

        private MouseState previousMouseState;
        private MouseState currentMouseState;

        internal XnaTextInput.TextInputHandler textHook;

        public int MouseX { get { return currentMouseState.X; } }
        public int MouseY { get { return currentMouseState.Y; } }

        public bool MouseHandled { get; set; }
        public float ElapsedSeconds { get; set; }

        public UInt32 MouseObject { get; set; }

        public Input(IntPtr windowHandle)
        {
            textHook = new XnaTextInput.TextInputHandler(windowHandle);
        }

        public void Update(float ElapsedSeconds)
        {
            previousState = currentState;
            currentState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            MouseHandled = false;

            this.ElapsedSeconds = ElapsedSeconds;
        }

        public void Bind(String name, params Keys[] keys)
        {
            List<Keys> bindlist = null;
            if (!bindings.ContainsKey(name)) bindings.Add(name, new List<Keys>());
            bindlist = bindings[name];
            foreach (var key in keys)
                if (!bindlist.Contains(key)) bindlist.Add(key);
        }

        public bool Check(String name)
        {
            if (!bindings.ContainsKey(name)) return false;
            foreach (var key in bindings[name])
                if (currentState.IsKeyDown(key)) return true;
            return false;
        }

        public bool Check(Keys key)
        {
            if (currentState.IsKeyDown(key)) return true;
            return false;
        }

        public bool Pressed(String name)
        {
            if (!bindings.ContainsKey(name)) return false;
            foreach (var key in bindings[name])
                if (currentState.IsKeyDown(key) && previousState.IsKeyUp(key)) return true;
            return false;
        }

        public bool Released(String name)
        {
            if (!bindings.ContainsKey(name)) return false;
            foreach (var key in bindings[name])
                if (currentState.IsKeyUp(key) && previousState.IsKeyDown(key)) return true;
            return false;
        }

        public bool MouseDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MouseReleased()
        {
            return currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MousePressed()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        }

        public bool MouseMoved
        {
            get { return currentMouseState.X != previousMouseState.X || currentMouseState.Y != previousMouseState.Y; }
        }

        public bool MouseEntered(Microsoft.Xna.Framework.Rectangle rect)
        {
            return !rect.Contains(previousMouseState.X, previousMouseState.Y)
                && rect.Contains(currentMouseState.X, currentMouseState.Y);
        }

        public bool MouseLeft(Microsoft.Xna.Framework.Rectangle rect)
        {
            return rect.Contains(previousMouseState.X, previousMouseState.Y)
                && !rect.Contains(currentMouseState.X, currentMouseState.Y);
        }

        public bool MouseInside(Microsoft.Xna.Framework.Rectangle rect)
        {
            return rect.Contains(currentMouseState.X, currentMouseState.Y);
        }

    }
}
