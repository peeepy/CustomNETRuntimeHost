using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Memory
{
    public class Module
    {
        private IntPtr _modulePtr;

        internal Module(IntPtr modulePtr)
        {
            _modulePtr = modulePtr;
        }

        public UIntPtr Base
        {
            get { return (UIntPtr)NativeModuleFuncs.GetModuleBase(_modulePtr); }
        }

        public ulong Size
        {
            get { return NativeModuleFuncs.GetModuleSize(_modulePtr); }
        }

        public UIntPtr End
        {
            get { return (UIntPtr)NativeModuleFuncs.GetModuleEnd(_modulePtr); }
        }

        public bool Valid()
        {
            return NativeModuleFuncs.IsModuleValid(_modulePtr);
        }
        //public IntPtr GetImport(string moduleName, string symbolName)
        //    {
        //        [DllImport("YourNativeDll.dll", CharSet = CharSet.Ansi)]
        //        static extern IntPtr GetModuleImport(IntPtr modulePtr, string moduleName, string symbolName);

        //        return GetModuleImport(_modulePtr, moduleName, symbolName);
        //    }

            // Add other methods as needed
    }
}
