using System;
using System.Runtime.InteropServices;


namespace rage
{
    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 0x24)]
    public unsafe class rlJson
    {
        [FieldOffset(0x00)]
        public uint unk0;  // 0x00

        [FieldOffset(0x04)]
        public uint unk1;  // 0x04

        [FieldOffset(0x08)]
        public IntPtr m_Buffer;  // 0x08

        [FieldOffset(0x10)]
        public uint m_Curlen;  // 0x10

        [FieldOffset(0x14)]
        public uint m_Maxlen;  // 0x14

        [FieldOffset(0x18)]
        public uint unk4;  // 0x18

        [FieldOffset(0x1C)]
        public uint m_Flags;  // 0x1C

        [FieldOffset(0x20)]
        public byte m_Flags2;  // 0x20

        public rlJson(IntPtr buffer, uint length)
        {
            m_Buffer = buffer;
            m_Maxlen = length;
            unk0 = 0;
            unk1 = 0;
            m_Curlen = 0;
            unk4 = 1;
            m_Flags = 0;
            m_Flags2 = 0;
        }

        public IntPtr GetBuffer()
        {
            return m_Buffer;
        }
    }
}

