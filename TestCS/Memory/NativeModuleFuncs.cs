using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace TestCS.Memory
{
    internal partial class NativeModuleFuncs
    {
        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool LoadModuleMgrModules();

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr GetModuleByHash(uint hash);


        // Module properties
        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial IntPtr GetModuleName(IntPtr modulePtr);

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial ulong GetModuleSize(IntPtr modulePtr);

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial UIntPtr GetModuleBase(IntPtr modulePtr);

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial UIntPtr GetModuleEnd(IntPtr modulePtr);

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsModuleValid(IntPtr modulePtr);
    }
}
