using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MISP;

namespace Gem.Math
{
    public partial class MispBinding
    {
        public static Vector3 Vector3Argument(Object obj)
        {
            if (obj is Vector3) return (obj as Vector3?).Value;
            return Vector3.Zero;
        }

        public static Vector4 Vector4Argument(Object obj)
        {
            if (obj is Vector4) return (obj as Vector4?).Value;
            return Vector4.Zero;
        }

        public static Matrix MatrixArgument(Object obj)
        {
            if (obj is Matrix) return (obj as Matrix?).Value;
            return Matrix.Identity;
        }

        public static Quaternion QuaternionArgument(Object obj)
        {
            if (obj is Quaternion) return (obj as Quaternion?).Value;
            return Quaternion.Identity;
        }
    }
}
