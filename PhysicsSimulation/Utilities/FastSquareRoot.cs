using System.Runtime.InteropServices;

namespace PhysicsSimulation.Utilities
{
    public class FastSquareRoot
    {
        public static float Sqrt(float z)
        {
            if (z == 0) 
                return 0;
            FloatIntUnion u;
            u.tmp = 0;
            var xhalf = 0.5f * z;
            u.f = z;
            u.tmp = 0x5f375a86 - (u.tmp >> 1);
            u.f = u.f * (1.5f - xhalf * u.f * u.f);
            return u.f * z;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct FloatIntUnion
    {
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public int tmp;
    }
}
