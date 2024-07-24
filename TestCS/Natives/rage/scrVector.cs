using System;
using System.Runtime.InteropServices;

namespace TestCS.Rage
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct scrVector
    {
        public float x;
        public float y;
        public float z;

        public scrVector(fvector3 vec) : this(vec.X, vec.Y, vec.Z) { }

        public scrVector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static scrVector operator +(scrVector a, scrVector b)
            => new scrVector(a.x + b.x, a.y + b.y, a.z + b.z);

        public static scrVector operator -(scrVector a, scrVector b)
            => new scrVector(a.x - b.x, a.y - b.y, a.z - b.z);

        public static scrVector operator *(scrVector a, scrVector b)
            => new scrVector(a.x * b.x, a.y * b.y, a.z * b.z);

        public static scrVector operator *(scrVector a, float scalar)
            => new scrVector(a.x * scalar, a.y * scalar, a.z * scalar);

        public static bool operator ==(scrVector a, scrVector b)
            => a.x == b.x && a.y == b.y && a.z == b.z;

        public static bool operator !=(scrVector a, scrVector b)
            => !(a == b);

        public override bool Equals(object? obj)
            => obj is scrVector vector && this == vector;

        public override int GetHashCode()
            => HashCode.Combine(x, y, z);

        public override string ToString()
            => $"({x}, {y}, {z})";

        // Implicit conversions
        public static implicit operator scrVector(fvector3 v) => new scrVector(v);
        public static implicit operator fvector3(scrVector v) => new fvector3(v.x, v.y, v.z);

        // Alias for Vector3
        public static implicit operator scrVector(Vector3<float> v) => new scrVector(v.X, v.Y, v.Z);
        public static implicit operator Vector3<float>(scrVector v) => new Vector3<float>(v.x, v.y, v.z);
    }
}