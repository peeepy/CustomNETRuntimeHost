using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using SharpDX.DXGI;
using SharpDX.Direct3D12;
using System.Collections;
using SharpDX;
using TestCS.Natives.ImGui;

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
        public delegate void WindowProcedureCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private bool _Resizing;
        private static SortedDictionary<uint, RendererCallback> _RendererCallBacks = new SortedDictionary<uint, RendererCallback>();
        private static List<WindowProcedureCallback> _WindowProcedureCallbacks = new List<WindowProcedureCallback>();

        public static Renderer GetInstance()
        {
            return new Renderer();
        }
        
        public static bool IsResizing()
        {
            return GetInstance()._Resizing;
        }
        public static void SetResizing(bool status)
        {
            GetInstance()._Resizing = status;
        }

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

            LOG.INFO("DirectX12 has initialised successfully.");
            return true;
        }



        public bool AddRendererCallback(RendererCallback callback, uint priority)
        {
            if (_RendererCallBacks.ContainsKey(priority))
            {
                return false;
            }
            _RendererCallBacks.Add(priority, callback);
            return true;
        }

        public void AddWindowProcedureCallback(WindowProcedureCallback callback)
        {
            _WindowProcedureCallbacks.Add(callback);
        }



        public static int WndProc(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam)
        {
            foreach (var callback in _WindowProcedureCallbacks)
            {
                callback(hwnd, msg, wparam, lparam);
            }
            return ImGui_ImplWin32_WndProcHandler(hwnd, msg, wparam, lparam);
        }

        public static void DX12OnPresent()
        {
            DX12NewFrame();
            foreach (var callback in _RendererCallBacks.Values)
            {
                callback();
            }
        }



        public static void DX12NewFrame()
        {
            DX12.ImGui_ImplDX12_NewFrame();
            Win32.ImGui_ImplWin32_NewFrame();
            ImGui.NewFrame();
        }

        public void DX12EndFrame()
        {
            WaitForNextFrame();
            var currentFrameContext = _FrameContext[_SwapChain.CurrentBackBufferIndex];
            currentFrameContext.CommandAllocator.Reset();

            // Create initial resource barrier (Present to RenderTarget)
            var barrierToRenderTarget = new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.Resource, ResourceStates.Present, ResourceStates.RenderTarget));

            _CommandList.Reset(currentFrameContext.CommandAllocator, null);
            _CommandList.ResourceBarrier(barrierToRenderTarget);
            _CommandList.SetRenderTargets(1, currentFrameContext.Descriptor, null);
            _CommandList.SetDescriptorHeaps(1, new[] { _DescriptorHeap });

            // Render ImGui
            ImGui.Render();
            unsafe
            {
                ImDrawDataPtr drawDataPtr = ImGui.GetDrawData();
                IntPtr nativePtr = (IntPtr)drawDataPtr.NativePtr;
                DX12.ImGui_ImplDX12_RenderDrawData(nativePtr, _CommandList.NativePointer);
            }

            // Create new barrier for transitioning back to Present
            var barrierToPresent = new ResourceBarrier(new ResourceTransitionBarrier(currentFrameContext.Resource, ResourceStates.RenderTarget, ResourceStates.Present));
            _CommandList.ResourceBarrier(barrierToPresent);

            _CommandList.Close();
            _CommandQueue.ExecuteCommandList(_CommandList);

            ulong fenceValue = _FenceLastSignaledValue + 1;
            _CommandQueue.Signal(_Fence, (long)fenceValue);
            _FenceLastSignaledValue = fenceValue;
            currentFrameContext.FenceValue = fenceValue;

            _SwapChain.Present(1, 0);
        }

        public void WaitForNextFrame()
        {
            ulong nextFrameIndex = _FrameIndex + 1;
            _FrameIndex = nextFrameIndex;

            IntPtr[] waitableObjects = new IntPtr[2];
            waitableObjects[0] = _SwapchainWaitableObject;
            int numWaitableObjects = 1;

            FrameContext frameCtx = _FrameContext[(int)nextFrameIndex % _SwapChainDesc.BufferCount];
            ulong fenceValue = frameCtx.FenceValue;

            if (fenceValue != 0) // means a fence was signaled
            {
                frameCtx.FenceValue = 0;
                _Fence.SetEventOnCompletion((long)fenceValue, _FenceEvent.SafeWaitHandle.DangerousGetHandle());
                waitableObjects[1] = _FenceEvent.SafeWaitHandle.DangerousGetHandle();
                numWaitableObjects = 2;
            }

            WaitHandle.WaitAll(Array.ConvertAll(waitableObjects.Take(numWaitableObjects).ToArray(), h => new AutoResetEvent(false) { Handle = h }));
        }

        public void WaitForLastFrame()
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



        public void DX12PreResize()
        {
            SetResizing(true);
            WaitForLastFrame();
            DX12.ImGui_ImplDX12_InvalidateDeviceObjects();

            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                if (_FrameContext[i].Resource != null)
                {
                    _FrameContext[i].Resource.Dispose();
                    _FrameContext[i].Resource = null;
                }
            }
        }

        public void DX12PostResize()
        {
            // Recreate our pointers and ImGui's
            DX12.ImGui_ImplDX12_CreateDeviceObjects();

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
                return InitDX12();
            }

            return false;
        }

        public static bool Init()
        {
            return GetInstance().InitImpl();
        }

        public void Dispose()
        {
            
            _FrameContext.Clear();

            _CommandList?.Dispose();
            _CommandAllocator?.Dispose();
            _BackbufferDescriptorHeap?.Dispose();
            _DescriptorHeap?.Dispose();
            _Fence?.Dispose();
            _FenceEvent?.Dispose();
            _Device?.Dispose();
            _SwapChain?.Dispose();
            _GameSwapChain?.Dispose();
            _CommandQueue?.Dispose();

            // Set all disposed objects to null
            _CommandList = null;
            _CommandAllocator = null;
            _BackbufferDescriptorHeap = null;
            _DescriptorHeap = null;
            _Fence = null;
            _FenceEvent = null;
            _Device = null;
            _SwapChain = null;
            _GameSwapChain = null;
            _CommandQueue = null;
        }
    }
}
