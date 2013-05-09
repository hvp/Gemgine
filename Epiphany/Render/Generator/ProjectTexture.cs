using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Epiphany.Render
{
    public partial class Generator
    {
        public static float ScalarProjection(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(A, B) / B.Length();
        }

        //Project texture coordinates onto a mesh
        public static void ProjectTexture(Mesh m, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
        {
            for (int i = 0; i < m.verticies.Length; ++i)
            {
                var pos = m.verticies[i].Position;
                pos -= origin;
                m.verticies[i].TextureCoordinate = new Vector2(
                    ScalarProjection(pos, uAxis),
                    ScalarProjection(pos, vAxis));
            }
        }

        public static Mesh ProjectTextureCopy(Mesh m, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
        {
            var r = Copy(m);
            ProjectTexture(r, origin, uAxis, vAxis);
            return r;
        }

        public static void ProjectTextureOrtho(Mesh m, Vector3 origin)
        {
            ProjectTexture(m, origin, Vector3.UnitX, Vector3.UnitY);
        }

        public static void ProjectTextureFit(Mesh m)
        {
            var box = CalculateBoundingBox(m);
            ProjectTexture(m, box.Min,
                new Vector3(box.Max.X - box.Min.X , 0, 0),
                new Vector3(0, box.Max.Y - box.Min.Y, 0));
        }
    }
}