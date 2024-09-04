using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Hooking.Hooks.Anticheat
{
    internal unsafe class UnkFunctionHook
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool UnkFunctionDelegate(Int64 cb);
        public static DetourHook<UnkFunctionDelegate> UnkFuncHook { get; }

        public static bool UnkFunction(Int64 cb)
        {
            Int64* modulePtr = (Int64*)((byte*)cb + 0x20);
            Int64 module = *modulePtr;

            byte* flagPtr = (byte*)((byte*)module + 0x18);
            *flagPtr = 3;

            return true;
        }
    }
}
}
}
