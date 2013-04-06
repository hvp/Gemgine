using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MISP;
using Microsoft.Xna.Framework;

namespace OSM
{
    public class Entry
    {
        public Int64 id;
        public String name;

        public virtual void FillRawEntry(RawEntry raw) { }
    }

    public class Way : Entry
    {
        public List<Int64> nodes;

        public Way() { }

        public Way(RawEntry raw)
        {
            id = raw.id;
            name = raw.name;
            var parts = raw.value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            nodes = new List<long>();
            foreach (var part in parts)
                nodes.Add(Int64.Parse(part));
        }

        public override void FillRawEntry(RawEntry raw)
        {
            raw.id = id;
            raw.name = name;
            raw.value = String.Join(" ", nodes);
            raw.type = RawEntry.TYPE_WAY;
        }
    }

}
