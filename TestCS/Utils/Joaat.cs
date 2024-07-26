using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace TestCS.Utils
{
    internal partial class JoaatHash
    {

        [LibraryImport("RDONatives.dll")]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        internal static partial UInt32 CalculateJoaat([MarshalAs(UnmanagedType.LPStr)] string str);

        public static UInt32 Joaat(string str)
        {
            return CalculateJoaat(str);
        }
    }

   
}
