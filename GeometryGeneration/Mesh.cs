using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeometryGeneration
{
    public class Mesh
    {
        public bool Textured = false;
        public Vertex[] verticies;
        public TexturedVertex[] texturedVerticies;
        public short[] indicies;

        public int VertexCount { get { return Textured ? texturedVerticies.Length : verticies.Length; } }

        public TexturedVertex GetTexturedVertex(int i)
        {
            if (Textured) return texturedVerticies[i];
            else return new TexturedVertex(verticies[i]);
        }

        public Vertex GetVertex(int i)
        {
            if (Textured) return new Vertex(texturedVerticies[i]);
            else return verticies[i];
        }
    }
}