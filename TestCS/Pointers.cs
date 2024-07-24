using System;
using System.Runtime.InteropServices;

namespace TestCS
{
    internal class NativePointers
    {
        [DllImport("RDONatives.dll")]
        internal static extern bool InitPointers();

        [DllImport("RDONatives.dll")]
        internal static extern void RestorePointers();

        [DllImport("RDONatives.dll")]
        internal static extern int IsVulkanPointers();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetSwapChainPointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetHwndPointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetCommandQueuePointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetQueuePresentKHRPointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetCreateSwapchainKHRPointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetAcquireNextImageKHRPointer();

        [DllImport("RDONatives.dll")]
        internal static extern IntPtr GetAcquireNextImage2KHRPointer();
    }

    public class Pointers
    {
        public static Pointers Instance { get; } = new Pointers();
        private TaskCompletionSource<bool> _initTaskSource = new TaskCompletionSource<bool>();
        public bool IsInitialized => _initTaskSource.Task.IsCompleted && _initTaskSource.Task.Result;

        public async Task<bool> InitAsync()
        {
            try
            {
                bool success = Init(); // Your existing Init method
                _initTaskSource.SetResult(success);
                return success;
            }
            catch (Exception ex)
            {
                _initTaskSource.SetException(ex);
                return false;
            }
        }

        public Task<bool> WaitForInitializationAsync() => _initTaskSource.Task;
        private Pointers() { }

        public IntPtr SwapChain { get; private set; }
        public IntPtr CommandQueue { get; private set; }
        public IntPtr QueuePresentKHR { get; set; }
        public IntPtr CreateSwapchainKHR { get; set; }
        public IntPtr AcquireNextImageKHR { get; set; }
        public IntPtr AcquireNextImage2KHR { get; set; }
        public IntPtr Hwnd { get; private set; }
        public IntPtr IsVulkan { get; private set; }

        public bool Init()
        {
            if (!NativePointers.InitPointers())
                return false;

            SwapChain = NativePointers.GetSwapChainPointer();
            CommandQueue = NativePointers.GetCommandQueuePointer();
            //QueuePresentKHR = NativePointers.GetQueuePresentKHRPointer();
            //CreateSwapchainKHR = NativePointers.GetCreateSwapchainKHRPointer();
            //AcquireNextImageKHR = NativePointers.GetAcquireNextImageKHRPointer();
            //AcquireNextImage2KHR = NativePointers.GetAcquireNextImage2KHRPointer();
            Hwnd = NativePointers.GetHwndPointer();
            IsVulkan = NativePointers.IsVulkanPointers();
            return true;
        }

        public void Restore()
        {
            NativePointers.RestorePointers();
        }
    }

}