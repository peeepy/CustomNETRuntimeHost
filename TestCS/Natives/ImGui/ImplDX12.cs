//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TestCS.Hooking.Hooks.GUI;

//namespace TestCS.Natives.ImGui
//{
//    internal class ImplDX12
//    {
//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_Init")]
//        public static extern bool Init(IntPtr device, int num_frames_in_flight, DXGI_FORMAT rtv_format, D3D12_CPU_DESCRIPTOR_HANDLE font_srv_cpu_desc_handle, IntPtr font_srv_gpu_desc_handle);

//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_Shutdown")]
//        public static extern void Shutdown();

//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_NewFrame")]
//        public static extern void NewFrame();

//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_RenderDrawData")]
//        public static extern void RenderDrawData(ImDrawDataPtr drawData);

//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_CreateDeviceObjects")]
//        public static extern bool CreateDeviceObjects();

//        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "ImGui_ImplDX11_InvalidateDeviceObjects")]
//        public static extern void InvalidateDeviceObjects();
//    }
//}
