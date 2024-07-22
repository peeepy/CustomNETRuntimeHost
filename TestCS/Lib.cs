using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using Natives;

namespace TestCS
{

    public static class Program
    {

        [UnmanagedCallersOnly]
        public static void Main()
        {
            Init("TestCS", "D:\\Coding\\csharp\\CustomNETRuntimeHost\\output\\init.log");
            LOG.INFO("Hello from C#! This console was created by the injected DLL.");
            LOG.INFO($"Current time is: {DateTime.Now}");
            // this doesn't work
            try
            {
                LOG.INFO($"The nearest rounded number to 74.5 is: {BUILTIN.ROUND(74.5f)}");

            }
            catch (Exception ex)
            {
                LOG.WARNING($"ROUND() could not be called: {ex}");
            }
        }
    }
}