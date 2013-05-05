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
        public bool Textured = false;

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

        public static short[] CompileIndicies(RawModel model)
        {
            var combinedIndex = new short[model.TotalIndicies];
            int vCount = 0;
            int iCount = 0;
            foreach (var part in model.parts)
            {
                for (int i = 0; i < part.indicies.Length; ++i) combinedIndex[i + iCount] = (short)(part.indicies[i] + vCount);
                vCount += part.VertexCount;
                iCount += part.indicies.Length;
            }

            return combinedIndex;
        }

        public static Vertex[] CompileVerticies(RawModel model)
        {
            var combinedVertex = new Vertex[model.TotalVerticies];

            int vCount = 0;
            foreach (var part in model.parts)
            {
                for (int i = 0; i < part.verticies.Length; ++i)
                    combinedVertex[i + vCount] = part.GetVertex(i);
                vCount += part.VertexCount;
            }

            return combinedVertex;
        }

        public static TexturedVertex[] CompileTexturedVerticies(RawModel model)
        {
            var combinedVertex = new TexturedVertex[model.TotalVerticies];

            int vCount = 0;
            foreach (var part in model.parts)
            {
                for (int i = 0; i < part.VertexCount; ++i)
                    combinedVertex[i + vCount] = part.GetTexturedVertex(i);
                vCount += part.VertexCount;
            }

            return combinedVertex;
        }

        public static CompiledModel CompileModel(RawModel model, GraphicsDevice device)
        {
            CompiledModel result = new CompiledModel();

            var combinedIndex = CompileIndicies(model);
            result.indicies = new IndexBuffer(device, typeof(Int16), combinedIndex.Length, BufferUsage.WriteOnly);
            result.indicies.SetData(combinedIndex);

            if (model.IsTextured)
            {
                var combinedVertex = CompileTexturedVerticies(model);
                result.verticies = new VertexBuffer(device, typeof(TexturedVertex), combinedVertex.Length, BufferUsage.WriteOnly);
                result.verticies.SetData(combinedVertex);
                result.Textured = true;
            }
            else
            {
                var combinedVertex = CompileVerticies(model);
                result.verticies = new VertexBuffer(device, typeof(Vertex), combinedVertex.Length, BufferUsage.WriteOnly);
                result.verticies.SetData(combinedVertex);
                result.Textured = false;
            }

            result.primitiveCount = combinedIndex.Length / 3;
            result.boundingSphere = new BoundingSphere(Vector3.Zero, 1);

            return result;
        }

    }
}
