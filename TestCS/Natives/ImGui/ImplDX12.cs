using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCS.Hooking.Hooks.GUI;

namespace TestCS.Natives.ImGui
{
    using System;
    using System.Runtime.InteropServices;

    internal class DX12
    {
        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_Init")]
        public static extern bool ImGui_ImplDX12_Init(IntPtr device, int num_frames_in_flight, DXGI_FORMAT rtv_format, IntPtr cbv_srv_heap, IntPtr font_srv_cpu_desc_handle, IntPtr font_srv_gpu_desc_handle);

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_Shutdown")]
        public static extern void ImGui_ImplDX12_Shutdown();

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_NewFrame")]
        public static extern void ImGui_ImplDX12_NewFrame();

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_RenderDrawData")]
        public static extern void ImGui_ImplDX12_RenderDrawData(IntPtr draw_data, IntPtr graphics_command_list);

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_CreateDeviceObjects")]
        public static extern bool ImGui_ImplDX12_CreateDeviceObjects();

        [DllImport("imgui_dx12win32", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImplDX12_InvalidateDeviceObjects")]
        public static extern void ImGui_ImplDX12_InvalidateDeviceObjects();
    }
}
