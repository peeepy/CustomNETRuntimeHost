using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RDR2CS
{
    public static partial class ModuleMgr
    {
        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool LoadModuleMgrModules();

        public static bool LoadModules()
        {
            return LoadModuleMgrModules();
        }
    }
}
