using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Hooking.Hooks.GUI
{
    public class WindowHook
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr WndProcDelegate(IntPtr hwnd, uint umsg, IntPtr wparam, IntPtr lparam);

        public static DetourHook<WndProcDelegate> WndProcHook { get; }

        public static IntPtr WndProc(IntPtr hwnd, uint umsg, IntPtr wparam, IntPtr lparam) // return type LRESULT, params HWND, WPARAM, LPARAM
        {
            LOG.INFO("Hooked WndProc");
            //    Renderer.WndProc(hwnd, umsg, wparam, lparam);

            return BaseHook.Get<DetourHook<WndProcDelegate>>(WndProc).Original(hwnd, umsg, wparam, lparam);
        }
    }
}
