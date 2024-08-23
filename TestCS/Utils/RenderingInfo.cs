using System;
using System.Runtime.InteropServices;

public enum ERenderingType : byte
{
    DX12 = 1,
    Unk,
    Vulkan
}

[StructLayout(LayoutKind.Sequential)]
public class RenderingInfo
{
    public IntPtr VTable; // Virtual table pointer
    public byte unk_0008; // 0x0008
    public ERenderingType RenderingType; // 0x0009

    public bool IsRenderingType(ERenderingType type)
    {
        return RenderingType == type;
    }

    public void SetRenderingType(ERenderingType type)
    {
        if (!IsRenderingType(type))
        {
            RenderingType = type;
        }
    }

    public static RenderingInfo FromIntPtr(IntPtr ptr)
    {
        return (RenderingInfo)Marshal.PtrToStructure(ptr, typeof(RenderingInfo));
    }
}