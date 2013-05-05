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
        public static Mesh CreateCube()
        {
            var result = new Mesh();
            result.verticies = new Vertex[8];
            result.verticies[0].Position = new Vector3( -0.5f, -0.5f, -0.5f);
            result.verticies[1].Position = new Vector3(  0.5f, -0.5f, -0.5f);
            result.verticies[2].Position = new Vector3(  0.5f,  0.5f, -0.5f);
            result.verticies[3].Position = new Vector3( -0.5f,  0.5f, -0.5f);

            result.verticies[4].Position = new Vector3( -0.5f, -0.5f,  0.5f);
            result.verticies[5].Position = new Vector3(  0.5f, -0.5f,  0.5f);
            result.verticies[6].Position = new Vector3(  0.5f,  0.5f,  0.5f);
            result.verticies[7].Position = new Vector3( -0.5f,  0.5f,  0.5f);

            result.indicies = new short[] { 
                0, 2, 1, 
                3, 2, 0,

                0, 1, 4,
                4, 1, 5,
            
                1, 2, 5,
                5, 2, 6,

                2, 3, 6,
                6, 3, 7,

                3, 0, 7,
                7, 0, 4,

                4, 5, 6,
                7, 4, 6
            };

            return result;
        }
    }
}