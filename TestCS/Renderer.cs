using SharpDX;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using Silk.NET.Vulkan;
using System.Diagnostics;
using TestCS.GUI;
using TestCS.Memory;

namespace TestCS
{
    public class FrameContext
    {
        public SharpDX.Direct3D12.CommandAllocator CommandAllocator { get; set; }
        public SharpDX.Direct3D12.Resource BackBuffer { get; set; }
        public SharpDX.Direct3D12.CpuDescriptorHandle RtvDescriptor { get; set; }
        public ulong FenceValue { get; set; }
    }


    public class Renderer : IDisposable
    {
        private Renderer() { }

        public delegate void RendererCallback();
        public delegate void WindowProcedureCallback(IntPtr hWnd, uint msg, IntPtr wparam, IntPtr lparam);

        private bool _Resizing;
        private static List<RendererCallback> _RendererCallBacks = new List<RendererCallback>();
        private static List<WindowProcedureCallback> _WindowProcedureCallbacks = new List<WindowProcedureCallback>();
        private static Renderer _Instance;
        private static bool _isInitialized = false;
        private static Stopwatch _frameStopwatch = new Stopwatch();
        public static bool IsInitialized() => _isInitialized;

        private static Renderer GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = new Renderer();
            }
            return _Instance;
        }

        public static bool IsResizing()
        {
            return GetInstance()._Resizing;
        }
        public static void SetResizing(bool status)
        {
            GetInstance()._Resizing = status;
        }

        //public ImFontAtlas _FontAtlas;
        // DX12
        private FrameContext[] _FrameContext { get; set; }
        private SwapChainDescription1 _SwapChainDesc { get; set; }
        private SwapChain1 _GameSwapChain { get; set; }
        private SwapChain3 _SwapChain { get; set; }
        private SharpDX.Direct3D12.Device _Device { get; set; }
        private SharpDX.Direct3D12.CommandQueue _CommandQueue { get; set; }
        private SharpDX.Direct3D12.CommandAllocator _CommandAllocator { get; set; }
        private SharpDX.Direct3D12.GraphicsCommandList _CommandList { get; set; }
        private SharpDX.Direct3D12.DescriptorHeap _RtvDescriptorHeap { get; set; }
        private SharpDX.Direct3D12.DescriptorHeap _DescriptorHeap { get; set; }
        private SharpDX.Direct3D12.Fence _Fence { get; set; }
        private AutoResetEvent _FenceEvent { get; set; }
        private ulong _FenceLastSignaledValue { get; set; }
        private IntPtr _SwapchainWaitableObject { get; set; }
        private ulong _FrameIndex { get; set; }

        private DebugInterface _debugInterface;
        private SharpDX.Direct3D12.InfoQueue _infoQueue;

        private bool InitDX12()
        {
            LOG.INFO("Initialising DX12...");

            if (Memory.PointerData.SwapChain == IntPtr.Zero)
            {
                LOG.ERROR("SwapChain pointer is invalid!");
                return false;
            }
            if (Memory.PointerData.CommandQueue == IntPtr.Zero)
            {
                LOG.ERROR("CommandQueue pointer is invalid!");
                return false;
            }

            // SwapChain initialization
            IntPtr swapChainPtr = Marshal.ReadIntPtr(Memory.PointerData.SwapChain);
            if (swapChainPtr == IntPtr.Zero)
            {
                LOG.WARNING("Dereferenced SwapChain pointer is invalid!");
                return false;
            }

            try
            {
                // Create base SwapChain object
                _GameSwapChain = new SwapChain1(swapChainPtr);
                //LOG.INFO("Base SwapChain created successfully.");

                // Query for SwapChain3 interface
                _SwapChain = _GameSwapChain.QueryInterface<SwapChain3>();
                if (_SwapChain == null)
                {
                    LOG.WARNING("Failed to query SwapChain3 interface, falling back to base SwapChain");
                    _SwapChain = (SwapChain3?)_GameSwapChain;
                }
                else
                {
                    //LOG.INFO("Successfully queried SwapChain3 interface.");
                }
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to create or query SwapChain: {ex.Message}");
                return false;
            }

            // CommandQueue initialization
            IntPtr commandQueuePtr = Marshal.ReadIntPtr(Memory.PointerData.CommandQueue);
            if (commandQueuePtr == IntPtr.Zero)
            {
                LOG.WARNING("Dereferenced CommandQueue pointer is invalid!");
                return false;
            }

            try
            {
                _CommandQueue = new CommandQueue(commandQueuePtr);
                //LOG.INFO("CommandQueue created successfully.");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to create CommandQueue: {ex.Message}");
                return false;
            }


            try
            {
                // Get the device from the SwapChain
                var temp = _SwapChain.GetDevice<SharpDX.Direct3D12.Device>();
                _Device = temp;

                if (_Device == null)
                {
                    LOG.ERROR("Failed to get D3D12 Device: Device is null");
                    return false;
                }
                //LOG.INFO("Successfully created D3D12 device.");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to get D3D12 Device with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }



            _SwapChainDesc = _SwapChain.Description1;

            if (_Device == null || _Device.NativePointer == IntPtr.Zero)
            {
                LOG.ERROR("Device is null or invalid");
                return false;
            }

            _Fence = _Device.CreateFence(0, FenceFlags.None);
            _FenceEvent = new AutoResetEvent(false);

            // Initialize the array based on the number of swap chain buffers
            _FrameContext = new FrameContext[_SwapChainDesc.BufferCount];

            // Populate the array with new FrameContext instances
            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext[i] = new FrameContext();
            }

            // Create descriptor heap for ImGui
            var descriptorHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = _SwapChainDesc.BufferCount,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };
            _DescriptorHeap = _Device.CreateDescriptorHeap(descriptorHeapDesc);

            try
            {
                _CommandAllocator = _Device.CreateCommandAllocator(CommandListType.Direct);
                if (_CommandAllocator == null)
                {
                    LOG.ERROR("Failed to create Command Allocator: CommandAllocator is null");
                    return false;
                }
                //LOG.INFO("Successfully created CommandAllocator");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Command Allocator with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }

            try
            {
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    _FrameContext[i].CommandAllocator = _CommandAllocator;
                }
                //LOG.INFO("Assigned CommandAllocator to all FrameContext instances.");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to assign CommandAllocator to FrameContext instances: {ex.Message}");
                return false;
            }

            try
            {
                // Create the command list.
                _CommandList = _Device.CreateCommandList(CommandListType.Direct, _CommandAllocator, null);
                if (_CommandList == null)
                {
                    LOG.ERROR("Failed to create Command List: CommandList is null");
                    return false;
                }
                //LOG.INFO("Successfully created CommandList");

                _CommandList.Close();
                //LOG.INFO("Successfully closed CommandList");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create or close Command List with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }


            // Create RTV descriptor heap
            var rtvDescriptorHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = _SwapChainDesc.BufferCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };
            _RtvDescriptorHeap = _Device.CreateDescriptorHeap(rtvDescriptorHeapDesc);

            // Create RTVs
            int rtvDescriptorSize = _Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
            var rtvHandle = _RtvDescriptorHeap.CPUDescriptorHandleForHeapStart;
            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext[i].BackBuffer = _SwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                _Device.CreateRenderTargetView(_FrameContext[i].BackBuffer, null, rtvHandle);
                _FrameContext[i].RtvDescriptor = rtvHandle;
                rtvHandle += rtvDescriptorSize;
            }

            try
            {
                var ctx = ImGui.CreateContext(null);
                ImGui.SetCurrentContext(ctx);
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to create ImGui context: {ex}");
                return false;
            }
            try
            {
                ImGui.ImGuiImplWin32Init(Memory.PointerData.Hwnd);
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to initialise ImGui_Win32: {ex}");
                return false;
            }
            try
            {
                IntPtr cpuHandle = _DescriptorHeap.CPUDescriptorHandleForHeapStart.Ptr;
                var gpuHandle = _DescriptorHeap.GPUDescriptorHandleForHeapStart.Ptr;
                ImGui.ImGuiImplDX12Init(_Device.NativePointer, _SwapChainDesc.BufferCount, DearImguiSharp.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8UNORM, _DescriptorHeap.NativePointer, cpuHandle, gpuHandle);
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to initialise ImGui_DX12: {ex}");
                return false;
            }
            try
            {
                ImGui.StyleColorsDark(null);
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to set ImGui style to dark: {ex}");
                return false;
            }


            LOG.INFO("DirectX12 has initialised successfully.");
            _isInitialized = true;
            return true;
        }

        //private void LogDebugMessages()
        //{
        //    if (_infoQueue == null) return;

        //    var messageCount = _infoQueue.NumStoredMessages;
        //    for (int i = 0; i < messageCount; i++)
        //    {
        //        var message = _infoQueue.GetMessage(i);
        //        LOG.WARNING($"D3D12 Debug: {message.Description}");
        //    }
        //    _infoQueue.ClearStoredMessages();
        //}

        public static void AddRendererCallback(RendererCallback callback)
        {
            _RendererCallBacks.Add(callback);
        }

        public static void AddWindowProcedureCallback(WindowProcedureCallback callback)
        {
            _WindowProcedureCallbacks.Add(callback);
        }

        public static void DX12OnPresent()
        {
            GetInstance().DX12OnPresentImpl();
        }
        private void DX12OnPresentImpl()
        {
            try
            {
                DX12NewFrame();
                // test of toggling imgui
                bool open = GUIMgr.IsOpen();
                if (open)
                {
                    ImGui.ShowDemoWindow(ref open);
                    if (open != GUIMgr.IsOpen())
                    {
                        GUIMgr.SetOpen(open);
                    }
                }
                DX12EndFrame();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Error in DX12NewFrame or DX12EndFrame: {ex}");
                return;
            }

        }

        //public static void DX12NewFrame()
        //{
        //    GetInstance().DX12NewFrameImpl();
        //}

        private void DX12NewFrame()
        {
            try
            {
                ImGui.ImGuiImplDX12NewFrame();

            }
            catch (Exception ex)
            {
                LOG.ERROR($"ImGuiImplDX12NewFrame() failed: {ex}");
            }
            try
            {
                ImGui.ImGuiImplWin32NewFrame();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"ImGuiImplWin32NewFrame() failed: {ex}");
            }
            try
            {
                ImGui.NewFrame();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"ImGui.NewFrame() failed: {ex}");
            }


        }

        private void DX12EndFrame()
        {
            if (_frameStopwatch == null || _SwapChain == null || _CommandList == null || _CommandQueue == null)
            {
                LOG.ERROR("Critical objects are null in DX12EndFrameImpl");
                LOG.ERROR($"Stopwatch: {_frameStopwatch}\n_SwapChain: {_SwapChain}\n_CommandList: {_CommandList}\nCommandQueue: {_CommandQueue}");
                return;
            }

            WaitForNextFrame();

            var currentFrameContext = GetInstance()._FrameContext[GetInstance()._SwapChain.CurrentBackBufferIndex];
            currentFrameContext.CommandAllocator.Reset();

            ResourceBarrier barrier = new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.BackBuffer, -1, ResourceStates.Present, ResourceStates.RenderTarget));
            GetInstance()._CommandList.Reset(currentFrameContext.CommandAllocator, null);
            GetInstance()._CommandList.ResourceBarrier(barrier);
            GetInstance()._CommandList.SetRenderTargets(1, currentFrameContext.RtvDescriptor, null);
            GetInstance()._CommandList.SetDescriptorHeaps(GetInstance()._DescriptorHeap);

            ImGui.Render();
            ImDrawData drawData = ImGui.GetDrawData();
            if (drawData != null)
            {
                ImGui.ImGuiImplDX12RenderDrawData(drawData, _CommandList.NativePointer);
            }

            GetInstance()._CommandList.ResourceBarrier(new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.BackBuffer, -1, ResourceStates.RenderTarget, ResourceStates.Present)));

            //LOG.INFO($"Closing command list for frame {_FrameIndex}");
            GetInstance()._CommandList.Close();

            //LOG.INFO($"Executing command list for frame {_FrameIndex}");
            _CommandQueue.ExecuteCommandList(_CommandList);

            ulong fenceValue = _FenceLastSignaledValue + 1;
            _CommandQueue.Signal(_Fence, (long)fenceValue);
            _FenceLastSignaledValue = fenceValue;
            currentFrameContext.FenceValue = fenceValue;

            //LOG.INFO($"Frame {_FrameIndex} signaled with fence value {fenceValue}");

            //try
            //{
            //    LOG.INFO($"Presenting frame {_FrameIndex}");
            //    _SwapChain.Present(0, PresentFlags.None);
            //    LOG.INFO($"Frame {_FrameIndex} presented successfully");
            //}
            //catch (SharpDXException ex)
            //{
            //    LOG.ERROR($"Failed to present frame: {ex.Descriptor}");
            //    LOG.ERROR($"Device removed: {_Device.DeviceRemovedReason}");
            //    //HandleDeviceRemoved();
            //    throw;
            //}
        }

        //private void PresentFrame()
        //{
        //        try
        //        {
        //            LOG.INFO($"Presenting frame {_FrameIndex}");
        //            _SwapChain.Present(1, 0);
        //            LOG.INFO($"Frame {_FrameIndex} presented successfully");
        //    }
        //        catch(SharpDXException ex)
        //        {
        //            LOG.ERROR($"Failed to present frame: {ex.Descriptor}");
        //            LOG.ERROR($"Device removed: {_Device.DeviceRemovedReason}");
        //            //HandleDeviceRemoved();
        //            throw;
        //        }
        //}

        // P/Invoke signature for WaitForMultipleObjects
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForMultipleObjects(
            uint nCount,
            IntPtr[] lpHandles,
            bool bWaitAll,
            uint dwMilliseconds);

        // Constants
        private const uint INFINITE = 0xFFFFFFFF;
        private const uint WAIT_OBJECT_0 = 0x00000000;
        private const uint WAIT_FAILED = 0xFFFFFFFF;

        private void WaitForNextFrameImpl()
        {
            // testing without waiting for swapchain
            ulong NextFrameIndex = _FrameIndex + 1;
            _FrameIndex = NextFrameIndex;

            FrameContext frameContext = _FrameContext[(int)NextFrameIndex % _SwapChainDesc.BufferCount];
            ulong fenceValue = frameContext.FenceValue;

            if (fenceValue != 0) // no fence was signaled
            {
                frameContext.FenceValue = 0;
                _Fence.SetEventOnCompletion((long)fenceValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                _FenceEvent.WaitOne(); // Wait for the GPU to complete the work
            }
        }
        //private void WaitForNextFrameImpl()
        //{
        //    _SwapchainWaitableObject = _SwapChain.FrameLatencyWaitableObject;
        //    ulong NextFrameIndex = GetInstance()._FrameIndex + 1;
        //    GetInstance()._FrameIndex = NextFrameIndex;
        //    IntPtr[] waitableObjects = { GetInstance()._SwapchainWaitableObject, IntPtr.Zero };
        //    int numWaitableObjects = 1;


        //    FrameContext frameContext = GetInstance()._FrameContext[(int)NextFrameIndex % GetInstance()._SwapChainDesc.BufferCount];
        //    ulong fenceValue = frameContext.FenceValue;
        //    if (fenceValue != 0) // no fence was signaled
        //    {
        //        frameContext.FenceValue = 0;
        //        GetInstance()._Fence.SetEventOnCompletion((long)fenceValue, _FenceEvent.GetSafeWaitHandle().DangerousGetHandle());
        //        waitableObjects[1] = GetInstance()._FenceEvent.GetSafeWaitHandle().DangerousGetHandle();
        //        numWaitableObjects = 2;
        //    }

        //    if (_SwapchainWaitableObject == IntPtr.Zero || _SwapchainWaitableObject == new IntPtr(-1))
        //    {
        //        LOG.ERROR("Invalid _SwapchainWaitableObject handle");
        //    }
        //    if (_FenceEvent.SafeWaitHandle.IsClosed || _FenceEvent.SafeWaitHandle.IsInvalid)
        //    {
        //        LOG.ERROR("Invalid _FenceEvent handle");
        //    }

        //    // Call WaitForMultipleObjects
        //    uint result = WaitForMultipleObjects((uint)numWaitableObjects, waitableObjects, true, INFINITE);

        //    // Error handling
        //    if (result >= WAIT_OBJECT_0 && result < WAIT_OBJECT_0 + numWaitableObjects)
        //    {
        //        LOG.WARNING("All waitable objects were signaled.");
        //    }
        //    else if (result == WAIT_FAILED)
        //    {
        //       LOG.ERROR("WaitForMultipleObjects failed with error code: " + Marshal.GetLastWin32Error());
        //        return;
        //    }
        //    else
        //    {
        //        LOG.WARNING("Unexpected wait result: " + result);
        //    }
        //}

        //private void HandleDeviceRemoved()
        //{
        //    //var removeReason = _Device.DeviceRemovedReason;
        //    //LOG.ERROR($"Device removed. Reason: {removeReason}");

        //    // Release all DirectX resources
        //    ReleaseResources();

        //    int retryCount = 0;
        //    while (retryCount < 3)
        //    {
        //        if (RecreateDeviceAndResources())
        //        {
        //            LOG.INFO("Device and resources successfully recreated");
        //            return;
        //        }
        //        retryCount++;
        //        LOG.WARNING($"Failed to recreate device and resources. Attempt {retryCount} of 3");
        //        Thread.Sleep(1000); // Wait a second before retrying
        //    }

        //    LOG.ERROR("Failed to recreate device after multiple attempts. Application needs to restart.");
        //}

        private void ReleaseResources()
        {

            ImGui.ImGuiImplWin32Shutdown();

            ImGui.ImGuiImplDX12Shutdown();

            LOG.INFO("Releasing DirectX resources...");

            // Wait for GPU to finish all work
            WaitForGpu();

            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext[i].CommandAllocator?.Dispose();
                _FrameContext[i].BackBuffer?.Dispose();
            }

            _CommandList?.Dispose();
            _RtvDescriptorHeap?.Dispose();
            _DescriptorHeap?.Dispose();
            _Fence?.Dispose();
            _FenceEvent?.Dispose();
            _SwapChain?.Dispose();
            _Device?.Dispose();

        LOG.INFO("All DirectX resources released");
        }

        private void WaitForGpu()
        {
            _FenceLastSignaledValue++;
            _CommandQueue.Signal(_Fence, (long)_FenceLastSignaledValue);
            if (_Fence.CompletedValue < (long)_FenceLastSignaledValue)
            {
                _Fence.SetEventOnCompletion((long)_FenceLastSignaledValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                _FenceEvent.WaitOne();
            }
        }

        //    private bool RecreateDeviceAndResources()
        //{
        //    LOG.INFO("Recreating device and resources...");

        //    if (!CreateDX12Resources())
        //    {
        //        LOG.ERROR("Failed to recreate DX12 resources");
        //        return false;
        //    }

        //    // Reset the command list to prepare for the next frame
        //    _CommandList.Reset(_FrameContext[0].CommandAllocator, null);

        //    try
        //    {
        //        ImGui.ImGuiImplWin32Init(Memory.PointerData.Hwnd);
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.ERROR($"Failed to initialise ImGui_Win32: {ex}");
        //        return false;
        //    }
        //    try
        //    {
        //        IntPtr cpuHandle = _DescriptorHeap.CPUDescriptorHandleForHeapStart.Ptr;
        //        var gpuHandle = _DescriptorHeap.GPUDescriptorHandleForHeapStart.Ptr;
        //        ImGui.ImGuiImplDX12Init(_Device.NativePointer, _SwapChainDesc.BufferCount, DearImguiSharp.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8UNORM, _DescriptorHeap.NativePointer, cpuHandle, gpuHandle);
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.ERROR($"Failed to initialise ImGui_DX12: {ex}");
        //        return false;
        //    }

        //    LOG.INFO("Device and resources recreated successfully");
        //    return true;
        //}

        //public static void DX12EndFrame()
        //{
        //    GetInstance().DX12EndFrameImpl();
        //}

        public static void WaitForNextFrame()
        {
            GetInstance().WaitForNextFrameImpl();
        }

        public static void WaitForLastFrame()
        {
            GetInstance().WaitForLastFrameImpl();
        }

        private void WaitForLastFrameImpl()
        {
            FrameContext currentFrameContext = GetInstance()._FrameContext[
     (GetInstance()._FrameIndex % (ulong)GetInstance()._SwapChainDesc.BufferCount)];
            ulong fenceValue = currentFrameContext.FenceValue;
            if (fenceValue == 0)
            {
                return;
            }

            currentFrameContext.FenceValue = 0;

            if ((ulong)GetInstance()._Fence.CompletedValue >= fenceValue)
            {
                return;
            }
            GetInstance()._Fence.SetEventOnCompletion((long)fenceValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
            GetInstance()._FenceEvent.WaitOne(Timeout.Infinite);
            LOG.INFO($"Waited for last frame");
        }

        public static void DX12PreResize()
        {
            GetInstance().DX12PreResizeImpl();
        }

        private void DX12PreResizeImpl()
        {
            try
            {
                SetResizing(true);
                WaitForLastFrame();
                ImGui.ImGuiImplDX12InvalidateDeviceObjects();
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    if (_FrameContext[i].BackBuffer != null)
                    {
                        _FrameContext[i].BackBuffer.Dispose();
                        _FrameContext[i].BackBuffer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.ERROR($"An error occurred in DX12PreResize: {ex.Message}");
                return;
            }
        }

        public static void DX12PostResize()
        {
            GetInstance().DX12PostResizeImpl();
        }

        private void DX12PostResizeImpl()
        {
            try
            {
                // Recreate our pointers and ImGui's
                ImGui.ImGuiImplDX12CreateDeviceObjects();
                int rtvDescriptorSize = _Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
                CpuDescriptorHandle rtvHandle = _RtvDescriptorHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    SharpDX.Direct3D12.Resource backBuffer;
                    _FrameContext[i].RtvDescriptor = rtvHandle;
                    using (backBuffer = _SwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i))
                    {
                        _Device.CreateRenderTargetView(backBuffer, null, rtvHandle);
                        _FrameContext[i].BackBuffer = backBuffer;
                        rtvHandle.Ptr += rtvDescriptorSize;
                    }
                }
                SetResizing(false);
            }
            catch (Exception ex)
            {
                LOG.ERROR($"An error occurred in DX12PostResize: {ex.Message}");
                return;
            }
        }



        private bool InitImpl()
        {
            if (Memory.PointerData.IsVulkan)
            {
                LOG.ERROR("Sorry, this doesn't support Vulkan. Switch to DX12 if you can.");
                return false;
            }
            else if (!Memory.PointerData.IsVulkan)
            {
                LOG.WARNING("Using DX12. Clear shader cache if you are having issues.");
                LOG.WARNING("Sleeping for 5s to avoid errors");
                Thread.Sleep(5000);
                return InitDX12();
            }

            return false;
        }

        public static bool Init()
        {
            return GetInstance().InitImpl();
        }

        public static void Destroy()
        {
            GetInstance().Dispose();
        }
        public void Dispose()
        {
            ImGui.ImGuiImplWin32Shutdown();
            if (PointerData.IsVulkan)
            {
                WaitForLastFrame();
                ImGui.ImGuiImplDX12Shutdown();
            }
            ImGui.DestroyContext(null);
            //_FrameContext.Clear();

            //_CommandList?.Dispose();
            //_CommandAllocator?.Dispose();
            //_BackbufferDescriptorHeap?.Dispose();
            //_DescriptorHeap?.Dispose();
            //_Fence?.Dispose();
            //_FenceEvent?.Dispose();
            //_Device?.Dispose();
            //_SwapChain?.Dispose();
            //_GameSwapChain?.Dispose();
            //_CommandQueue?.Dispose();

            //// Set all disposed objects to null
            //_CommandList = null;
            //_CommandAllocator = null;
            //_BackbufferDescriptorHeap = null;
            //_DescriptorHeap = null;
            //_Fence = null;
            //_FenceEvent = null;
            //_Device = null;
            //_SwapChain = null;
            //_GameSwapChain = null;
            //_CommandQueue = null;
        }
    }
}
