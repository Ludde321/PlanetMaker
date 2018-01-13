using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct Vector2s : IEquatable<Vector2s>
    {
        public short x;
        public short y;

        public Vector2s(short x0, short y0)
        {
            x = x0;
            y = y0;
        }

        public bool Equals(Vector2s b)
        {
            return x == b.x && y == b.y;
        }
    }
}
