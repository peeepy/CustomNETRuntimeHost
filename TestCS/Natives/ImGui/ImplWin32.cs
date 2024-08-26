using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCS.Hooking.Hooks.GUI;

namespace TestCS.Natives.ImGui
{
    internal class Win32
    {
        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplWin32_WndProcHandler")]
        public static extern int ImGui_ImplWin32_WndProcHandler(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplWin32_Shutdown")]
        public static extern void ImGui_ImplWin32_Shutdown();

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplWin32_NewFrame")]
        public static extern void ImGui_ImplWin32_NewFrame();

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplWin32_Init")]
        public static extern bool ImGui_ImplWin32_Init(IntPtr hwnd);
    }
}
