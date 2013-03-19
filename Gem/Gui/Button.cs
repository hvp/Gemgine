using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Gui
{
    public class Button : UIItem
    {
        public Button(Rectangle rect) : base(rect)
        {
            Hover = false;
        }

        public override void Update(Input input)
        {
            Hover = input.MouseInside(rect);
            if (Hover && input.MousePressed())
            {
                if (settings["onClick"] != null) return; // onClick(this);
                input.MouseHandled = true;
            }
        }
    }
}
