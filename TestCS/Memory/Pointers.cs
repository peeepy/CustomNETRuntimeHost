using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Silk.NET.Vulkan;

namespace TestCS.Memory
{
    public static class PointerData
    {
        public static SwapChain1 SwapChain { get; set; }
        public static IntPtr SwapChainPtr { get; set; }

        public class Pointers
        {
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
                Pattern swapchainPtrn = new Pattern("IDXGISwapChain1", "48 8B 58 60 48 8B 0D");
                LOG.INFO("Swapchain pattern added");
                scanner.AddPattern(swapchainPtrn, ptr =>
                {
                    IntPtr swapChainPtr = ptr.Add(4).Add(3).Rip().AsIntPtr();
                    //PointerData.SwapChain = swapChainPtr.GetComObject<SwapChain1>();
                });

                scanner.ScanAsync(scanSuccess =>
                {
                    if (scanSuccess)
                    {
                        LOG.INFO("Patterns scanned successfully.");
                        //try
                        //{
                        //    IntPtr swapchainPtr = scanner.GetPatternResult(swapchainPtrn);
                        //    //IntPtr swapchainPtr = IntPtr.Zero;
                        //    if (swapchainPtr == IntPtr.Zero)
                        //    {
                        //        LOG.ERROR("SwapChain pattern was not found.");
                        //        initCallback(false);
                        //    }
                            
                            //try
                            //{
                                //PointerCalculator swapChainCalc = new PointerCalculator(swapchainPtr);
                                //LOG.INFO("PointerCalculator created successfully");
                                //swapChainCalc = swapChainCalc.Add(4).Add(3).Rip();
                                //LOG.INFO($"Final SwapChain pattern found: {swapchainPtr.ToInt64():X}");
                                //swapChainCalc = swapChainCalc.Add(4);
                                //LOG.INFO($"After first Add(4): {swapChainCalc.AsIntPtr().ToInt64():X}");

                                //swapChainCalc = swapChainCalc.Add(3);
                                //LOG.INFO($"After second Add(3): {swapChainCalc.AsIntPtr().ToInt64():X}");

                                //swapChainCalc = swapChainCalc.Rip();
                                //LOG.INFO($"After Rip(): {swapChainCalc.AsIntPtr().ToInt64():X}");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        LOG.ERROR($"Failed to create PointerCalculator: {ex.GetType().Name} - {ex.Message}");
                        //    }
                            
                        //}
                        //catch (Exception ex)
                        //{
                        //    LOG.ERROR($"Unexpected error in ProcessResultsAsync: {ex.Message}");
                        //    initCallback(false);
                        //}
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
