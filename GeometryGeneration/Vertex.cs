using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeometryGeneration
{
    public struct Vertex : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float)*7, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public Vertex(TexturedVertex other)
        {
            this.Position = other.Position;
            this.Color = other.Color;
            this.Normal = other.Normal;
        }

        public Vertex(Vertex other)
        {
            this.Position = other.Position;
            this.Color = other.Color;
            this.Normal = other.Normal;
        }
    };

    public struct TexturedVertex : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;
        public Vector2 Texcoord;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public TexturedVertex(TexturedVertex other)
        {
            this.Position = other.Position;
            this.Color = other.Color;
            this.Normal = other.Normal;
            this.Texcoord = other.Texcoord;
        }

        public TexturedVertex(Vertex other)
        {
            this.Position = other.Position;
            this.Color = other.Color;
            this.Normal = other.Normal;
            this.Texcoord = Vector2.Zero;
        }
    };
}
