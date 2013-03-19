using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MISP;

namespace XNAConsole.StreetData
{
    public class IDList : List<Int64>, IScriptSerializable
    {
        public IDList(IEnumerable<Int64> data) : base(data) { }
        public IDList() { }
        
        public override string  ToString()
        {
            return String.Join(", ", this);
        }

        string IScriptSerializable.Serialize()
        {
            return "(osm.id-list " + String.Join(" ", this) + ")";
        }
    }

    public class Entry
    {
        public Int64 ID;
        public virtual PointF center(Data master) { return new PointF(0, 0); }
        public virtual RectangleF bounds(Data master) { return new RectangleF(0, 0, 0, 0); }
    }

    public class Node : Entry, IScriptSerializable
    {
        public double lon;
        public double lat;

        public override string  ToString()
        {
            return "[N " + ID + " " + lon + "," + lat + "]";
        }

        public override PointF center(Data master)
        {
            return new PointF((float)lon, (float)lat);
        }

        public override RectangleF bounds(Data master)
        {
            return new RectangleF((float)lon, (float)lat, 0, 0);
        }

        string IScriptSerializable.Serialize()
        {
            return "(osm.node " + ID + " " + lon + " " + lat + ")";
        }
    }

    public class Way : Entry, IScriptSerializable
    {
        public IDList nodes;
        public String name;

        public override string ToString()
        {
            return "[W " + ID + " " + name + " " + nodes.ToString() + "]";
        }

        public override RectangleF bounds(Data master)
        {
            if (nodes.Count == 0) return new RectangleF(0, 0, 0, 0);
            var R = master[(int)nodes[0]].bounds(master);
            for (int i = 1; i < nodes.Count; ++i)
                R = RectangleF.Union(R, master[(int)nodes[i]].bounds(master));
            return R;
        }

        public override PointF center(Data master)
        {
            var b = bounds(master);
            return new PointF(b.X + (b.Width / 2), b.Y + (b.Height / 2));
        }

        string IScriptSerializable.Serialize()
        {
            return "(osm.way " + ID + " " + name + " " + String.Join(" ", nodes) + ")";
        }
    }

    public class Data : List<Entry>, IScriptSerializable
    {
        public Data(IEnumerable<Entry> en) : base(en) { }
        public Data() { }
        public Data(Entry e) { Add(e); }

        internal double boundsMinLon;
        internal double boundsMaxLon;
        internal double boundsMinLat;
        internal double boundsMaxLat;

        string IScriptSerializable.Serialize()
        {
            return "(osm.data " + boundsMinLon + " " + boundsMaxLon + " " + boundsMinLat + " " + boundsMaxLat + " " 
                + String.Join("\n", this.Select(e => (e as IScriptSerializable).Serialize())) + ")";
        }
    }

    public static class MispBinding
    {
        public static GenericScriptObject GenerateStaticBinding()
        {
            var result = new GenericScriptObject();

            result.SetProperty("id-list", Function.MakeSystemFunction("id-list",
                Arguments.Args(Arguments.Repeat("id")), "Create an idlist from a list of ids.",
                (context, arguments) =>
                {
                    var r = new IDList();
                    foreach (var arg in arguments) r.Add(AutoBind.UIntArgument(arg));
                    return r;
                }));

            result.SetProperty("node", Function.MakeSystemFunction("node",
                Arguments.Args("id", "lon", "lat"), "Create a node.",
                (context, arguments) =>
                {
                    return new Node
                    {
                        ID = AutoBind.UIntArgument(arguments[0]),
                        lon = AutoBind.NumericArgument(arguments[1]),
                        lat = AutoBind.NumericArgument(arguments[2])
                    };
                }));

            return result;
        }
    }
}
