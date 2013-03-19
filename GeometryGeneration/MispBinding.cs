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
            return AutoBind.GenerateLazyBindingObjectForStaticLibrary(typeof(Gen));
        }   
    }
}
