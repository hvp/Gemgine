using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Epiphany.Render
{
    public partial class CompiledMesh: IDisposable
    {
        public VertexBuffer verticies;
        public IndexBuffer indicies;
        public int primitiveCount;
                
        public void Dispose()
        {
            verticies.Dispose();
            indicies.Dispose();
        }
        
        public static CompiledMesh CompileMesh(Mesh mesh, GraphicsDevice device)
        {
            var result = new CompiledMesh();

            result.indicies = new IndexBuffer(device, typeof(Int16), mesh.indicies.Length, BufferUsage.WriteOnly);
            result.indicies.SetData(mesh.indicies);

            result.verticies = new VertexBuffer(device, typeof(VertexPositionColorTexture), mesh.verticies.Length, BufferUsage.WriteOnly);
            result.verticies.SetData(mesh.verticies);

            result.primitiveCount = mesh.indicies.Length / 3;
            return result;
        }
    }
}
