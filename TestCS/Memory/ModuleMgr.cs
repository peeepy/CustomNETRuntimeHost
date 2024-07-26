using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestCS.Utils.JoaatHash;

namespace TestCS.Memory
{
    public class ModuleManager
    {
        public static ModuleManager Instance { get; } = new ModuleManager();

        private ModuleManager() { }

        public bool LoadModules()
        {
            try
            {
                return NativeModuleFuncs.LoadModuleMgrModules();
            }
            catch(Exception ex)
            {
                LOG.ERROR($"Failed to load modules. {ex}");
                return false;
            }
        }

        private Module? GetImpl(string name)
        {
            IntPtr modulePtr = NativeModuleFuncs.GetModuleByHash(Joaat(name));
            return modulePtr != IntPtr.Zero ? new Module(modulePtr) : null;
        }

        public static Module? Get(string name)
        {
            return ModuleManager.Instance.GetImpl(name);
        }

    }
}
