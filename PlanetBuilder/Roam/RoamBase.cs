
using System.Collections.Generic;

namespace PlanetBuilder.Roam
{
    public class RoamBase
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

        // private RoamDiamond AllocDiamond()
        // {
        //     var diamond = FreeDiamonds.First;
        //     FreeDiamonds.RemoveFirst();
        //     ActiveDiamonds.AddFirst(diamond);
        //     return diamond.Value;
        // }

        // private RoamTriangle AllocTriangle()
        // {
        //     var triangle = FreeTriangles.First;
        //     FreeTriangles.RemoveFirst();
        //     ActiveTriangles.AddFirst(triangle);
        //     return triangle.Value;
        // }

        // private RoamVertex AllocVertex()
        // {
        //     var vertex = FreeVertexes.First;
        //     FreeVertexes.RemoveFirst();
        //     ActiveVertexes.AddFirst(vertex);
        //     return vertex.Value;
        // }

        //        void initVertexes(RoamTexture* textures[6]);
        //        void initTriangles();
        //        virtual bool subdivideTriangle(RoamTriangle* triangle, bool split) = 0;

        //    virtual void computeVertexAltitude(RoamVertex* vertex, const Vec3d &normal) = 0;
        //    virtual void computeVertexAltitude(RoamVertex* vertex, const RoamTriangle* triangle) = 0;
        //    virtual double computeVertexAltitude(const Vec3d &normal) = 0;

        public void Update()
        {
            Merge();
            Split();
        }

        public void Split()
        {
            var triangle = ActiveTriangles.NextNode;
            while (triangle != ActiveTriangles)
            {
                var next = triangle.NextNode;

                triangle.Flags |= RoamTriangleFlags.Visible;

                if (triangle.Flags.HasFlag(RoamTriangleFlags.ForceSplit) || triangle.Material.SubdivideTriangle(triangle, true))
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
                    Vector2d texCoordTriangle = Vector2d.MiddlePoint(triangle.TextureCoords[0], triangle.TextureCoords[2]);
                    Vector2d texCoordOpposite = Vector2d.MiddlePoint(opposite.TextureCoords[0], opposite.TextureCoords[2]);

                    // allocVertex();
                    var vertex = new RoamVertex();
                    vertex.InsertBefore(ActiveVertexes);

                    // Compute vertex
                    triangle.Material.ComputeVertexAltitude(vertex, triangle);

                    // 4 x allocTriangle();
                    var tri0 = new RoamTriangle();
                    var tri1 = new RoamTriangle();
                    var tri2 = new RoamTriangle();
                    var tri3 = new RoamTriangle();
                    tri0.InsertBefore(ActiveTriangles);
                    tri1.InsertBefore(ActiveTriangles);
                    tri2.InsertBefore(ActiveTriangles);
                    tri3.InsertBefore(ActiveTriangles);

                    ushort nextLevel = (ushort)(triangle.Level + 1);

                    // Triangle 0
                    tri0.Set(triangle, 2, 1, vertex, texCoordTriangle);
                    tri0.SetNeighbors(triangle.LeftNeighbor, tri1, tri3);
                    tri0.BaseNeighbor.ReplaceNeighbor(triangle, tri0);

                    // Triangle 1
                    tri1.Set(triangle, 1, 0, vertex, texCoordTriangle);
                    tri1.SetNeighbors(triangle.RightNeighbor, tri2, tri0);
                    tri1.BaseNeighbor.ReplaceNeighbor(triangle, tri1);

                    // Triangle 2
                    tri2.Set(opposite, 2, 1, vertex, texCoordOpposite);
                    tri2.SetNeighbors(opposite.LeftNeighbor, tri3, tri1);
                    tri2.BaseNeighbor.ReplaceNeighbor(opposite, tri2);

                    // Triangle 3
                    tri3.Set(opposite, 1, 0, vertex, texCoordOpposite);
                    tri3.SetNeighbors(opposite.RightNeighbor, tri0, tri2);
                    tri3.BaseNeighbor.ReplaceNeighbor(opposite, tri3);

                    // Remove old diamonds
                    var d0 = triangle.Diamond;
                    if (d0 != null)
                    {
                        d0.ReleaseTriangles();
                        d0.Remove();
                        //d0.InsertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                    }
                    var d1 = opposite.Diamond;
                    if (d1 != null)
                    {
                        d1.ReleaseTriangles();
                        d1.Remove();
                        //d1.InsertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                    }

                    // Update lists
                    opposite.InsertBefore(InactiveTriangles);

                    next = triangle.NextNode;

                    triangle.InsertBefore(InactiveTriangles);

                    // Create new diamond
                    //var diamond = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                    var diamond = new RoamDiamond();
                    diamond.InsertBefore(ActiveDiamonds);
                    diamond.SetTriangles(tri0, tri1, tri2, tri3);
                }
                triangle = next;
            }
        }

        public void Merge()
        {
            var diamond = ActiveDiamonds.NextNode;
            while (diamond != ActiveDiamonds)
            {
                var next = diamond.NextNode;

                var triangle = diamond.Triangles[0].Parent;
                var opposite = diamond.Triangles[2].Parent;

                if (!(triangle.Material.SubdivideTriangle(triangle, false) || opposite.Material.SubdivideTriangle(opposite, false)))
                {
                    var tri0 = diamond.Triangles[0];
                    var tri1 = diamond.Triangles[1];
                    var tri2 = diamond.Triangles[2];
                    var tri3 = diamond.Triangles[3];

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
                                //RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                var d = new RoamDiamond();
                                d.InsertBefore(ActiveDiamonds);
                                d.SetTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.Parent == t2.Parent && t3.Parent == t0.Parent)
                            {
                                //RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                var d = new RoamDiamond();
                                d.InsertBefore(ActiveDiamonds);
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
                                //RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                var d = new RoamDiamond();
                                d.InsertBefore(ActiveDiamonds);
                                d.SetTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.Parent == t2.Parent && t3.Parent == t0.Parent)
                            {
                                //RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                var d = new RoamDiamond();
                                d.InsertBefore(ActiveDiamonds);
                                d.SetTriangles(t1, t2, t3, t0);
                            }
                        }
                    }

                    // Free vertex and triangles
                    var vertex = tri0.Vertexes[1];

                    // freeVertex(vertex);
                    //vertex.insertAfter(&freeVertexes, numActiveVertexes, numFreeVertexes);
                    vertex.Remove();

                    // 4 x freeTriangle();
                    // tri0.InsertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    // tri1.InsertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    // tri2.InsertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    // tri3.InsertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);

                    tri0.Remove();
                    tri1.Remove();
                    tri2.Remove();
                    tri3.Remove();

                    next = diamond.NextNode;

                    // freeDiamond(diamond);
                    //diamond.insertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                    diamond.Remove();
                }
                diamond = next;
            }
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

                    if (t1.HasVertex(t0.Vertexes[0]) && t1.HasVertex(t0.Vertexes[1]))
                        t0.RightNeighbor = t1;
                    else if (t1.HasVertex(t0.Vertexes[1]) && t1.HasVertex(t0.Vertexes[2]))
                        t0.LeftNeighbor = t1;
                    else if (t1.HasVertex(t0.Vertexes[2]) && t1.HasVertex(t0.Vertexes[0]))
                        t0.BaseNeighbor = t1;
                }
                t0.Flags |= RoamTriangleFlags.Modified;
            }
        }

    }
}