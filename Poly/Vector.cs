namespace Poly
{
    struct Vector
    {
        public Vector(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        internal int x; internal int y; internal int z;

        public static Vector operator +(Vector v, LatticeVector delta)
        {
            return new Vector(v.x + XUnit(delta), v.y + YUnit(delta), v.z + ZUnit(delta));
        }

        public static int XUnit(LatticeVector vector)
        {
            return (((int)vector >> 0) & 0b11) - 1;
        }

        public static int YUnit(LatticeVector vector)
        {
            return (((int)vector >> 2) & 0b11) - 1;
        }

        public static int ZUnit(LatticeVector vector)
        {
            return (((int)vector >> 4) & 0b11) - 1;
        }

    }
}
