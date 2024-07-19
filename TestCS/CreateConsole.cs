using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestCS
{
    internal class NewConsole
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateConsoleScreenBuffer(uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwFlags, IntPtr lpScreenBufferData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleActiveScreenBuffer(IntPtr hConsoleOutput);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);

        public static void CreateConsole()
        {
            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            const uint FILE_SHARE_READ = 0x00000001;
            const uint FILE_SHARE_WRITE = 0x00000002;
            const uint CONSOLE_TEXTMODE_BUFFER = 1;

            IntPtr hConsole = CreateConsoleScreenBuffer(
                GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                CONSOLE_TEXTMODE_BUFFER,
                IntPtr.Zero);

            if (hConsole == IntPtr.Zero)
            {
                File.WriteAllText(@"D:\CODING\csharp\CustomNETRuntimeHost\output\error.log", "Failed to create console buffer");
                return;
            }

            if (!SetConsoleActiveScreenBuffer(hConsole))
            {
                File.WriteAllText(@"D:\CODING\csharp\CustomNETRuntimeHost\output\error.log", "Failed to set console active screen buffer");
                return;
            }

            string message = "Hello from C#! This console was created by the injected DLL.\r\n";
            message += $"Current time is: {DateTime.Now}\r\n";
            message += "Press any key to close this console...\r\n";

            uint written;
            WriteConsole(hConsole, message, (uint)message.Length, out written, IntPtr.Zero);

            // Keep the console open until a key is pressed
            Console.ReadKey();
        }
        
    }
}
