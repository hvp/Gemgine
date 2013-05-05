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
        public static GenericScriptObject BindXNAMath()
        {
            var xna = new GenericScriptObject();

            xna.SetProperty("v", Function.MakeSystemFunction("Vector3",
                Arguments.Args("x", "y", "z", Arguments.Optional("w")), "Create a new vector.",
                (context, arguments) =>
                {
                    if (arguments[3] == null)
                        return new Vector3(AutoBind.NumericArgument(arguments[0]),
                            AutoBind.NumericArgument(arguments[1]),
                            AutoBind.NumericArgument(arguments[2]));
                    else
                        return new Vector4(AutoBind.NumericArgument(arguments[0]),
                            AutoBind.NumericArgument(arguments[1]),
                            AutoBind.NumericArgument(arguments[2]),
                            AutoBind.NumericArgument(arguments[3]));
                }));

            xna.SetProperty("helper", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(MathHelper)));
            xna.SetProperty("matrix", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Matrix)));
            xna.SetProperty("quat", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Quaternion)));
            xna.SetProperty("v2", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Vector2), true));
            xna.SetProperty("v3", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Vector3), true));
            xna.SetProperty("v4", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Vector4), true));
            xna.SetProperty("rect", AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Rectangle), true));

            return xna;
        }
    }
}
