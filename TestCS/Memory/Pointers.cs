using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace TestCS.Memory
{
    public static class PointerData
    {
        public static SwapChain1 SwapChain { get; set; }

        public class Pointers
        {
            public static async Task<bool> Init()
            {
                Module rdr2 = ModuleManager.Get("RDR2.exe");
                if (rdr2 == null)
                {
                    LOG.ERROR("Could not find RDR2.exe.");
                    return false;
                }

                PatternScanner scanner = new PatternScanner(rdr2);

                try
                {
                    Pattern swapchainPtrn = new Pattern("IDXGISwapChain1", "48 8B 58 60 48 8B 0D");
                    LOG.INFO("Swapchain pattern added");
                    scanner.AddPattern(swapchainPtrn, ptr =>
                    {
                        PointerCalculator swapChainPtr = ptr.Add(4).Add(3).Rip();
                        IntPtr SwapChainPtr = swapChainPtr.AsIntPtr();
                        PointerData.SwapChain = swapChainPtr.GetComObject<SwapChain1>();
                    });
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Failed to add pattern: {ex}");
                }

                try
                {
                    if (!await scanner.ScanAsync())
                    {
                        LOG.ERROR("Some patterns could not be found. Stopping here.");
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Failed to scan: {ex}");
                }
                return true;
            }
        }
    }
}
