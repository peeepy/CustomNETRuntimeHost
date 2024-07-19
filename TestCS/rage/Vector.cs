using System.Runtime.InteropServices;

namespace TestCS.Rage
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public unsafe struct Vector3<T> where T : unmanaged
    {
        [FieldOffset(0)] public T X;
        [FieldOffset(4)] public T Y;
        [FieldOffset(8)] public T Z;

        [FieldOffset(0)] private fixed float _data[3];

        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index > 2)
                    throw new IndexOutOfRangeException("Index must be between 0 and 2");
                fixed (float* ptr = _data)
                {
                    return *(T*)(&ptr[index]);
                }
            }
            set
            {
                if (index < 0 || index > 2)
                    throw new IndexOutOfRangeException("Index must be between 0 and 2");
                fixed (float* ptr = _data)
                {
                    *(T*)(&ptr[index]) = value;
                }
            }
        }
    }

    // fvector3 as a separate struct using Vector3<float>
    public struct fvector3
    {
        public Vector3<float> Value;

        public fvector3(float x, float y, float z)
        {
            Value = new Vector3<float>(x, y, z);
        }

        public float X { get => Value.X; set => Value.X = value; }
        public float Y { get => Value.Y; set => Value.Y = value; }
        public float Z { get => Value.Z; set => Value.Z = value; }

        public float this[int index]
        {
            get => Value[index];
            set => Value[index] = value;
        }

        // Implicit conversion operators
        public static implicit operator Vector3<float>(fvector3 v) => v.Value;
        public static implicit operator fvector3(Vector3<float> v) => new fvector3 { Value = v };
    }
}