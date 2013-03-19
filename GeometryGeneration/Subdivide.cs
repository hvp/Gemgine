//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;

//namespace GeometryGeneration
//{
//    public partial class MeshPart
//    {
//        internal class Edge
//        {
//            public short S;
//            public short E;
//            public short C;

//            internal Edge(short S, short E)
//            {
                
//                this.S = S;
//                this.E = E;
//            }
//        }

//        internal static bool CompareEdges(Edge A, Edge B)
//        {
//            if (A.S == B.S && A.E == B.E) return true;
//            if (A.E == B.S && A.S == B.E) return true;
//            return false;
//        }
        
//        public MeshPart Subdivide()
//        {
//            var edges = new List<Edge>();
            
//            var newIndicies = new short[indicies.Length * 4];
//            var vertsAdded = 0;
//            var triEdges = new Edge[3];
//            for (int tri = 0; tri < indicies.Length / 3; ++tri)
//            {
//                triEdges[0] = new Edge(indicies[tri * 3], indicies[tri * 3 + 1]);
//                triEdges[1] = new Edge(indicies[tri * 3 + 1], indicies[tri * 3 + 2]);
//                triEdges[2] = new Edge(indicies[tri * 3 + 2], indicies[tri * 3]);

//                foreach (var edge in triEdges)
//                {
//                    var existingEdge = edges.FirstOrDefault((e) => { return CompareEdges(e, edge); });
//                    if (existingEdge != null)
//                    {
//                        edge.C = existingEdge.C;
//                    }
//                    else
//                    {
//                        edges.Add(edge);
//                        edge.C = (short)(verticies.Length + vertsAdded);
//                        vertsAdded += 1;
//                    }
//                }

//                /*
//                 *          E S
//                 *         /   \
//                 *        /     \
//                 *    2  C ----  C    0
//                 *      / \     / \
//                 *     /   \   /   \
//                 *    S     \ /     E
//                 *      E----C----S
//                 *           1
//                 */           

//                CopyIndicies(newIndicies, tri * 3 * 4, new short[] {
//                    triEdges[0].S, triEdges[0].C, triEdges[2].C,
//                    triEdges[0].C, triEdges[0].E, triEdges[1].C,
//                    triEdges[1].C, triEdges[1].E, triEdges[2].C,
//                    triEdges[0].C, triEdges[1].C, triEdges[2].C
//                });
//            }

//            var newVerticies = new Vertex[verticies.Length + edges.Count];
//            for (int i = 0; i < verticies.Length; ++i) newVerticies[i] = verticies[i];

//            foreach (var edge in edges)
//            {
//                newVerticies[edge.C] = verticies[edge.S];
//                newVerticies[edge.C].Position = (verticies[edge.S].Position + verticies[edge.E].Position) / 2;
//            }

//            var r = new MeshPart();
//            r.verticies = newVerticies;
//            r.indicies = newIndicies;
//            return r;
//        }
//    }
//}