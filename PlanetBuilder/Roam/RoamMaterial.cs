using PlanetBuilder;

namespace PlanetBuilder.Roam
{

    public abstract class RoamMaterial
    {
        public abstract bool SubdivideTriangle(RoamTriangle triangle, bool split);
    }
}