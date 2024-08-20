using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using SharpGen.Runtime;
using Silk.NET.Vulkan;

namespace TestCS.Memory
{
    public static class PointerData
    {
        // This represents IDXGISwapChain1**
        // This will be used when we actually set the SwapChain object via hooking
        public static IntPtr SwapChain { get; set; }
        public static bool IsVulkan { get; }
        public static IntPtr WndProc { get; set; } //PVOID

        public class Pointers
        {

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);


            public static void Init(Action<bool> initCallback)
            {
                Module rdr2 = ModuleManager.Get("RDR2.exe");
                if (rdr2 == null)
                {
                    LOG.ERROR("Could not find RDR2.exe.");
                    initCallback(false);
                    return;
                }

                PatternScanner scanner = new PatternScanner(rdr2);

                Pattern WndProcPtrn = new("WndProc", "48 89 5C 24 ? 4C 89 4C 24 ? 48 89 4C 24 ? 55 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 60");
                scanner.AddPattern(WndProcPtrn, ptr =>
                {
                    WndProc = ptr.AsIntPtr(); //PVOID
                });


                Pattern swapchainPtrn = new("IDXGISwapChain1", "48 8B 58 60 48 8B 0D");
                scanner.AddPattern(swapchainPtrn, ptr =>
                {
                    try
                    {
                        LOG.INFO($"Swapchain pattern found, initial pointer: {ptr.AsIntPtr():X}");

                        // This is equivalent to IDXGISwapChain1**
                        SwapChain = ptr.Add(4).Add(3).Rip().AsIntPtr();
                        LOG.INFO($"SwapChain pointer to pointer (IDXGISwapChain1**): {SwapChain:X}");
                    }
                    catch (Exception ex)
                    {
                        LOG.ERROR($"Error in swapchain delegate: {ex.Message}");
                        LOG.ERROR($"Stack trace: {ex.StackTrace}");
                    }
                });

                scanner.ScanAsync(scanSuccess =>
                {
                    if (scanSuccess)
                    {
                        LOG.INFO("Patterns scanned successfully.");
                        initCallback(true);
                    }
                    else
                    {
                        LOG.ERROR("Some patterns could not be found. Stopping here.");
                        initCallback(false);
                    }
                });
            }
        }
    }
}