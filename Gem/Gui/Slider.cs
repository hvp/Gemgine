using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Gem.Gui
{
    public class VerticalSlider : UIItem
    {
        public VerticalSlider(Rectangle rect)
            : base(rect)
        {
        }

        private int position = 0;
        private bool draggingHandle = false;
        private int mouseStart = 0;
        private int startPosition = 0;

        private int size { get { return rect.Height - GetIntegerSetting("handle-size", 8); } }
        private int spaces { get { return (maximum - minimum) / step; } }
        private float spaceSize { get { return (float)size / (float)spaces; } }
        public int handlePosition { get { return (int)(((position - minimum) / step) * spaceSize) + (handle_size / 2); } }
        public int step { get { return GetIntegerSetting("step", 1); } }
        public int maximum { get { return GetIntegerSetting("maximum", 100); } }
        public int minimum { get { return GetIntegerSetting("minimum", 0); } }
        public int handle_size { get { return GetIntegerSetting("handle-size", 8); } }
        public int get_position() { return position; }

        public override void Update(Input input, GuiModule module)
        {
            base.Update(input, module);

            if (input.MousePressed())
            {
                if (rect.Contains(input.MouseX, input.MouseY))
                {
                    var mouseOffset = input.MouseY - rect.Y - (handle_size / 2);

                    mouseStart = input.MouseY;

                    var hitPlace = mouseOffset + (spaceSize / 2);
                    var hitSpace = hitPlace / spaceSize;
                    position = minimum + (int)(hitSpace * step);

                    draggingHandle = true;
                    startPosition = position;

                    if (settings["on-change"] != null)
                        module.ButtonEvent(settings["on-change"], this);
                }
            }

            if (input.MouseDown() && draggingHandle)
            {
                var mouseDelta = input.MouseY - mouseStart;

                var stepsPassed = mouseDelta / spaceSize;
                position = startPosition + (int)(stepsPassed * step);

                if (position < minimum) position = minimum;
                if (position > maximum) position = maximum;

                if (settings["on-change"] != null)
                    module.ButtonEvent(settings["on-change"], this);
            }

            if (input.MouseReleased())
            {
                draggingHandle = false;
            }
        }

        public override void Render(Renderer.RenderContext2D context)
        {
            base.Render(context);
            if (Visible)
            {
                context.Texture = context.White;
                context.Color = (GetSetting("fg-color", Vector3.Zero) as Vector3?).Value;

                var halfWidth = rect.Width / 2;
                var handleSize = this.handle_size;

                context.Quad(rect.X + halfWidth - 1, rect.Y, 2, rect.Height);
                context.Quad(rect.X, rect.Y + handlePosition - (handleSize / 2), rect.Width, handleSize);
            }
        }

    }

}