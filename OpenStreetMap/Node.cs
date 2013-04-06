using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MISP;
using Microsoft.Xna.Framework;

namespace OSM
{
    public class Node : Entry
    {
        public float lon;
        public float lat;

        public Node() { }

        public Node(RawEntry raw)
        {
            id = raw.id;
            name = raw.name;

            var parts = raw.value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            lon = float.Parse(parts[0]);
            lat = float.Parse(parts[1]);
        }

        public override void FillRawEntry(RawEntry raw)
        {
            raw.id = id;
            raw.name = name;
            raw.value = lon.ToString() + " " + lat.ToString();
            raw.type = RawEntry.TYPE_NODE;
        }

        public Vector2 AsVector() { return new Vector2(lon, lat); }
    }
}
