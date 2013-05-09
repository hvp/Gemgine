using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Epiphany.Render
{
    public interface ISceneNode
    {
        void UpdateWorldTransform(Matrix m);
        void Draw(Renderer renderer);
    }
}
