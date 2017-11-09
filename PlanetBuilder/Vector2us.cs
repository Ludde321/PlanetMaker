using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetBuilder
{
    public struct Vector2us : IEquatable<Vector2us>
    {
        public ushort x;
        public ushort y;

        public Vector2us(ushort x0, ushort y0)
        {
            x = x0;
            y = y0;
        }

        public bool Equals(Vector2us b)
        {
            return x == b.x && y == b.y;
        }
    }
}
