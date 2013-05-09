using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Epiphany
{
    public class Euler
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Vector3 EulerOrientation = Vector3.Zero;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateTranslation(Position)
                    * Matrix.CreateFromYawPitchRoll(EulerOrientation.X, EulerOrientation.Y, EulerOrientation.Z)
                    * Matrix.CreateScale(Scale);           
            }
        }

        
    }
}
