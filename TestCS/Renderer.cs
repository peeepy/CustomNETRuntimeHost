using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Vortice.DXGI;
using Vortice.Direct3D12;
using Vortice.DXCore;
using System.Collections;
using Silk.NET.Core.Native;
using Win32;
using System.Reflection;

namespace TestCS
{
    public class Renderer
    {
        private Renderer() { }

        private static Renderer GetInstance()
        {
            return new Renderer();
        }

        [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
        internal static unsafe extern bool ImGui_ImplWin32_Init(void* hwnd);

        // DX12
        private SwapChainDescription _SwapChainDesc;
        private IDXGISwapChain1 _GameSwapChain;
        private IDXGISwapChain3 _SwapChain;
        private ID3D12Device _Device;
        private ID3D12CommandQueue _CommandQueue;
        private ID3D12CommandAllocator _CommandAllocator;
        private ID3D12GraphicsCommandList _CommandList;
        private ID3D12DescriptorHeap _BackbufferDescriptorHeap;
        private ID3D12DescriptorHeap _DescriptorHeap;
        private ID3D12Fence _Fence;


        // Vulkan
        private static List<String> InstanceExtensions = ["VK_KHR_surface"];
        private static List<String> ValidationLayers = ["VK_LAYER_KHRONOS_validation"];

        [StructLayout(LayoutKind.Sequential)]
        private struct ImGui_ImplVulkanH_Frame
        {
            public CommandPool CommandPool;       // VkCommandPool
            public CommandBuffer CommandBuffer;     // VkCommandBuffer
            public Fence Fence;             // VkFence
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
        private Device _VkDevice;
        private Device _VkFakeDevice;
        private Extent2D _VkImageExtent;
        private RenderPass _VkRenderPass;
        private DescriptorPool _VkDescriptorPool;
        private PipelineCache _VkPipelineCache;
        private UInt32 _VkMinImageCount = 2;
        private AllocationCallbacks _VkAllocator;
        private UInt32 _VkQueueFamily;
        private List<QueueFamilyProperties> _VKQueueFamilies;
        private ImGui_ImplVulkanH_Frame[] _VkFrames = new ImGui_ImplVulkanH_Frame[8];
        private ImGui_ImplVulkanH_FrameSemaphores[] m_VkFrameSemaphores = new ImGui_ImplVulkanH_FrameSemaphores[8];

        private unsafe bool InitDX12()
        {
            if (!Pointers.SwapChain)
            {
                LOG.WARNING("SwapChain pointer is invalid!");

                return false;
            }

            if (!Pointers.CommandQueue)
            {
                LOG.WARNING("CommandQueue pointer is invalid!");

                return false;
            }

            _GameSwapChain = new IDXGISwapChain1(Pointers.SwapChain);
            if (_GameSwapChain == null)
            {
                LOG.WARNING("WARNING: Dereferenced SwapChain pointer is invalid!");
                return false;
            }

            // CommandQueue
            _CommandQueue = new ID3D12CommandQueue(Pointers.CommandQueue);
            if (_CommandQueue == null)
            {
                LOG.WARNING("WARNING: Dereferenced CommandQueue pointer is invalid!");
                return false;
            }

            // Query for IDXGISwapChain3 interface
            _SwapChain = _GameSwapChain.QueryInterface<IDXGISwapChain3>();
            if (_SwapChain == null)
            {
                Console.WriteLine("Failed to query IDXGISwapChain3 interface");
                return false;
            }

            Win32.HResult result = _SwapChain.GetDevice(typeof(ID3D12Device).GUID, out IntPtr devicePtr);
            if (result.Failure)
            {
                Console.WriteLine($"Failed to get D3D12 Device with result: [{result}]");
                return false;
            }

            _Device = new ID3D12Device(devicePtr);
        

        // Get the swap chain description
        _SwapChainDesc = _SwapChain.Description;

            if (_Device->CreateFence(0, FenceFlags.None, __uuidof(ID3D12Fence), (void**)_Fence.GetAddressOf()); result < 0)
		{
                LOG(WARNING) << "Failed to create Fence with result: [" << result << "]";

                return false;
            }

            if (const auto result = m_FenceEvent = CreateEventA(nullptr, FALSE, FALSE, nullptr); !result)
		{
                LOG(WARNING) << "Failed to create Fence Event!";

                return false;
            }
        }
        private unsafe bool InitVulkan()
        {
            InstanceCreateInfo instanceInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                EnabledExtensionCount = (UInt32)InstanceExtensions.Count,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(InstanceExtensions.ToArray())
            };

            try
            {
                if (_vk.CreateInstance(ref instanceInfo, ref _VkAllocator, out _Vkinstance) != Result.Success)
                {
                    LOG.ERROR("Vulkan Instance could not be created");
                }
            }
            catch(Exception e)
            {
                LOG.ERROR($"{e}");
            }

            //UInt32 GpuCount;

            try
            {
                if (_vk.EnumeratePhysicalDevices(_Vkinstance, null, null) != Result.Success)
                {
                    LOG.ERROR("vkEnumeratePhysicalDevices failed");
                }
            }
            catch (Exception e)
            {
                LOG.ERROR($"{e}");
            }

            // TODO: Add device creation methods, etc
            //select discrete gpu - if none is available use first device
            var devices = _vk.GetPhysicalDevices(_Vkinstance);
            foreach (var gpu in devices)
            {
                var properties = _vk.GetPhysicalDeviceProperties(gpu);
                if (properties.DeviceType == PhysicalDeviceType.DiscreteGpu) _VkphysicalDevice = gpu;
            }
            if (_VkphysicalDevice.Handle == 0) _VkphysicalDevice = devices.First();
            var deviceProps = _vk.GetPhysicalDeviceProperties(_VkphysicalDevice);
            LOG.INFO($"Using GPU: {SilkMarshal.PtrToString((nint)deviceProps.DeviceName)}");

            var queueFamilyCount = 0u; /// uint32
            _vk.GetPhysicalDeviceQueueFamilyProperties(_VkphysicalDevice, ref queueFamilyCount, null);
            var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
            fixed (QueueFamilyProperties* pQueueFamilies = queueFamilies)
                _vk.GetPhysicalDeviceQueueFamilyProperties(_VkphysicalDevice, ref queueFamilyCount, pQueueFamilies);

            for (var i = queueFamilyCount; i < queueFamilies.Length; i++)
            {
                if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    _VkQueueFamily = i;
                    break;
                }
            }

            float QueuePriority = 1.0f;

            var queueCreateInfo = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueCount = 1,
                QueueFamilyIndex = _VkQueueFamily,
                PQueuePriorities = &QueuePriority
            };

            List<String> enabledDeviceExtensions = ["VK_KHR_swapchain"];
            byte** DeviceExtension = (byte**)SilkMarshal.StringArrayToPtr(enabledDeviceExtensions.ToArray());

            var deviceCreateInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = (UInt32)1,
                EnabledExtensionCount = 1,
                PpEnabledExtensionNames = DeviceExtension
            };

            if (_vk.CreateDevice(_VkphysicalDevice, ref deviceCreateInfo, ref _VkAllocator, out _VkFakeDevice) != Result.Success)
                throw new Exception("Could not create device");

            //Pointers.QueuePresentKHR = reinterpret_cast<void*>(vkGetDeviceProcAddr(m_VkFakeDevice, "vkQueuePresentKHR"));
            //Pointers.CreateSwapchainKHR = reinterpret_cast<void*>(vkGetDeviceProcAddr(m_VkFakeDevice, "vkCreateSwapchainKHR"));
            //Pointers.AcquireNextImageKHR = reinterpret_cast<void*>(vkGetDeviceProcAddr(m_VkFakeDevice, "vkAcquireNextImageKHR"));
            //Pointers.AcquireNextImage2KHR = reinterpret_cast<void*>(vkGetDeviceProcAddr(m_VkFakeDevice, "vkAcquireNextImage2KHR"));

            _vk.DestroyDevice(_VkFakeDevice, ref _VkAllocator);
            ImGui.CreateContext();
            //ImGui_ImplWin32_Init(Pointers.Hwnd);

            LOG.INFO("Vulkan renderer has initialised successfully.");
            return true;
        }



        static void VkSetScreenSize(Extent2D extent)
        {
            GetInstance()._VkImageExtent = extent;
        }
    }
}
