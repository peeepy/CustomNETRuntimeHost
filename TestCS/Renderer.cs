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

namespace TestCS
{
    public class FrameContext
    {
        public SharpDX.Direct3D12.CommandAllocator CommandAllocator { get; set; }
        public SharpDX.Direct3D12.Resource Resource { get; set; }
        public SharpDX.Direct3D12.CpuDescriptorHandle Descriptor { get; set; }
        public ulong FenceValue { get; set; }
    }


    public class Renderer
    {
        private Renderer() { }

        private static Renderer GetInstance()
        {
            return new Renderer();
        }

        // DX12
        private List<FrameContext> _FrameContext;
        private SwapChainDescription _SwapChainDesc;
        //private SwapChain1 _GameSwapChain;
        private SwapChain1 _SwapChain;
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

        // Vulkan
        private static List<String> InstanceExtensions = ["VK_KHR_surface"];
        private static List<String> ValidationLayers = ["VK_LAYER_KHRONOS_validation"];

        [StructLayout(LayoutKind.Sequential)]
        private struct ImGui_ImplVulkanH_Frame
        {
            public CommandPool CommandPool;       // VkCommandPool
            public CommandBuffer CommandBuffer;     // VkCommandBuffer
            public Silk.NET.Vulkan.Fence Fence;             // VkFence
            public Image Backbuffer;        // VkImage
            public ImageView BackbufferView;    // VkImageView
            public Framebuffer Framebuffer;       // VkFramebuffer
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ImGui_ImplVulkanH_FrameSemaphores
        {
            Silk.NET.Vulkan.Semaphore ImageAcquiredSemaphore;
            Silk.NET.Vulkan.Semaphore RenderCompleteSemaphore;
        };
        private Vk _vk = Vk.GetApi();
        private Instance _Vkinstance;
        private PhysicalDevice _VkphysicalDevice;
        private Silk.NET.Vulkan.Device _VkDevice;
        private Silk.NET.Vulkan.Device _VkFakeDevice;
        private Extent2D _VkImageExtent;
        private RenderPass _VkRenderPass;
        private DescriptorPool _VkDescriptorPool;
        private PipelineCache _VkPipelineCache;
        private UInt32 _VkMinImageCount = 2;
        private AllocationCallbacks _VkAllocator;
        private UInt32 _VkQueueFamily;
        private List<QueueFamilyProperties> _VKQueueFamilies;
        private ImGui_ImplVulkanH_Frame[] _VkFrames = new ImGui_ImplVulkanH_Frame[8];
        private ImGui_ImplVulkanH_FrameSemaphores[] _VkFrameSemaphores = new ImGui_ImplVulkanH_FrameSemaphores[8];

        private unsafe bool InitDX12()
        {
            if (Memory.PointerData.SwapChain == nint.Zero)
            {
                LOG.WARNING("SwapChain pointer is invalid!");

                return false;
            }

            if (Memory.PointerData.CommandQueue == nint.Zero)
            {
                LOG.WARNING("CommandQueue pointer is invalid!");

                return false;
            }

            // Create SwapChain3 directly from the pointer
            _SwapChain = new SwapChain1(Memory.PointerData.SwapChain);
            if (_SwapChain == null)
            {
                LOG.WARNING("WARNING: Dereferenced SwapChain pointer is invalid!");
                return false;
            }

            //// TODO: Handle proper disposal; need to manually dispose correctly
            //_GameSwapChain = new SwapChain1(Memory.PointerData.SwapChain);
            //if (_GameSwapChain == null)
            //{
            //    LOG.WARNING("WARNING: Dereferenced SwapChain pointer is invalid!");
            //    return false;
            //}

            // CommandQueue
            _CommandQueue = new CommandQueue(Memory.PointerData.CommandQueue);
            if (_CommandQueue == null)
            {
                LOG.WARNING("WARNING: Dereferenced CommandQueue pointer is invalid!");
                return false;
            }

           
            try
            {
                _SwapChain.GetDevice(typeof(SharpDX.Direct3D12.Device).GUID, out IntPtr devicePtr);
                _Device = new SharpDX.Direct3D12.Device(devicePtr);
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to get D3D12 Device with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}");
                return false;
            }

            // Get the swap chain description
            _SwapChainDesc = _SwapChain.Description;

            try
            {
                // Create the fence using SharpDX
                _Fence = _Device.CreateFence(0, FenceFlags.None);
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Fence with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}");
                return false;
            }

            // Create the fence event using AutoResetEvent
            _FenceEvent = new AutoResetEvent(false);
            if (_FenceEvent == null)
            {
                LOG.ERROR("Failed to create Fence Event!");
                return false;
            }

            // NOTE: May need to fill in the framecontext struct
            // Add instances to the list based on the number of swap chain buffers
            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext.Add(new FrameContext { });
            }

            DescriptorHeapDescription DescriptorDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = _SwapChainDesc.BufferCount,
                Flags = DescriptorHeapFlags.ShaderVisible,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView
            };
            try
            {
                _DescriptorHeap = _Device.CreateDescriptorHeap(DescriptorDesc);
            }
            catch (SharpDXException ex)
            {
                LOG.WARNING($"Failed to create Descriptor Heap with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}]");
                return false;
            }


            try
            {
                _CommandAllocator = _Device.CreateCommandAllocator(CommandListType.Direct);
            }
            catch (SharpDXException ex)
            {
                LOG.WARNING($"Failed to create Command Allocator with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}");
                return false;
            }

            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                _FrameContext[i].CommandAllocator = _CommandAllocator;
            }


            // Create the command list.
            try
            {
                _CommandList = _Device.CreateCommandList(CommandListType.Direct, _CommandAllocator, null);
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to create Command List with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}");
                return false;
            }

            try
            {
                _CommandList.Close();
            }
            catch (SharpDXException ex)
            {
                LOG.ERROR($"Failed to finalize the creation of Command List with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}");
                return false;
            }


            DescriptorHeapDescription DescriptorBackbufferDesc = new DescriptorHeapDescription()
            {
                DescriptorCount = _SwapChainDesc.BufferCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };
            try
            {
                _BackbufferDescriptorHeap = _Device.CreateDescriptorHeap(DescriptorBackbufferDesc);
            }
            catch (SharpDXException ex)
            {
                LOG.WARNING($"Failed to create Backbuffer Descriptor Heap with result: [{ex.ResultCode}] - {ex.ResultCode.ToString()}]");
                return false;
            }

            int RTVDescriptorSize = _Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
            CpuDescriptorHandle RTVHandle = _BackbufferDescriptorHeap.CPUDescriptorHandleForHeapStart;
            for (int i = 0; i < _SwapChainDesc.BufferCount; i++)
            {
                SharpDX.Direct3D12.Resource BackBuffer;
                _FrameContext[i].Descriptor = RTVHandle;
                BackBuffer = _SwapChain.GetBackBuffer<SharpDX.Direct3D12.Resource>(i);
                _Device.CreateRenderTargetView(BackBuffer, null, RTVHandle);
                _FrameContext[i].Resource = BackBuffer;
                RTVHandle.Ptr = RTVDescriptorSize;
            }

            //ImGui.CreateContext();
            //TODO: Actually export these from imgui and create P/Invoke wrappers
            //Natives.ImGui.ImGui_ImplWin32_Init(Memory.PointerData.Hwnd)
            //Natives.ImGui.ImGui_ImplDX12_Init(_Device,
            //_SwapChainDesc.BufferCount,
            //DXGI_FORMAT_R8G8B8A8_UNORM // DXGI_FORMAT 28?,
            //_DescriptorHeap,
            //_DescriptorHeap.CPUDescriptorHandleForHeapStart, 
            //_DescriptorHeap.GPUDescriptorHandleForHeapStart);

            //ImGui.StyleColorsDark();
            LOG.INFO("DirectX 12 renderer has finished initializing.");
            return true;
        }

        public static void DX12OnPresent()
        {
            Renderer.GetInstance().DX12OnPresentImpl();
        }

        private void DX12OnPresentImpl()
        {
            LOG.INFO("Hooked DX12 SwapChain Present");

        }

        //private unsafe bool InitVulkan()
        //{
        //    InstanceCreateInfo instanceInfo = new InstanceCreateInfo
        //    {
        //        SType = StructureType.InstanceCreateInfo,
        //        EnabledExtensionCount = (UInt32)InstanceExtensions.Count,
        //        PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(InstanceExtensions.ToArray())
        //    };

        //    try
        //    {
        //        if (_vk.CreateInstance(ref instanceInfo, ref _VkAllocator, out _Vkinstance) != Silk.NET.Vulkan.Result.Success)
        //        {
        //            LOG.WARNING("Vulkan Instance could not be created");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LOG.ERROR($"{e}");
        //    }

        //    //UInt32 GpuCount;

        //    try
        //    {
        //        if (_vk.EnumeratePhysicalDevices(_Vkinstance, null, null) != Silk.NET.Vulkan.Result.Success)
        //        {
        //            LOG.WARNING("vkEnumeratePhysicalDevices failed");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LOG.ERROR($"{e}");
        //    }

        //    //select discrete gpu - if none is available use first device
        //    var devices = _vk.GetPhysicalDevices(_Vkinstance);
        //    foreach (var gpu in devices)
        //    {
        //        var properties = _vk.GetPhysicalDeviceProperties(gpu);
        //        if (properties.DeviceType == PhysicalDeviceType.DiscreteGpu) _VkphysicalDevice = gpu;
        //    }
        //    if (_VkphysicalDevice.Handle == 0) _VkphysicalDevice = devices.First();
        //    var deviceProps = _vk.GetPhysicalDeviceProperties(_VkphysicalDevice);
        //    LOG.INFO($"Using GPU: {SilkMarshal.PtrToString((nint)deviceProps.DeviceName)}");

        //    var queueFamilyCount = 0u; /// uint32
        //    _vk.GetPhysicalDeviceQueueFamilyProperties(_VkphysicalDevice, ref queueFamilyCount, null);
        //    var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
        //    fixed (QueueFamilyProperties* pQueueFamilies = queueFamilies)
        //        _vk.GetPhysicalDeviceQueueFamilyProperties(_VkphysicalDevice, ref queueFamilyCount, pQueueFamilies);

        //    for (var i = queueFamilyCount; i < queueFamilies.Length; i++)
        //    {
        //        if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
        //        {
        //            _VkQueueFamily = i;
        //            break;
        //        }
        //    }

        //    float QueuePriority = 1.0f;

        //    var queueCreateInfo = new DeviceQueueCreateInfo
        //    {
        //        SType = StructureType.DeviceQueueCreateInfo,
        //        QueueCount = 1,
        //        QueueFamilyIndex = _VkQueueFamily,
        //        PQueuePriorities = &QueuePriority
        //    };

        //    List<String> enabledDeviceExtensions = ["VK_KHR_swapchain"];
        //    byte** DeviceExtension = (byte**)SilkMarshal.StringArrayToPtr(enabledDeviceExtensions.ToArray());

        //    var deviceCreateInfo = new DeviceCreateInfo
        //    {
        //        SType = StructureType.DeviceCreateInfo,
        //        QueueCreateInfoCount = (UInt32)1,
        //        EnabledExtensionCount = 1,
        //        PpEnabledExtensionNames = DeviceExtension
        //    };

        //    if (_vk.CreateDevice(_VkphysicalDevice, ref deviceCreateInfo, ref _VkAllocator, out _VkFakeDevice) != Result.Success)
        //        throw new Exception("Could not create device");

        //    Memory.PointerData.QueuePresentKHR = (IntPtr)_vk.GetDeviceProcAddr(_VkFakeDevice, "vkQueuePresentKHR");
        //    Memory.PointerData.CreateSwapchainKHR = (IntPtr)_vk.GetDeviceProcAddr(_VkFakeDevice, "vkCreateSwapchainKHR");
        //    Memory.PointerData.AcquireNextImageKHR = (IntPtr)_vk.GetDeviceProcAddr(_VkFakeDevice, "vkAcquireNextImageKHR");
        //    Memory.PointerData.AcquireNextImage2KHR = (IntPtr)_vk.GetDeviceProcAddr(_VkFakeDevice, "vkAcquireNextImage2KHR");


        //    _vk.DestroyDevice(_VkFakeDevice, ref _VkAllocator);

        //    ImGui.CreateContext();
        //    //TODO: Export from Imgui and create P/Invoke wrapper
        //    //ImGui_ImplWin32_Init(Memory.PointerData.Hwnd);

        //    LOG.INFO("Vulkan renderer has initialised successfully.");
        //    return true;
        //}


        //void VkCreateRenderTarget(Silk.NET.Vulkan.Device device, SwapchainKHR Swapchain)
        //{
        //    uint imageCount = 0; // is assigned value of num of images from below func

        //    // TODO: find correct GetSwapchainImagesKHR func
        //    //Silk.NET.Vulkan.Result result = _vk.GetSwapchainImagesKHR(device, Swapchain, ref imageCount, null);
        //    //if (result != Silk.NET.Vulkan.Result.Success)
        //    //{
        //    //    LOG.WARNING($"vkGetSwapchainImagesKHR failed with result: [{result}]");
        //    //    return;
        //    //}

        //    Image[] BackBuffers = new Image[8];
        //    //result = _vk.GetSwapchainImagesKHR(device, Swapchain, ref imageCount, swapchainImages);
        //    //if (result != Silk.NET.Vulkan.Result.Success)
        //    //{
        //    //    LOG.WARNING($"vkGetSwapchainImagesKHR 2 failed with result: [{result}]");
        //    //    return;
        //    //}

        //    for (uint i = 0; i < imageCount; i++)
        //    {
        //        _VkFrames[i].Backbuffer = BackBuffers[i];

        //        ImGui_ImplVulkanH_Frame fd = _VkFrames[i];
        //        ImGui_ImplVulkanH_FrameSemaphores fsd = _VkFrameSemaphores[i];
        //        {
        //            CommandPoolCreateInfo info = new CommandPoolCreateInfo
        //            {
        //                SType = StructureType.CommandPoolCreateInfo,
        //                Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
        //                QueueFamilyIndex = _VkQueueFamily
        //            };

        //            Silk.NET.Vulkan.Result result2 = _vk.CreateCommandPool(device, in info, in _VkAllocator, out fd.CommandPool);
        //            if (result2 != Silk.NET.Vulkan.Result.Success)
        //            {
        //                LOG.WARNING($"vkCreateCommandPool failed with result: [{result2}]");
        //                return;
        //            }
        //        }
        //        {
        //            CommandBufferAllocateInfo info = new CommandBufferAllocateInfo
        //            {
        //                SType = StructureType.CommandBufferAllocateInfo,
        //                CommandPool = fd.CommandPool,
        //                Level = CommandBufferLevel.Primary,
        //                CommandBufferCount = 1
        //            };

        //            Silk.NET.Vulkan.Result result = _vk.AllocateCommandBuffers(device, in info, out fd.CommandBuffer);
        //            if (result != Silk.NET.Vulkan.Result.Success)
        //            {
        //                LOG.WARNING($"vkAllocateCommandBuffers failed with result: [{result}]");
        //                return;
        //            }
        //        }

        //    }


        //}
        //static void VkSetScreenSize(Extent2D extent)
        //{
        //    GetInstance()._VkImageExtent = extent;
        //}

        private bool InitImpl()
        {
            if (Memory.PointerData.IsVulkan)
            {
                LOG.INFO("Using Vulkan");
                return false;
            }
            else if (!Memory.PointerData.IsVulkan)
            {
                LOG.INFO("Using DX12. Clear shader cache if you are having issues.");
                LOG.INFO("Sleeping for 5s to avoid errors...");
                Thread.Sleep(5000);
                return InitDX12();
            }

            return false;
        }

        public static bool Init()
        {
            return GetInstance().InitImpl();
        }
    }
}
