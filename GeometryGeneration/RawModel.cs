using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeometryGeneration
{
    public partial class RawModel
    {
        public List<Mesh> parts = new List<Mesh>();

        public RawModel(Mesh part)
        {
            AddPart(part);
        }

        public RawModel()
        {
        }

        public void AddPart(Mesh part)
        {
            parts.Add(part);
        }

        public void AddParts(RawModel other)
        {
            parts.AddRange(other.parts);
        }

        public void AddParts(IEnumerable<Mesh> parts)
        {
            this.parts.AddRange(parts);
        }

        public int TotalVerticies { get { return parts.Sum((p) => p.verticies.Length); } }
        public int TotalIndicies { get { return parts.Sum((p) => p.indicies.Length); } }
    }
}
