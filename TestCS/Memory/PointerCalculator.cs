using System;
using System.Runtime.InteropServices;

namespace TestCS.Memory
{
    public sealed class PointerCalculator
    {
        private readonly IntPtr m_InternalPtr;

        [DllImport("RDONatives.dll")]
        private static extern IntPtr PointerCalculator_Add(IntPtr ptr, IntPtr offset);

        [DllImport("RDONatives.dll")]
        private static extern IntPtr PointerCalculator_Rip(IntPtr ptr);

        [DllImport("RDONatives.dll")]
        private static extern IntPtr PointerCalculator_Sub(IntPtr ptr, IntPtr offset);

        public PointerCalculator(IntPtr ptr)
        {
            m_InternalPtr = ptr;
        }

        public IntPtr AsIntPtr()
        {
            return m_InternalPtr;
        }

        public PointerCalculator Add(int offset)
        {
            return new PointerCalculator(PointerCalculator_Add(m_InternalPtr, new IntPtr(offset)));
        }

        public PointerCalculator Sub(int offset)
        {
            return new PointerCalculator(PointerCalculator_Sub(m_InternalPtr, new IntPtr(offset)));
        }

        public PointerCalculator Rip()
        {
            return new PointerCalculator(PointerCalculator_Rip(m_InternalPtr));
        }

        //public IntPtr ReadIntPtr()
        //{
        //    return Marshal.ReadIntPtr(m_InternalPtr);
        //}

        //public void WriteIntPtr(IntPtr value)
        //{
        //    Marshal.WriteIntPtr(m_InternalPtr, value);
        //}

        public T GetComObject<T>() where T : SharpDX.ComObject
        {
            IntPtr ptr = Marshal.ReadIntPtr(m_InternalPtr);
            return SharpDX.ComObject.As<T>(ptr);
        }

        ////public T ReadUnmanaged<T>() where T : unmanaged
        ////{
        ////    return Marshal.PtrToStructure<T>(m_InternalPtr);
        ////}

        ////public void WriteUnmanaged<T>(T value) where T : unmanaged
        ////{
        ////    Marshal.StructureToPtr(value, m_InternalPtr, false);
        ////}

        //public Delegate GetFunctionPointer(Type delegateType)
        //{
        //    return Marshal.GetDelegateForFunctionPointer(ReadIntPtr(), delegateType);
        //}

        //public T GetFunctionPointer<T>() where T : Delegate
        //{
        //    return Marshal.GetDelegateForFunctionPointer<T>(ReadIntPtr());
        //}

        //public bool ReadBoolean()
        //{
        //    return Marshal.ReadByte(m_InternalPtr) != 0;
        //}

        //public void WriteBoolean(bool value)
        //{
        //    Marshal.WriteByte(m_InternalPtr, (byte)(value ? 1 : 0));
        //}

        //public static explicit operator bool(PointerCalculator pc)
        //{
        //    return pc.m_InternalPtr != IntPtr.Zero;
        //}

        //public static bool operator ==(PointerCalculator a, PointerCalculator b)
        //{
        //    if (ReferenceEquals(a, b)) return true;
        //    if (a is null || b is null) return false;
        //    return a.m_InternalPtr == b.m_InternalPtr;
        //}

        //public static bool operator !=(PointerCalculator a, PointerCalculator b)
        //{
        //    return !(a == b);
        //}

        //public override bool Equals(object obj)
        //{
        //    return obj is PointerCalculator calculator && this == calculator;
        //}

        //public override int GetHashCode()
        //{
        //    return m_InternalPtr.GetHashCode();
        //}

        //public static implicit operator PointerCalculator(IntPtr ptr)
        //{
        //    return new PointerCalculator(ptr);
        //}
    }
}