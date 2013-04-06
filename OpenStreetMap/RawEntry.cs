using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace OSM
{
    public class RawEntry
    {
        public const int TYPE_NODE = 0;
        public const int TYPE_WAY = 1;

        public Int64 id;
        public int type;
        public String name;
        public String value;

        public Entry GetEntry()
        {
            switch (type)
            {
                case TYPE_NODE:
                    return new Node(this);
                case TYPE_WAY:
                    return new Way(this);
            }
            return null;
        }
    }

}
