using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using TestCS.Hooking.Hooks.GUI;
using TestCS.Memory;
using EasyHook;

namespace TestCS.Hooking
{
    public static class Hooking
    {
        private static void AddHooks() 
        {
            try
            {
                BaseHook.Add(WindowHook.WndProc, new DetourHook<WindowHook.WndProcDelegate>("WndProc", PointerData.WndProc, WindowHook.WndProc));
                LOG.INFO("Added WndProc hook");
            }
            catch (Exception ex) {
                LOG.ERROR($"Error adding WndProc hook: {ex}");
                    }

            // TODO: Finish renderer...
            //if (!PointerData.IsVulkan)
            //{
            //    BaseHook.Add(SwapChainHook.Present, new DetourHook<SwapChainHook.PresentDelegate>("SwapChain::Present", GetVF(PointerData.SwapChain, SwapChainHook.VMTPresentIdx), SwapChainHook.Present));
            //    BaseHook.Add(SwapChainHook.ResizeBuffers, new DetourHook<SwapChainHook.ResizeBuffersDelegate>("SwapChain::ResizeBuffers", GetVF(PointerData.SwapChain, SwapChainHook.VMTResizeBuffersIdx), SwapChainHook.ResizeBuffers));
            //}


            
        }


        public static bool Init()
        {
           AddHooks();
            BaseHook.EnableAll();
            LOG.INFO("Hooks enabled.");
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
