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
using Vortice.Vulkan;
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
        private IDXGISwapChain3? _SwapChain;
        private ID3D12Device? _Device;
        private ID3D12CommandQueue? _CommandQueue;
        private ID3D12CommandAllocator? _CommandAllocator;
        private ID3D12GraphicsCommandList? _CommandList;
        private ID3D12DescriptorHeap? _BackbufferDescriptorHeap;
        private ID3D12DescriptorHeap? _DescriptorHeap;
        private ID3D12Fence? _Fence;


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
        private List<QueueFamilyProperties> _VKQueueFamilies;
        private ImGui_ImplVulkanH_Frame[] _VkFrames = new ImGui_ImplVulkanH_Frame[8];
        private ImGui_ImplVulkanH_FrameSemaphores[] m_VkFrameSemaphores = new ImGui_ImplVulkanH_FrameSemaphores[8];

        private unsafe void InitVulkan()
        {
            InstanceCreateInfo instanceInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                EnabledExtensionCount = (UInt32)InstanceExtensions.Count,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(InstanceExtensions.ToArray())
            };

            try
            {
                if (_vk.CreateInstance(instanceInfo, _VkAllocator, out _Vkinstance) != Result.Success)
                {
                    LOG.ERROR("Vulkan Instance could not be created");
                }
            }
            catch(Exception e)
            {
                LOG.ERROR($"{e}");
            }

            UInt32 GpuCount;

            try
            {
                if (_vk.EnumeratePhysicalDevices(_Vkinstance, &GpuCount, null) != Result.Success)
                {
                    LOG.ERROR("vkEnumeratePhysicalDevices failed");
                }
            }
            catch (Exception e)
            {
                LOG.ERROR($"{e}");
            }

            // TODO: Add device creation methods, etc

            //DestroyDevice(_VkFakeDevice, _VkAllocator); // can't do this?
            ImGui.CreateContext();
            ImGui_ImplWin32_Init(Pointers.Hwnd);


        }
        

        static void VkSetScreenSize(Extent2D extent)
        {
            GetInstance()._VkImageExtent = extent;
        }
    }
}
