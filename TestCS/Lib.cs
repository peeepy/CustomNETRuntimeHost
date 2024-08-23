using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
//using Natives;
using TestCS.Memory;
using static TestCS.Memory.PointerData;
using TestCS.Hooking;

namespace TestCS
{

    public static class Program
    {

        [UnmanagedCallersOnly]
        public static void Main()
        {
            Logger.Init("TestCS", "D:\\Coding\\csharp\\CustomNETRuntimeHost\\output\\init.log");
            LOG.INFO($"Current time is: {DateTime.Now}");
            ModuleManager.Instance.LoadModules();
            LOG.INFO("Loaded modules.");
            LOG.INFO("Scanning for pointers...");
            //TODO: Fix blocking of thread when this is initialising
            //LOG.INFO($"Am I using Vulkan? Result: {Pointers.Instance.IsVulkan}");
            //LOG.INFO($"Hwnd pointer: {Pointers.Instance.Hwnd
            Pointers.Init(); 
        }

        //private static async Task InitializeEverythingAsync()
        //{
        //    Memory.ModuleManager.LoadModules();
        //    await Pointers.Instance.InitAsync();
        //    try
        //    {
        //        bool initialized = await Pointers.Instance.WaitForInitializationAsync();
        //        if (initialized)
        //        {
        //            LOG.INFO("Pointers initialised.");
        //        }
        //        else
        //        {
        //            LOG.ERROR("Pointers initialization failed");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.WARNING($"Error during pointers initialization or checking: {ex}");
        //    }
        //}
    }
}