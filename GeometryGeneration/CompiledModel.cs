using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeometryGeneration
{
    public partial class CompiledModel: IDisposable
    {
        public VertexBuffer verticies;
        public IndexBuffer indicies;
        public BoundingSphere boundingSphere;
        public int primitiveCount;

        public void Draw(GraphicsDevice GraphicsDevice)
        {
            GraphicsDevice.SetVertexBuffer(verticies);
            GraphicsDevice.Indices = indicies;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verticies.VertexCount, 
                0, System.Math.Min(primitiveCount, 65535));
        }

        public void Dispose()
        {
            verticies.Dispose();
            indicies.Dispose();
        }

        public static CompiledModel CompileModel(RawModel model, GraphicsDevice device, CompiledModel into = null)
        {
            CompiledModel result = into;

            var combinedVertex = new Vertex[model.TotalVerticies];
            var combinedIndex = new short[model.TotalIndicies];

            int vCount = 0;
            int iCount = 0;
            foreach (var part in model.parts)
            {
                for (int i = 0; i < part.verticies.Length; ++i)
                {
                    combinedVertex[i + vCount] = part.verticies[i];
                }
                for (int i = 0; i < part.indicies.Length; ++i) combinedIndex[i + iCount] = (short)(part.indicies[i] + vCount);
                vCount += part.verticies.Length;
                iCount += part.indicies.Length;
            }

            if (result == null)
            {
                result = new CompiledModel();
                result.verticies = new VertexBuffer(device, typeof(Vertex), combinedVertex.Length, BufferUsage.WriteOnly);
                result.indicies = new IndexBuffer(device, typeof(Int16), combinedIndex.Length, BufferUsage.WriteOnly);
            }
            else
            {
                if (result.verticies.VertexCount < combinedVertex.Length)
                {
                    result.verticies = new VertexBuffer(device, typeof(Vertex), combinedVertex.Length, BufferUsage.WriteOnly);
                }
                if (result.indicies.IndexCount < combinedIndex.Length)
                    result.indicies = new IndexBuffer(device, typeof(Int16), combinedIndex.Length, BufferUsage.WriteOnly);
            }

            device.SetVertexBuffer(null);
            device.Indices = null;
            result.verticies.SetData(combinedVertex);
            result.indicies.SetData(combinedIndex);
            result.primitiveCount = combinedIndex.Length / 3;
            result.boundingSphere = new BoundingSphere(Vector3.Zero, 1);

            //var largestDistance = 0.0f;
            //foreach (var v0 in combinedVertex)
            //    foreach (var v1 in combinedVertex)
            //    {
            //        var v = v1.Position - v0.Position;
            //        if (v.LengthSquared() > largestDistance)
            //        {
            //            largestDistance = v.LengthSquared();
            //            result.boundingSphere = new BoundingSphere((v0.Position + v1.Position) / 2.0f, v.Length() / 2.0f);
            //        }
            //    }

            return result;
        }

    }
}
