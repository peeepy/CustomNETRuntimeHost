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
            Logger.Init("TestCS", "D:\\Coding\\csharp\\output2\\init.log");
            LOG.INFO($"Current time is: {DateTime.Now}");
            ModuleManager.Instance.LoadModules();
            LOG.INFO("Loaded modules.");
            LOG.INFO("Scanning for pointers...");
            Pointers.Init();
            Renderer.Init();
            Hooking.Hooking.Init();
            ScriptManager.Instance.Init();
            LOG.INFO("Script manager initialised.");
        }
    }
}