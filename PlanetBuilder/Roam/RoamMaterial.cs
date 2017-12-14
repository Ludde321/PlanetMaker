using PlanetBuilder;

namespace PlanetBuilder.Roam
{

    public abstract class RoamMaterial
    {
        public abstract bool SubdivideTriangle(RoamTriangle triangle, bool split);

        public virtual void ComputeVertexAltitude(RoamVertex vertex, Vector3d normal)
        {
            vertex.LinearPosition = normal;
            vertex.Normal = normal;
            vertex.Position = normal;
        }
        public virtual void ComputeVertexAltitude(RoamVertex vertex, RoamTriangle triangle)
        {
            vertex.LinearPosition = Vector3d.MiddlePoint(triangle.Vertexes[0].Position, triangle.Vertexes[2].Position);
            vertex.Normal = Vector3d.Normalize(vertex.LinearPosition);

            vertex.Position = vertex.LinearPosition;//vertex.Normal * (groundRadius + vertex.altitude);
        }

    }
}