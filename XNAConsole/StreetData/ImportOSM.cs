using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XNAConsole.StreetData
{
    public static class Import
    {
        public static Data ImportRawOSM(String filename)
        {
            var idMap = new Dictionary<Int64, Int64>();
            var data = new Data();

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
                    var lat = Double.Parse(reader.GetAttribute("lat"));
                    var lon = Double.Parse(reader.GetAttribute("lon"));

                    data.Add(new Node { ID = data.Count, lat = lat, lon = lon });
                    idMap.Add(id, data.Count - 1);
                    reader.Read();

                }
                else if (type == "bound")
                {
                    var str = reader.GetAttribute("box");
                    var parts = str.Split(',');
                    data.boundsMinLat = double.Parse(parts[0]);
                    data.boundsMinLon = double.Parse(parts[1]);
                    data.boundsMaxLat = double.Parse(parts[2]);
                    data.boundsMaxLon = double.Parse(parts[3]);
                    reader.Read();
                }
                else if (type == "way")
                {
                    var id = Int64.Parse(reader.GetAttribute("id"));
                    var nodeChain = new IDList();
                    String name = null;

                    while (reader.ReadState == ReadState.Interactive)
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "way")
                        {
                            for (int i = 0; i < nodeChain.Count; ++i)
                            {
                                if (!idMap.ContainsKey(nodeChain[i]))
                                {
                                    reader.Read();
                                    goto outerLoop;
                                }
                                nodeChain[i] = idMap[nodeChain[i]];
                            }

                            data.Add(new Way { ID = data.Count, name = (name ?? "").ToUpper(), nodes = nodeChain });
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

            return data;
        }
    }
}
