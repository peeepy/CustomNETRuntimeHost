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
        public SharpDX.Direct3D12.Resource Resource { get; set; }
        public SharpDX.Direct3D12.CpuDescriptorHandle Descriptor { get; set; }
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
        private List<FrameContext> _FrameContext = new List<FrameContext>();
        private SwapChainDescription _SwapChainDesc;
        private SwapChain1 _GameSwapChain;
        private SwapChain3 _SwapChain;
        private SharpDX.Direct3D12.Device _Device;
        private SharpDX.Direct3D12.CommandQueue _CommandQueue;
        private SharpDX.Direct3D12.CommandAllocator _CommandAllocator;
        private SharpDX.Direct3D12.GraphicsCommandList _CommandList;
        private SharpDX.Direct3D12.DescriptorHeap _BackbufferDescriptorHeap;
        private SharpDX.Direct3D12.DescriptorHeap _DescriptorHeap;
        private SharpDX.Direct3D12.Fence _Fence;
        private AutoResetEvent _FenceEvent;
        private ulong _FenceLastSignaledValue;
        private IntPtr _SwapchainWaitableObject;
        private ulong _FrameIndex;

        private DebugInterface _debugInterface;
        private SharpDX.Direct3D12.InfoQueue _infoQueue;

        private bool InitDX12()
        {
            LOG.INFO("Initialising DX12...");

            CreateDX12Resources();

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

        private bool CreateDX12Resources()
        {
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

            //LOG.INFO($"SwapChain pointer-to-pointer value: 0x{Memory.PointerData.SwapChain.ToInt64():X}");
            IntPtr swapChainPtr = IntPtr.Zero;
            try
            {
                swapChainPtr = Marshal.ReadIntPtr(Memory.PointerData.SwapChain);
                //LOG.INFO($"Dereferenced SwapChain pointer value: 0x{swapChainPtr.ToInt64():X}");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to dereference SwapChain pointer: {ex.Message}");
                return false;
            }

            if (swapChainPtr == IntPtr.Zero)
            {
                LOG.ERROR("Dereferenced SwapChain pointer is null!");
                return false;
            }

            //LOG.INFO($"CommandQueue pointer-to-pointer value: 0x{Memory.PointerData.CommandQueue.ToInt64():X}");
            IntPtr commandQueuePtr = IntPtr.Zero;
            try
            {
                commandQueuePtr = Marshal.ReadIntPtr(Memory.PointerData.CommandQueue);
                //LOG.INFO($"Dereferenced CommandQueue pointer value: 0x{commandQueuePtr.ToInt64():X}");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to dereference CommandQueue pointer: {ex.Message}");
                return false;
            }

            if (commandQueuePtr == IntPtr.Zero)
            {
                LOG.ERROR("Dereferenced CommandQueue pointer is null!");
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
                //LOG.INFO("Attempting to create SwapChain object");
                _GameSwapChain = new SwapChain1(swapChainPtr);
                //LOG.INFO("Base SwapChain created successfully.");

                //LOG.INFO("Attempting to query SwapChain3 interface");
                _SwapChain = _GameSwapChain.QueryInterface<SwapChain3>();
                if (_SwapChain == null)
                {
                    LOG.WARNING("Failed to query SwapChain3 interface, falling back to base SwapChain");
                    //_SwapChain = _GameSwapChain;
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

            //// Enable the debug layer before creating the D3D12 device
            //try
            //{
            //    var debugInterface = DebugInterface.Get();
            //    if (debugInterface != null)
            //    {
            //        debugInterface.EnableDebugLayer();
            //        LOG.INFO("Debug layer enabled successfully.");
            //    }
            //    else
            //    {
            //        LOG.WARNING("Failed to obtain Debug Interface.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LOG.ERROR($"Failed to enable debug layer: {ex.Message}");
            //}

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

            //try
            //{
            //    _infoQueue = _Device.QueryInterface<SharpDX.Direct3D12.InfoQueue>();
            //    if (_infoQueue != null)
            //    {
            //        _infoQueue.SetBreakOnSeverity(MessageSeverity.Corruption, true);
            //        _infoQueue.SetBreakOnSeverity(MessageSeverity.Error, true);
            //        _infoQueue.SetBreakOnSeverity(MessageSeverity.Warning, true);
            //        LOG.INFO("InfoQueue configured successfully.");
            //    }
            //    else
            //    {
            //        LOG.WARNING("Failed to get InfoQueue interface, debug messages will be limited.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LOG.ERROR($"Failed to query InfoQueue interface: {ex.Message}");
            //}


            try
            {
                // Get the swap chain description
                _SwapChainDesc = _SwapChain.Description;
                //LOG.INFO("SwapChain Description fetched successfully.");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to get SwapChain Description: {ex.Message}");
                return false;
            }

            if (_Device == null || _Device.NativePointer == IntPtr.Zero)
            {
                LOG.ERROR("Device is null or invalid before creating Fence");
                return false;
            }

            try
            {
                // Create the fence using SharpDX
                _Fence = _Device.CreateFence(0, FenceFlags.None);
                if (_Fence == null)
                {
                    LOG.ERROR("Failed to create Fence: Fence is null");
                    return false;
                }
                //LOG.INFO("Successfully created Fence.");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Fence with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }

            try
            {
                // Create the fence event using AutoResetEvent
                _FenceEvent = new AutoResetEvent(false);
                if (_FenceEvent == null)
                {
                    LOG.ERROR("Failed to create Fence Event: FenceEvent is null");
                    return false;
                }
                //LOG.INFO("Created Fence event.");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to create Fence event: {ex.Message}");
                return false;
            }

            try
            {
                //LOG.INFO($"SwapChainDesc.BufferCount: {_SwapChainDesc.BufferCount}");
                //LOG.INFO($"_FrameContext is null: {_FrameContext == null}");

                if (_FrameContext == null)
                {
                    _FrameContext = new List<FrameContext>();
                    LOG.INFO("Initialized _FrameContext as it was null");
                }

                // Add instances to the list based on the number of swap chain buffers
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    _FrameContext.Add(new FrameContext());
                }
                //LOG.INFO($"Added {_SwapChainDesc.BufferCount} FrameContext instances.");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to add new FrameContexts: {ex.Message}");
                LOG.ERROR($"Exception type: {ex.GetType().Name}");
                LOG.ERROR($"Stack trace: {ex.StackTrace}");
                return false;
            }

            try
            {
                DescriptorHeapDescription DescriptorDesc = new DescriptorHeapDescription()
                {
                    DescriptorCount = _SwapChainDesc.BufferCount,
                    Flags = DescriptorHeapFlags.ShaderVisible,
                    Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
                };
                _DescriptorHeap = _Device.CreateDescriptorHeap(DescriptorDesc);
                if (_DescriptorHeap == null)
                {
                    LOG.ERROR("Failed to create Descriptor Heap: DescriptorHeap is null");
                    return false;
                }
                //LOG.INFO("Successfully created Descriptor Heap.");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Descriptor Heap with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }

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

            try
            {
                DescriptorHeapDescription DescriptorBackbufferDesc = new DescriptorHeapDescription()
                {
                    DescriptorCount = _SwapChainDesc.BufferCount,
                    Flags = DescriptorHeapFlags.None,
                    Type = DescriptorHeapType.RenderTargetView
                };
                _BackbufferDescriptorHeap = _Device.CreateDescriptorHeap(DescriptorBackbufferDesc);
                if (_BackbufferDescriptorHeap == null)
                {
                    LOG.ERROR("Failed to create Backbuffer Descriptor Heap: BackbufferDescriptorHeap is null");
                    return false;
                }
                //LOG.INFO("Successfully created Backbuffer Descriptor Heap");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Backbuffer Descriptor Heap with result: [{ex.ResultCode}] - {ex.Message}");
                return false;
            }

            try
            {
                int RTVDescriptorSize = _Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
                CpuDescriptorHandle RTVHandle = _BackbufferDescriptorHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    SharpDX.Direct3D12.Resource BackBuffer;
                    _FrameContext[i].Descriptor = RTVHandle;
                    BackBuffer = _SwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                    if (BackBuffer == null)
                    {
                        LOG.ERROR($"Failed to get BackBuffer for index {i}: BackBuffer is null");
                        return false;
                    }
                    _Device.CreateRenderTargetView(BackBuffer, null, RTVHandle);
                    _FrameContext[i].Resource = BackBuffer;
                    RTVHandle.Ptr += RTVDescriptorSize;
                }
                //LOG.INFO("Successfully created RenderTargetViews for all back buffers");
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Failed to create RTVHandle or RenderTargetViews: {ex.Message}");
                return false;
            }
            return true;
        }

        private void LogDebugMessages()
        {
            if (_infoQueue == null) return;

            var messageCount = _infoQueue.NumStoredMessages;
            for (int i = 0; i < messageCount; i++)
            {
                var message = _infoQueue.GetMessage(i);
                LOG.WARNING($"D3D12 Debug: {message.Description}");
            }
            _infoQueue.ClearStoredMessages();
        }

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
            }
            catch (Exception ex)
            {
                LOG.ERROR($"DX12NewFrame failed: {ex}");
                return;
            }

            //Menu.Draw();
            
            ImGui.Render();

            try
            {
                DX12EndFrame();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"DX12EndFrame failed: {ex}");
                return;
            }

        }

        public static void DX12NewFrame()
        {
            GetInstance().DX12NewFrameImpl();
        }

        private void DX12NewFrameImpl()
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

        private void DX12EndFrameImpl()
        {
            if (_frameStopwatch == null || _SwapChain == null || _CommandList == null || _CommandQueue == null)
            {
                LOG.ERROR("Critical objects are null in DX12EndFrameImpl");
                return;
            }

            _frameStopwatch.Restart();
            LOG.INFO($"Starting frame {_FrameIndex}");

            try
            {
                PrepareFrame();
                RenderFrame();
                PresentFrame();
                WaitForNextFrame();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Unhandled exception in DX12EndFrameImpl: {ex.Message}");
                LOG.ERROR($"Stack trace: {ex.StackTrace}");
                HandleDeviceRemoved();
            }

            _frameStopwatch.Stop();
            LOG.INFO($"Frame {_FrameIndex} completed in {_frameStopwatch.ElapsedMilliseconds}ms");
            //LogDebugMessages();
        }

        private void PrepareFrame()
        {
            var currentFrameContext = _FrameContext[_SwapChain.CurrentBackBufferIndex];
            currentFrameContext.CommandAllocator.Reset();
            _CommandList.Reset(currentFrameContext.CommandAllocator, null);

            _CommandList.ResourceBarrier(new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.Resource, ResourceStates.Present, ResourceStates.RenderTarget)));

            LOG.INFO($"Frame {_FrameIndex} prepared. Back buffer index: {_SwapChain.CurrentBackBufferIndex}");
        }

        private void RenderFrame()
        {
            _CommandList.SetDescriptorHeaps(1, new[] { _DescriptorHeap });

            var currentFrameContext = _FrameContext[_SwapChain.CurrentBackBufferIndex];
            _CommandList.ClearRenderTargetView(currentFrameContext.Descriptor, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1), 0, null);

            // Render ImGui
            ImDrawData drawData = ImGui.GetDrawData();
            if (drawData != null)
            {
                ImGui.ImGuiImplDX12RenderDrawData(drawData, _CommandList.NativePointer);
            }

            LOG.INFO($"Frame {_FrameIndex} rendered");
        }

        private void PresentFrame()
        {
            try
            {
                var currentFrameContext = _FrameContext[_SwapChain.CurrentBackBufferIndex];
                LOG.INFO($"Setting resource barrier for frame {_FrameIndex}");
                _CommandList.ResourceBarrier(new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.Resource, ResourceStates.RenderTarget, ResourceStates.Present)));

                LOG.INFO($"Closing command list for frame {_FrameIndex}");
                _CommandList.Close();

                LOG.INFO($"Executing command list for frame {_FrameIndex}");
                _CommandQueue.ExecuteCommandList(_CommandList);

                LOG.INFO($"Presenting frame {_FrameIndex}");
                _SwapChain.Present(1, 0);

                LOG.INFO($"Frame {_FrameIndex} presented successfully");
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"SharpDXException in PresentFrame: {ex.Message}");
                if (ex.ResultCode == ResultCode.DeviceRemoved)
                {
                    HandleDeviceRemoved();
                    return;
                }
                throw;
            }
            catch (Exception ex)
            {
                LOG.ERROR($"Unexpected exception in PresentFrame: {ex.Message}");
                throw;
            }
        }

        private void WaitForNextFrameImpl()
        {
            var currentFrameContext = _FrameContext[_SwapChain.CurrentBackBufferIndex];
            ulong fenceValue = _FenceLastSignaledValue + 1;
            _CommandQueue.Signal(_Fence, (long)fenceValue);
            _FenceLastSignaledValue = fenceValue;
            currentFrameContext.FenceValue = fenceValue;

            if ((ulong)_Fence.CompletedValue < currentFrameContext.FenceValue)
            {
                _Fence.SetEventOnCompletion((long)currentFrameContext.FenceValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                _FenceEvent.WaitOne();
            }

            _FrameIndex = (ulong)_SwapChain.CurrentBackBufferIndex;
            LOG.INFO($"Waiting for frame {_FrameIndex} completed");
        }

        private void HandleDeviceRemoved()
        {
            var removeReason = _Device.DeviceRemovedReason;
            LOG.ERROR($"Device removed. Reason: {removeReason}");

            // Release all DirectX resources
            ReleaseResources();

            int retryCount = 0;
            while (retryCount < 3)
            {
                if (RecreateDeviceAndResources())
                {
                    LOG.INFO("Device and resources successfully recreated");
                    return;
                }
                retryCount++;
                LOG.WARNING($"Failed to recreate device and resources. Attempt {retryCount} of 3");
                Thread.Sleep(1000); // Wait a second before retrying
            }

            LOG.ERROR("Failed to recreate device after multiple attempts. Application needs to restart.");
        }

        private void ReleaseResources()
        {

            ImGui.ImGuiImplWin32Shutdown();

            ImGui.ImGuiImplDX12Shutdown();

            LOG.INFO("Releasing DirectX resources...");

            // Wait for GPU to finish all work
            WaitForGpu();

            // Release frame resources
            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext[i].CommandAllocator?.Dispose();
                _FrameContext[i].Resource?.Dispose();
            }

            // Release other DirectX resources
            _CommandList?.Dispose();
            _DescriptorHeap?.Dispose();
            _SwapChain?.Dispose();
            _CommandQueue?.Dispose();
            _Fence?.Dispose();
            _FenceEvent?.Dispose();

            // Release device last
            _Device?.Dispose();

            LOG.INFO("All DirectX resources released");
        }

        private void WaitForGpu()
        {
            // Schedule a Signal command in the queue
            _CommandQueue.Signal(_Fence, (long)++_FenceLastSignaledValue);

            // Wait until the fence has been processed
            if (_Fence.CompletedValue < (long)_FenceLastSignaledValue)
            {
                _Fence.SetEventOnCompletion((long)_FenceLastSignaledValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                _FenceEvent.WaitOne();
            }

            // Reset the frame index to the current back buffer index
            _FrameIndex = (ulong)_SwapChain.CurrentBackBufferIndex;
        }

        private bool RecreateDeviceAndResources()
        {
            LOG.INFO("Recreating device and resources...");

            if (!CreateDX12Resources())
            {
                LOG.ERROR("Failed to recreate DX12 resources");
                return false;
            }

            // Reset the command list to prepare for the next frame
            _CommandList.Reset(_FrameContext[0].CommandAllocator, null);

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

            LOG.INFO("Device and resources recreated successfully");
            return true;
        }

        public static void DX12EndFrame()
        {
            GetInstance().DX12EndFrameImpl();
        }

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
            try
            {
                FrameContext frameCtx = _FrameContext[(int)(_FrameIndex % (ulong)_SwapChainDesc.BufferCount)];
                ulong fenceValue = frameCtx.FenceValue;
                if (fenceValue == 0)
                {
                    return;
                }
                frameCtx.FenceValue = 0;
                if (_Fence.CompletedValue >= (long)fenceValue)
                {
                    return;
                }
                _Fence.SetEventOnCompletion((long)fenceValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                _FenceEvent.WaitOne();
            }
            catch (Exception ex)
            {
                LOG.ERROR($"An error occurred in WaitForLastFrame: {ex.Message}");
                return;
            }
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
                    if (_FrameContext[i].Resource != null)
                    {
                        _FrameContext[i].Resource.Dispose();
                        _FrameContext[i].Resource = null;
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
                CpuDescriptorHandle rtvHandle = _BackbufferDescriptorHeap.CPUDescriptorHandleForHeapStart;
                for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
                {
                    SharpDX.Direct3D12.Resource backBuffer;
                    _FrameContext[i].Descriptor = rtvHandle;
                    using (backBuffer = _SwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i))
                    {
                        _Device.CreateRenderTargetView(backBuffer, null, rtvHandle);
                        _FrameContext[i].Resource = backBuffer;
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
