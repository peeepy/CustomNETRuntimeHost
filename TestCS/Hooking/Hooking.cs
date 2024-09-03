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
            catch (Exception ex)
            {
                LOG.ERROR($"Error adding WndProc hook: {ex}");
            }

            //try
            //{
            //    IntPtr setCursorPosFuncPtr = WindowHook.GetSetCursorPosAddress();
            //    BaseHook.Add(WindowHook.SetCursorPos, new DetourHook<WindowHook.SetCursorPosDelegate>("SetCursorPos", setCursorPosFuncPtr, WindowHook.SetCursorPos));
            //    LOG.INFO("Added SetCursorPos hook");
            //}
            //catch (Exception ex)
            //{
            //    LOG.ERROR($"Error adding WndProc hook: {ex}");
            //}

            if (!PointerData.IsVulkan)
            {
                try
                {
                    BaseHook.Add(SwapChainHook.Present, new DetourHook<SwapChainHook.PresentDelegate>("Swapchain::Present", GetVF(PointerData.SwapChain, SwapChainHook.VMTPresentIdx), SwapChainHook.Present));
                    //LOG.INFO("Added SwapChain Present hook");
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error adding SwapChain Present hook: {ex}");
                }
                try
                {
                    BaseHook.Add(SwapChainHook.ResizeBuffers, new DetourHook<SwapChainHook.ResizeBuffersDelegate>("ResizeBuffers", GetVF(PointerData.SwapChain, SwapChainHook.VMTResizeBuffersIdx), SwapChainHook.ResizeBuffers));
                    LOG.INFO("Added SwapChain ResizeBuffers hook");
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error adding SwapChain Resize Buffers hook: {ex}");
                }
            }
        }
        

        public static void Destroy()
        {
            BaseHook.DisableAll();
            //for (var hook = BaseHook.Hooks)
            //{
            //    BaseHook.Hooks.Remove
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
            try
            {
                IntPtr swapChain = Marshal.ReadIntPtr(ptr);
                IntPtr vfTable = Marshal.ReadIntPtr(swapChain);
                IntPtr functionPtr = Marshal.ReadIntPtr(vfTable + index * IntPtr.Size);

                return functionPtr;
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Error reading vtable pointer: {ex}");
                return IntPtr.Zero;
            }
        }
    }
}
