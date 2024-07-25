//using System;
//using System.Runtime.InteropServices;

//namespace TestCS
//{
//    internal class NativePointers
//    {
//        [DllImport("RDONatives.dll")]
//        internal static extern bool InitPointers();

//        [DllImport("RDONatives.dll")]
//        internal static extern void RestorePointers();

//        [DllImport("RDONatives.dll")]
//        internal static extern int IsVulkanPointers();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetSwapChainPointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetHwndPointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetCommandQueuePointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetQueuePresentKHRPointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetCreateSwapchainKHRPointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetAcquireNextImageKHRPointer();

//        [DllImport("RDONatives.dll")]
//        internal static extern IntPtr GetAcquireNextImage2KHRPointer();
//    }

//    public class Pointers
//    {
//public static Pointers Instance { get; } = new Pointers();
//private TaskCompletionSource<bool> _initTaskSource = new TaskCompletionSource<bool>();
//public bool IsInitialized => _initTaskSource.Task.IsCompleted && _initTaskSource.Task.Result;

//public async Task<bool> InitAsync()
//{
//    try
//    {
//        bool success = Init(); // Your existing Init method
//        _initTaskSource.SetResult(success);
//        return success;
//    }
//    catch (Exception ex)
//    {
//        _initTaskSource.SetException(ex);
//        return false;
//    }
//}

//public Task<bool> WaitForInitializationAsync() => _initTaskSource.Task;
//private Pointers() { }

//        public IntPtr SwapChain { get; private set; }
//        public IntPtr CommandQueue { get; private set; }
//        public IntPtr QueuePresentKHR { get; set; }
//        public IntPtr CreateSwapchainKHR { get; set; }
//        public IntPtr AcquireNextImageKHR { get; set; }
//        public IntPtr AcquireNextImage2KHR { get; set; }
//        public IntPtr Hwnd { get; private set; }
//        public IntPtr IsVulkan { get; private set; }

//        public bool Init()
//        {
//            if (!NativePointers.InitPointers())
//                return false;

//            SwapChain = NativePointers.GetSwapChainPointer();
//            CommandQueue = NativePointers.GetCommandQueuePointer();
//            //QueuePresentKHR = NativePointers.GetQueuePresentKHRPointer();
//            //CreateSwapchainKHR = NativePointers.GetCreateSwapchainKHRPointer();
//            //AcquireNextImageKHR = NativePointers.GetAcquireNextImageKHRPointer();
//            //AcquireNextImage2KHR = NativePointers.GetAcquireNextImage2KHRPointer();
//            Hwnd = NativePointers.GetHwndPointer();
//            IsVulkan = NativePointers.IsVulkanPointers();
//            return true;
//        }

//        public void Restore()
//        {
//            NativePointers.RestorePointers();
//        }
//    }

//}

namespace TestCS
{
    using System;
    using System.Runtime.InteropServices;

    public static class NativeScanner
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ScanCallbackDelegate(IntPtr ptr);

        [DllImport("RDONatives.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ScanPattern(string moduleName, string patternName, string pattern, IntPtr callback);

        [DllImport("RDONatives.dll")]
        private static extern IntPtr PointerCalculator_Add(IntPtr ptr, IntPtr offset);

        [DllImport("RDONatives.dll")]
        private static extern IntPtr PointerCalculator_Rip(IntPtr ptr);

        public static bool Scan(string moduleName, string patternName, string pattern, Action<IntPtr> callback)
        {
            ScanCallbackDelegate nativeCallback = ptr => callback(ptr);
            IntPtr nativeCallbackPtr = Marshal.GetFunctionPointerForDelegate(nativeCallback);
            bool result = ScanPattern(moduleName, patternName, pattern, nativeCallbackPtr);

            // Important: Keep a reference to the delegate to prevent garbage collection
            GC.KeepAlive(nativeCallback);

            return result;
        }

        public static IntPtr Add(IntPtr ptr, int offset)
        {
            return PointerCalculator_Add(ptr, new IntPtr(offset));
        }

        public static IntPtr Rip(IntPtr ptr)
        {
            return PointerCalculator_Rip(ptr);
        }
    }

    public class Pointers
    {
        public IntPtr SwapChain { get; private set; }
        public IntPtr CommandQueue { get; private set; }
        //public Func<RenderingInfo> GetRendererInfo { get; private set; }

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

        static public bool Init()
        {
            bool success = true;

            success &= NativeScanner.Scan("RDR2.exe", "IDXGISwapChain1", "48 8B 58 60 48 8B 0D", ptr =>
            {
                Pointers.Instance.SwapChain = NativeScanner.Rip(NativeScanner.Add(NativeScanner.Add(ptr, 4), 3));
            });

            success &= NativeScanner.Scan("RDR2.exe", "ID3D12CommandQueue", "FF 50 10 48 8B 0D ? ? ? ? 48 8B 01", ptr =>
            {
                Pointers.Instance.CommandQueue = NativeScanner.Rip(NativeScanner.Add(NativeScanner.Add(ptr, 3), 3));
            });

            //success &= NativeScanner.Scan("RDR2.exe", "c", "E8 ? ? ? ? 38 58 09", ptr =>
            //{
            //    IntPtr funcPtr = NativeScanner.Rip(NativeScanner.Add(ptr, 1));
            //    GetRendererInfo = Marshal.GetDelegateForFunctionPointer<Func<RenderingInfo>>(funcPtr);
            //});

            return success;
        }
    }
}