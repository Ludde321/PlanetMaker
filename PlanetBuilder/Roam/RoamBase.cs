
using System;
using System.Collections.Generic;
using System.IO;

namespace PlanetBuilder.Roam
{
    public abstract class RoamBase
    {
        // public SimpleList<RoamDiamond> Diamonds;
        // public SimpleList<RoamTriangle> Triangles;
        // public SimpleList<RoamVertex> Vertexes;

        // public SimpleList<RoamDiamond> FreeDiamonds;
        // public SimpleList<RoamTriangle> FreeTriangles;
        // public SimpleList<RoamVertex> FreeVertexes;

        public readonly SimpleList<RoamDiamond> ActiveDiamonds = new RoamDiamond();
        public readonly SimpleList<RoamTriangle> ActiveTriangles = new RoamTriangle();
        public readonly SimpleList<RoamVertex> ActiveVertexes = new RoamVertex();

        // public SimpleList<RoamDiamond> InactiveDiamonds;
        public readonly SimpleList<RoamTriangle> InactiveTriangles = new RoamTriangle();

        protected RoamVertex AllocVertex()
        {
            var vertex = new RoamVertex();
            vertex.InsertBefore(ActiveVertexes);
            return vertex;
        }
        protected RoamTriangle AllocTriangle()
        {
            var triangle = new RoamTriangle();
            triangle.InsertBefore(ActiveTriangles);
            return triangle;
        }
        protected RoamDiamond AllocDiamond()
        {
            var diamond = new RoamDiamond();
            diamond.InsertBefore(ActiveDiamonds);
            return diamond;
        }

        protected void FreeVertex(RoamVertex vertex)
        {
            vertex.Remove();
        }

        protected void FreeTriangle(RoamTriangle triangle)
        {
            triangle.Remove();
        }

        protected void FreeDiamond(RoamDiamond diamond)
        {
            diamond.Remove();
        }

        //        void initVertexes(RoamTexture* textures[6]);
        //        void initTriangles();
        //        virtual bool subdivideTriangle(RoamTriangle* triangle, bool split) = 0;

        //    virtual void computeVertexAltitude(RoamVertex* vertex, const Vec3d &normal) = 0;
        //    virtual void computeVertexAltitude(RoamVertex* vertex, const RoamTriangle* triangle) = 0;
        //    virtual double computeVertexAltitude(const Vec3d &normal) = 0;

        public void Split()
        {
            var triangle = ActiveTriangles.NextNode;
            while (triangle != ActiveTriangles)
            {
                var next = triangle.NextNode;

                triangle.Flags |= RoamTriangleFlags.Visible;

                if (triangle.Flags.HasFlag(RoamTriangleFlags.ForceSplit) || SubdivideTriangle(triangle))
                {
                    var opposite = triangle.BaseNeighbor;

                    if (opposite.BaseNeighbor != triangle)
                    {
                        triangle.Flags |= RoamTriangleFlags.ForceSplit;
                        opposite.Flags |= RoamTriangleFlags.ForceSplit;
                        opposite.InsertBefore(triangle);
                        triangle = opposite;
                        continue;
                    }

                    triangle.Flags &= ~RoamTriangleFlags.ForceSplit;

                    // Compute new texture coords
                    Vector2d texCoordTriangle = Vector2d.MiddlePoint(triangle.TextureCoords0, triangle.TextureCoords2);
                    Vector2d texCoordOpposite = Vector2d.MiddlePoint(opposite.TextureCoords0, opposite.TextureCoords2);

                    var vertex = AllocVertex();

                    // Compute vertex
                    ComputeVertexAltitude(vertex, triangle);

                    // 4 x allocTriangle();
                    var tri0 = AllocTriangle();
                    var tri1 = AllocTriangle();
                    var tri2 = AllocTriangle();
                    var tri3 = AllocTriangle();

                    // Triangle 0
//                    tri0.Set(triangle, 2, 1, vertex, texCoordTriangle);
                    tri0.Set(triangle, triangle.Vertexes2, vertex, triangle.Vertexes1, triangle.TextureCoords2, texCoordTriangle, triangle.TextureCoords1);
                    tri0.SetNeighbors(triangle.LeftNeighbor, tri1, tri3);
                    tri0.BaseNeighbor.ReplaceNeighbor(triangle, tri0);

                    // Triangle 1
//                    tri1.Set(triangle, 1, 0, vertex, texCoordTriangle);
                    tri1.Set(triangle, triangle.Vertexes1, vertex, triangle.Vertexes0, triangle.TextureCoords1, texCoordTriangle, triangle.TextureCoords0);
                    tri1.SetNeighbors(triangle.RightNeighbor, tri2, tri0);
                    tri1.BaseNeighbor.ReplaceNeighbor(triangle, tri1);

                    // Triangle 2
//                    tri2.Set(opposite, 2, 1, vertex, texCoordOpposite);
                    tri2.Set(opposite, opposite.Vertexes2, vertex, opposite.Vertexes1, opposite.TextureCoords2, texCoordTriangle, opposite.TextureCoords1);
                    tri2.SetNeighbors(opposite.LeftNeighbor, tri3, tri1);
                    tri2.BaseNeighbor.ReplaceNeighbor(opposite, tri2);

                    // Triangle 3
//                    tri3.Set(opposite, 1, 0, vertex, texCoordOpposite);
                    tri3.Set(opposite, opposite.Vertexes1, vertex, opposite.Vertexes0, opposite.TextureCoords1, texCoordTriangle, opposite.TextureCoords0);
                    tri3.SetNeighbors(opposite.RightNeighbor, tri0, tri2);
                    tri3.BaseNeighbor.ReplaceNeighbor(opposite, tri3);

                    // Remove old diamonds
                    var d0 = triangle.Diamond;
                    if (d0 != null)
                    {
                        d0.ReleaseTriangles();
                        FreeDiamond(d0);
                    }
                    var d1 = opposite.Diamond;
                    if (d1 != null)
                    {
                        d1.ReleaseTriangles();
                        FreeDiamond(d1);
                    }

                    // Update lists
                    opposite.InsertBefore(InactiveTriangles);

                    next = triangle.NextNode;

                    triangle.InsertBefore(InactiveTriangles);

                    // Create new diamond
                    var diamond = AllocDiamond();
                    diamond.SetTriangles(tri0, tri1, tri2, tri3);
                }
                triangle = next;
            }
        }

        protected abstract bool MergeDiamond(RoamDiamond diamond);
        protected abstract bool SubdivideTriangle(RoamTriangle triangle);

        public void Merge()
        {
            var diamond = ActiveDiamonds.NextNode;
            while (diamond != ActiveDiamonds)
            {
                var next = diamond.NextNode;

                var triangle = diamond.Triangles0.Parent;
                var opposite = diamond.Triangles2.Parent;

                if(MergeDiamond(diamond))
                {
                    var tri0 = diamond.Triangles0;
                    var tri1 = diamond.Triangles1;
                    var tri2 = diamond.Triangles2;
                    var tri3 = diamond.Triangles3;

                    // Update neighbors
                    triangle.SetNeighbors(opposite, tri0.BaseNeighbor, tri1.BaseNeighbor);
                    opposite.SetNeighbors(triangle, tri2.BaseNeighbor, tri3.BaseNeighbor);

                    tri0.BaseNeighbor.ReplaceNeighbor(tri0, triangle);
                    tri1.BaseNeighbor.ReplaceNeighbor(tri1, triangle);
                    tri2.BaseNeighbor.ReplaceNeighbor(tri2, opposite);
                    tri3.BaseNeighbor.ReplaceNeighbor(tri3, opposite);

                    // Update lists
                    triangle.InsertBefore(ActiveTriangles);
                    opposite.InsertBefore(ActiveTriangles);

                    triangle.Diamond = null;
                    opposite.Diamond = null;

                    RoamTriangle t0, t1, t2, t3;

                    // Create new diamond 1
                    if (triangle.Level > 0)
                    {
                        t0 = triangle;
                        t1 = t0.LeftNeighbor;
                        t2 = t1.LeftNeighbor;
                        t3 = t2.LeftNeighbor;

                        if (t3.LeftNeighbor == t0)
                        {
                            if (t0.Parent == t1.Parent && t2.Parent == t3.Parent)
                            {
                                var d = AllocDiamond();
                                d.SetTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.Parent == t2.Parent && t3.Parent == t0.Parent)
                            {
                                var d = AllocDiamond();
                                d.SetTriangles(t1, t2, t3, t0);
                            }
                        }
                    }
                    // Create new diamond 2
                    if (opposite.Level > 0)
                    {
                        t0 = opposite;
                        t1 = t0.LeftNeighbor;
                        t2 = t1.LeftNeighbor;
                        t3 = t2.LeftNeighbor;

                        if (t3.LeftNeighbor == t0)
                        {
                            if (t0.Parent == t1.Parent && t2.Parent == t3.Parent)
                            {
                                var d = AllocDiamond();
                                d.SetTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.Parent == t2.Parent && t3.Parent == t0.Parent)
                            {
                                var d = AllocDiamond();
                                d.SetTriangles(t1, t2, t3, t0);
                            }
                        }
                    }

                    // Free vertex and triangles
                    var vertex = tri0.Vertexes1;

                    FreeVertex(vertex);

                    // 4 x freeTriangle();
                    FreeTriangle(tri0);
                    FreeTriangle(tri1);
                    FreeTriangle(tri2);
                    FreeTriangle(tri3);

                    next = diamond.NextNode;

                    FreeDiamond(diamond);
                }
                diamond = next;
            }
        }


        protected virtual void ComputeVertexAltitude(RoamVertex vertex, Vector3d normal)
        {
            vertex.LinearPosition = normal;
            vertex.Normal = normal;
            vertex.Position = normal;
        }
        protected virtual void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
        {
            vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes0.Position, triangle.Vertexes2.Position);
            vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

            vertex.Position = vertex.LinearPosition;//vertex.Normal * (groundRadius + vertex.altitude);
        }

        protected void InitTriangles()
        {
            // Compute neighbor relationships
            var t0 = ActiveTriangles.NextNode;
            for (; t0 != ActiveTriangles; t0 = t0.NextNode)
            {
                var t1 = ActiveTriangles.NextNode;
                for (; t1 != ActiveTriangles; t1 = t1.NextNode)
                {
                    if (t0 == t1) continue;

                    if (t1.HasVertex(t0.Vertexes0) && t1.HasVertex(t0.Vertexes1))
                        t0.RightNeighbor = t1;
                    else if (t1.HasVertex(t0.Vertexes1) && t1.HasVertex(t0.Vertexes2))
                        t0.LeftNeighbor = t1;
                    else if (t1.HasVertex(t0.Vertexes2) && t1.HasVertex(t0.Vertexes0))
                        t0.BaseNeighbor = t1;
                }
                t0.Flags |= RoamTriangleFlags.Modified;
            }
        }


        public void PrintSummary()
        {
            Console.WriteLine($"NumVertexes: {ActiveVertexes.Count()}");
            Console.WriteLine($"NumTriangles: {ActiveTriangles.Count()}");
            Console.WriteLine($"NumDiamonds: {ActiveDiamonds.Count()}");
        }

        public void SaveStl(string filename)
        {
            using(var stlWriter = new StlWriter(File.Create(filename)))
            {
                var triangle = ActiveTriangles.NextNode;
                for (; triangle != ActiveTriangles;triangle = triangle.NextNode)
                {
                    stlWriter.AddTriangle(triangle.Vertexes0.Position, triangle.Vertexes1.Position, triangle.Vertexes2.Position);
                }
            }
            
        }

    }
}