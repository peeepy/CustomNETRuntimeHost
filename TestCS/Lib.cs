using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
//using Natives;
using TestCS.Memory;
using static TestCS.Memory.PointerData;
using TestCS.Hooking;
using TestCS.GUI;

namespace TestCS
{

    public static class Program
    {

        [UnmanagedCallersOnly]
        public static void Main()
        {
            Logger.Init("TestCS", "D:\\Coding\\csharp\\output2\\init.log");
            if (!ModuleManager.LoadModules())
            {
                Unload();
            }
            if (!Pointers.Init())
            {
                Unload();
            }
            if (!Renderer.Init()) {
                Unload();
            }

            GUIMgr.Init();

            Hooking.Hooking.Init();

            ScriptManager.Init();

            LOG.INFO("Script manager initialised.");

            FiberPool.Init(5);
            LOG.INFO("Fiber pool initialised.");
           

            //try
            //{
            //    Notifications.Show("TestCS", "Loaded succesfully", NotificationType.Success);
            //}
            //catch (Exception ex)
            //{
            //    LOG.ERROR($"Failed to show notification: {ex}");
            //}
            // add try/catch for fiberpool push notification
            
        }
        private static void Unload()
        {
            Hooking.Hooking.Destroy();
            LOG.INFO("Hooking uninitialised");
            Renderer.Destroy();
            LOG.INFO("Renderer uninitialised");
        }
    }
}