using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OSM
{
    public static class Import
    {
        public static void ImportRawOSM(String filename, Action<Entry> entryHandler)
        {
            var reader = new XmlTextReader(filename);
            reader.ReadToFollowing("osm");
            reader.MoveToContent();

            while (reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.Read();
                    continue;
                }

                var type = reader.Name;
                if (type == "node")
                {
                    var id = Int64.Parse(reader.GetAttribute("id"));
                    var lat = Single.Parse(reader.GetAttribute("lat"));
                    var lon = Single.Parse(reader.GetAttribute("lon"));

                    entryHandler(new Node { id = id, lat = lat, lon = lon });
                    reader.Read();
                }
                else if (type == "way")
                {
                    var id = Int64.Parse(reader.GetAttribute("id"));
                    var nodeChain = new List<Int64>();
                    String name = null;

                    while (reader.ReadState == ReadState.Interactive)
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "way")
                        {
                            entryHandler(new Way { id = id, name = (name ?? "").ToUpper(), nodes = nodeChain });
                            reader.Read();
                            goto outerLoop;
                        }

                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            reader.Read();
                            continue;
                        }

                        if (reader.Name == "nd")
                            nodeChain.Add(Int64.Parse(reader.GetAttribute("ref")));
                        else if (reader.Name == "tag")
                        {
                            var k = reader.GetAttribute("k");
                            if (k == "name") name = reader.GetAttribute("v");
                        }
                        reader.Read();
                    }

                }
                else
                {
                    reader.Read();
                }
            outerLoop: ;
            }
        }
    }
}
