using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Gem.Gui
{
    public class UIItem
    {
        public Rectangle rect;
        public List<UIItem> children = new List<UIItem>();
        public UIItem parent;
        public int id;
        public bool Visible = true;
        public MISP.ScriptObject defaults;
        public MISP.ScriptObject settings;
        public MISP.ScriptObject hoverSettings;
        public bool Hover { get; set; }
        
        public UIItem root { 
            get { if (parent == null) return this;
                return parent.root; }}

        public UIItem(Rectangle rect)
        {
            this.rect = rect;
            Hover = false;
        }

        public virtual void Update(Input input)
        { 
            Hover = input.MouseInside(rect);
            if (Visible) 
                foreach (var child in children) child.Update(input);
        }

        public virtual void KeyboardEvent(System.Windows.Forms.KeyPressEventArgs args)
        {

        }
		
		public virtual void AddChild(UIItem child)
		{
			children.Add(child);
            child.defaults = defaults;
			child.parent = this;
		}
		
		public virtual void RemoveChild(UIItem child)	
        {
            children.Remove(child);
        }
		
		public void Destroy()
		{
			if (parent != null)
				parent.RemoveChild(this);
			parent = null;
		}

        private Object GetSetting(String name, Object _default)
        {
            if (Hover && hoverSettings != null && hoverSettings[name] != null) return hoverSettings[name];
            if (settings != null && settings[name] != null) return settings[name];
            if (defaults != null && defaults[name] != null) return defaults[name];
            return _default;
        }
		
		public virtual void Render(Renderer.RenderContext2D context) 
		{
            if (Visible)
            {
                if (GetSetting("hidden-container", null) == null)
                {
                    context.Texture = context.White;
                    context.Color = (GetSetting("bg-color", Vector3.One) as Vector3?).Value;
                    context.Quad(rect);

                    var label = GetSetting("label", null);
                    var font = GetSetting("font", null);
                    if (label != null && font != null)
                    {
                        context.Color = (GetSetting("text-color", Vector3.Zero) as Vector3?).Value;
                        BitmapFont.RenderText(label.ToString(), rect.X, rect.Y, rect.Width + rect.X,
                            context, font as BitmapFont);
                    }
                }

                foreach (var child in children)
                    child.Render(context);
            }
        }

	}

}