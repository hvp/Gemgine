using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeometryGeneration
{
    public partial class Gen
    {
        public static Mesh CreateUnitPolygon(int sides)
        {
            var result = new Mesh();
            result.verticies = new Vertex[sides + 1];
            result.verticies[0].Position = new Vector3(0, 0, 0);
            for (int i = 0; i < sides; ++i)
            {
                var matrix = Matrix.CreateRotationZ(MathHelper.ToRadians(-(360.0f/sides) * i));
                result.verticies[i + 1].Position = Vector3.Transform(new Vector3(0, 1, 0.0f), matrix);
            }

            result.indicies = new short[3 * sides];
            for (int i = 0; i < sides; ++i)
            {
                result.indicies[i * 3] = 0;
                result.indicies[(i * 3)+2] = (short)(i + 1);
                result.indicies[(i * 3)+1] = (short)(i == (sides - 1) ? 1 : (i + 2));
            }
            
            return result;
        }

        public static Mesh CreateQuad()
        {
            var result = new Mesh();
            result.verticies = new Vertex[4];
            result.verticies[0].Position = new Vector3(0, 0, 0);
            result.verticies[1].Position = new Vector3(1, 0, 0);
            result.verticies[2].Position = new Vector3(1, 1, 0);
            result.verticies[3].Position = new Vector3(0, 1, 0);

            result.indicies = new short[] { 0, 1, 2, 3, 0, 2 };
            return result;
        }

        public static Mesh CreateTexturedQuad()
        {
            var result = new Mesh();
            result.Textured = true;

            result.texturedVerticies = new TexturedVertex[4];

            result.texturedVerticies[0].Position = new Vector3(-0.5f, -0.5f, 0);
            result.texturedVerticies[1].Position = new Vector3( 0.5f, -0.5f, 0);
            result.texturedVerticies[2].Position = new Vector3( 0.5f,  0.5f, 0);
            result.texturedVerticies[3].Position = new Vector3(-0.5f,  0.5f, 0);

            for (int i = 0; i < 4; ++i)
                result.texturedVerticies[i].Texcoord =
                    new Vector2(result.texturedVerticies[i].Position.X + 0.5f, result.texturedVerticies[i].Position.Y + 0.5f);

            result.indicies = new short[] { 0, 1, 2, 3, 0, 2 };
            return result;
        }
    }
}