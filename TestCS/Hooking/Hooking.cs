using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using TestCS.Hooking.Hooks.GUI;
using TestCS.Memory;

namespace TestCS.Hooking
{
    public static class Hooking
    {
        private static void AddHooks() 
        {
            BaseHook.Add(WindowHook.WndProc, new DetourHook<WindowHook.WndProcDelegate>("WndProc", PointerData.WndProc, WindowHook.WndProc));

            // TODO: Finish renderer...
            if (!PointerData.IsVulkan)
            {
                BaseHook.Add(SwapChainHook.Present, new DetourHook<SwapChainHook.PresentDelegate>("SwapChain::Present", GetVF(PointerData.SwapChain, SwapChainHook.VMTPresentIdx), SwapChainHook.Present));
                BaseHook.Add(SwapChainHook.ResizeBuffers, new DetourHook<SwapChainHook.ResizeBuffersDelegate>("SwapChain::ResizeBuffers", GetVF(PointerData.SwapChain, SwapChainHook.VMTResizeBuffersIdx), SwapChainHook.ResizeBuffers));
            }


            

            //SwapChainHook.PresentHook = new DetourHook<SwapChainHook.PresentDelegate>("SwapChain::Present", GetVF(PointerData.SwapChain, SwapChainHook.VMTPresentIdx), SwapChainHook.Present);
            //SwapChainHook.ResizeBuffersHook = new DetourHook<SwapChainHook.ResizeBuffersDelegate>("SwapChain::ResizeBuffers", GetVF(PointerData.SwapChain, SwapChainHook.VMTResizeBuffersIdx), SwapChainHook.ResizeBuffers);
        }


        public static bool Init()
        {
           AddHooks();
            BaseHook.EnableAll();
            return true;
        }

        public static IntPtr GetVF(IntPtr ptr, int index)
        {
            IntPtr vtable = Marshal.ReadIntPtr(ptr); // Dereference once to get IDXGISwapChain1*
            IntPtr vfTable = Marshal.ReadIntPtr(vtable); // Read the vtable pointer
            return Marshal.ReadIntPtr(vfTable + index * IntPtr.Size); // Get the function pointer at the specified index
        }
    }
}
