using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace XNAConsole
{
    public class OctNode
    {
        internal ModelComponent spacial;
        internal BoundingSphere bounds;
        internal bool boundsChanged = true;
    }
}
