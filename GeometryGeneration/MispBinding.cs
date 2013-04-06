using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISP;

namespace GeometryGeneration
{
    public class MispBinding
    {
        public static RawModel ModelArgument(Object o)
        {
            if (o is RawModel) return o as RawModel;
            if (o is Mesh)
            {
                var m = new RawModel();
                m.AddPart(o as Mesh);
                return m;
            }
            throw new ScriptError("Object isn't a model.", null);
        }

        public static Mesh MeshArgument(Object obj)
        {
            if (obj is Mesh) return obj as Mesh;
            throw new ScriptError("Object is not a mesh.", null);
        }


        public static ScriptObject GenerateBindingObject()
        {
            var r = AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Gen));
            r.SetProperty("merge", Function.MakeSystemFunction("merge",
                Arguments.Args(Arguments.Repeat("mesh")),
                "Merge many models into one.",
                (context, arguments) =>
                {
                    return Gen.Merge(AutoBind.ListArgument(arguments[0]).Select(o => o as Mesh).ToArray());
                }));
            return r;
        }   
    }
}
