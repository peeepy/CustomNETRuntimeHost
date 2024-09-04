using System;
using System.Runtime.InteropServices;

namespace rage
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public unsafe abstract class rlMetric
    {
        public abstract int GetType();
        public abstract int GetType2();
        public abstract string GetName();
        public abstract bool Serialize(rage.rlJson* serializer);
        public abstract int GetSize();
        public abstract uint GetNameHash(); // was joaat_t. hopefully it works
        public abstract bool _0x38();
        public abstract bool _0x40();

        ~rlMetric() { }
    }
}