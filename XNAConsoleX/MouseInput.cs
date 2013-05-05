using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace XNAConsole
{
    public class MouseInput
    {
        private MouseState previousMouseState;
        private MouseState currentMouseState;

        public int MouseX { get { return currentMouseState.X; } }
        public int MouseY { get { return currentMouseState.Y; } }

        public bool MouseHandled { get; set; }
        public UInt32 MouseObject { get; set; }

        public void Update(float ElapsedSeconds)
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            MouseHandled = false;
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
