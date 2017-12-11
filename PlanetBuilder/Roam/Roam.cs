
using System.Collections.Generic;

namespace PlanetBuilder.Roam
{
    public class Roam
    {
        public LinkedList<RoamDiamond> Diamonds;
        public LinkedList<RoamTriangle> Triangles;
        public LinkedList<RoamVertex> Vertexes;

        public LinkedList<RoamDiamond> FreeDiamonds;
        public LinkedList<RoamTriangle> FreeTriangles;
        public LinkedList<RoamVertex> FreeVertexes;

        public LinkedList<RoamDiamond> ActiveDiamonds;
        public LinkedList<RoamTriangle> ActiveTriangles;
        public LinkedList<RoamVertex> ActiveVertexes;

        public LinkedList<RoamDiamond> InactiveDiamonds;
        public LinkedList<RoamTriangle> InactiveTriangles;

        private RoamDiamond AllocDiamond()
        {
            var diamond = FreeDiamonds.First;
            FreeDiamonds.RemoveFirst();
            ActiveDiamonds.AddFirst(diamond);
            return diamond.Value;
        }

        private RoamTriangle AllocTriangle()
        {
            var triangle = FreeTriangles.First;
            FreeTriangles.RemoveFirst();
            ActiveTriangles.AddFirst(triangle);
            return triangle.Value;
        }

        private RoamVertex AllocVertex()
        {
            var vertex = FreeVertexes.First;
            FreeVertexes.RemoveFirst();
            ActiveVertexes.AddFirst(vertex);
            return vertex.Value;
        }

        //        void initVertexes(RoamTexture* textures[6]);
        //        void initTriangles();
        //        virtual bool subdivideTriangle(RoamTriangle* triangle, bool split) = 0;

        //    virtual void computeVertexAltitude(RoamVertex* vertex, const Vec3d &normal) = 0;
        //    virtual void computeVertexAltitude(RoamVertex* vertex, const RoamTriangle* triangle) = 0;
        //    virtual double computeVertexAltitude(const Vec3d &normal) = 0;
/* 
        public void Update()
        {
            Merge();
            Split();
        }

        public void Split()
        {
            var triangleNode = ActiveTriangles.First;
            while (triangleNode != null)
            {
                var next = triangleNode.Next;
                var triangle = triangleNode.Value;

                triangle.Flags |= RoamTriangleFlags.Visible;

                if ((triangle.Flags & RoamTriangleFlags.ForceSplit) || triangle.Material.SubdivideTriangle(triangle, true))
                {
                    RoamTriangle opposite = triangle.BaseNeighbor;

                    if (opposite.BaseNeighbor != triangle)
                    {
                        triangle.Flags |= RoamTriangleFlags.ForceSplit;
                        opposite.Flags |= RoamTriangleFlags.ForceSplit;
                        triangleNode = ActiveTriangles.AddBefore(triangleNode, opposite);
                        continue;
                    }

                    triangle.Flags &= ~RoamTriangleFlags.ForceSplit;

                    // Compute new texture coords
                    Vector2d texCoordTriangle = .5 * (triangle.TextureCoords[0] + triangle.TextureCoords[2]);
                    Vector2d texCoordOpposite = .5 * (opposite.TextureCoords[0] + opposite.TextureCoords[2]);

                    // allocVertex();
                    RoamVertex* vertex = freeVertexes.nextNode.insertBefore(&activeVertexes, numFreeVertexes, numActiveVertexes);

                    // Compute vertex
                    computeVertexAltitude(vertex, triangle);

                    vertex.createdAt = vertexTimer;

                    // 4 x allocTriangle();
                    RoamTriangle* tri0 = freeTriangles.nextNode.insertBefore(&activeTriangles, numFreeTriangles, numActiveTriangles);
                    RoamTriangle* tri1 = freeTriangles.nextNode.insertBefore(&activeTriangles, numFreeTriangles, numActiveTriangles);
                    RoamTriangle* tri2 = freeTriangles.nextNode.insertBefore(&activeTriangles, numFreeTriangles, numActiveTriangles);
                    RoamTriangle* tri3 = freeTriangles.nextNode.insertBefore(&activeTriangles, numFreeTriangles, numActiveTriangles);

                    ushort nextLevel = triangle.level + 1;

                    // Triangle 0
                    tri0.set(triangle, 2, 1, vertex, texCoordTriangle, 0);
                    tri0.setNeighbors(triangle.leftNeighbor, tri1, tri3);
                    tri0.baseNeighbor.replaceNeighbor(triangle, tri0);

                    // Triangle 1
                    tri1.set(triangle, 1, 0, vertex, texCoordTriangle, 1);
                    tri1.setNeighbors(triangle.rightNeighbor, tri2, tri0);
                    tri1.baseNeighbor.replaceNeighbor(triangle, tri1);

                    // Triangle 2
                    tri2.set(opposite, 2, 1, vertex, texCoordOpposite, 0);
                    tri2.setNeighbors(opposite.leftNeighbor, tri3, tri1);
                    tri2.baseNeighbor.replaceNeighbor(opposite, tri2);

                    // Triangle 3
                    tri3.set(opposite, 1, 0, vertex, texCoordOpposite, 1);
                    tri3.setNeighbors(opposite.rightNeighbor, tri0, tri2);
                    tri3.baseNeighbor.replaceNeighbor(opposite, tri3);

                    // Remove old diamonds
                    RoamDiamond* d0 = triangle.diamond;
                    if (d0)
                    {
                        d0.releaseTriangles();
                        d0.insertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                    }
                    RoamDiamond* d1 = opposite.diamond;
                    if (d1)
                    {
                        d1.releaseTriangles();
                        d1.insertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                    }

                    // Update lists
                    opposite.insertBefore(&inactiveTriangles, numActiveTriangles, numInactiveTriangles);

                    next = triangle.nextNode;

                    triangle.insertBefore(&inactiveTriangles, numActiveTriangles, numInactiveTriangles);

                    // Create new diamond
                    RoamDiamond* diamond = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                    diamond.setTriangles(tri0, tri1, tri2, tri3);
                }
                triangle = next;
            }
        }

        void Roam::merge()
        {
            RoamDiamond* diamond = activeDiamonds.nextNode;
            while (diamond != &activeDiamonds)
            {
                RoamDiamond* next = diamond.nextNode;

                RoamTriangle* triangle = diamond.triangles[0].parent;
                RoamTriangle* opposite = diamond.triangles[2].parent;

                if (!(subdivideTriangle(triangle, false) || subdivideTriangle(opposite, false)))
                {
                    RoamTriangle* tri0 = diamond.triangles[0];
                    RoamTriangle* tri1 = diamond.triangles[1];
                    RoamTriangle* tri2 = diamond.triangles[2];
                    RoamTriangle* tri3 = diamond.triangles[3];

                    // Update neighbors
                    triangle.setNeighbors(opposite, tri0.baseNeighbor, tri1.baseNeighbor);
                    opposite.setNeighbors(triangle, tri2.baseNeighbor, tri3.baseNeighbor);

                    tri0.baseNeighbor.replaceNeighbor(tri0, triangle);
                    tri1.baseNeighbor.replaceNeighbor(tri1, triangle);
                    tri2.baseNeighbor.replaceNeighbor(tri2, opposite);
                    tri3.baseNeighbor.replaceNeighbor(tri3, opposite);

                    // Update lists
                    triangle.insertBefore(&activeTriangles, numInactiveTriangles, numActiveTriangles);
                    opposite.insertBefore(&activeTriangles, numInactiveTriangles, numActiveTriangles);

                    triangle.diamond = NULL;
                    opposite.diamond = NULL;

                    RoamTriangle* t0, *t1, *t2, *t3;

                    // Create new diamond 1
                    if (triangle.level > 0)
                    {
                        t0 = triangle;
                        t1 = t0.leftNeighbor;
                        t2 = t1.leftNeighbor;
                        t3 = t2.leftNeighbor;

                        if (t3.leftNeighbor == t0)
                        {
                            if (t0.parent == t1.parent && t2.parent == t3.parent)
                            {
                                RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                d.setTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.parent == t2.parent && t3.parent == t0.parent)
                            {
                                RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                d.setTriangles(t1, t2, t3, t0);
                            }
                        }
                    }
                    // Create new diamond 2
                    if (opposite.level > 0)
                    {
                        t0 = opposite;
                        t1 = t0.leftNeighbor;
                        t2 = t1.leftNeighbor;
                        t3 = t2.leftNeighbor;

                        if (t3.leftNeighbor == t0)
                        {
                            if (t0.parent == t1.parent && t2.parent == t3.parent)
                            {
                                RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                d.setTriangles(t0, t1, t2, t3);
                            }
                            else
                            if (t1.parent == t2.parent && t3.parent == t0.parent)
                            {
                                RoamDiamond* d = freeDiamonds.nextNode.insertBefore(&activeDiamonds, numFreeDiamonds, numActiveDiamonds);
                                d.setTriangles(t1, t2, t3, t0);
                            }
                        }
                    }

                    // Free vertex and triangles
                    RoamVertex* vertex = tri0.vertexes[1];

                    // freeVertex(vertex);
                    vertex.insertAfter(&freeVertexes, numActiveVertexes, numFreeVertexes);

                    // 4 x freeTriangle();
                    tri0.insertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    tri1.insertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    tri2.insertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);
                    tri3.insertAfter(&freeTriangles, numActiveTriangles, numFreeTriangles);

                    next = diamond.nextNode;

                    // freeDiamond(diamond);
                    diamond.insertAfter(&freeDiamonds, numActiveDiamonds, numFreeDiamonds);
                }
                diamond = next;
            }
        }
*/


        private void InitTriangles()
        {
            // Compute neighbor relationships
            foreach (var t0 in ActiveTriangles)
            {
                foreach (var t1 in ActiveTriangles)
                {
                    if (t0 == t1)
                        continue;

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