﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class Component
    {
        public UInt32 EntityID { get; set; }
        public UInt32 SyncID { get; set; }
        public virtual void AssociateSiblingComponents(IEnumerable<Component> siblings) { }
    }
}
