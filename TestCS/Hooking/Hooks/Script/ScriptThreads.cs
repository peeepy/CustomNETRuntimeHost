using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Hooking.Hooks.Script
{
    public class ScriptThreads
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ScriptThreadDelegate(IntPtr threads, int unk);
        public DetourHook<ScriptThreadDelegate> RunScriptThreadsHook { get; }

        public static bool RunScriptThreads(IntPtr threads, int unk)
        {
            if (g_IsRunning)
            {
                ScriptManager.Tick();
            }
            return BaseHook.Get<DetourHook<ScriptThreadDelegate>>(RunScriptThreads).Original(threads, unk);
    }
}
