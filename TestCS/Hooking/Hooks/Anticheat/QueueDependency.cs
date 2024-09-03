using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TestCS.Hooking;

namespace TestCS.Hooking.Hooks.Anticheat
{
    public class QueueDependencyHook
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        private static IntPtr moduleBase = IntPtr.Zero;
        private static long moduleSize = 0;

        private static bool IsAddressInGameRegion(IntPtr address)
        {
            if (moduleBase == IntPtr.Zero || moduleSize == 0)
            {
                MODULEINFO info;
                if (!GetModuleInformation(Process.GetCurrentProcess().Handle, GetModuleHandle(null), out info, (uint)Marshal.SizeOf(typeof(MODULEINFO))))
                {
                    Console.WriteLine("GetModuleInformation failed!");
                    return true;
                }
                else
                {
                    moduleBase = info.lpBaseOfDll;
                    moduleSize = info.SizeOfImage;
                }
            }
            return address.ToInt64() > moduleBase.ToInt64() && address.ToInt64() < (moduleBase.ToInt64() + moduleSize);
        }

        private static bool IsJumpInstruction(IntPtr fptr)
        {
            if (!IsAddressInGameRegion(fptr))
                return false;
            byte value = Marshal.ReadByte(fptr);
            return value == 0xE9;
        }

        private static bool IsUnwantedDependency(IntPtr cb)
        {
            IntPtr f1 = Marshal.ReadIntPtr(cb);
            IntPtr f2 = Marshal.ReadIntPtr(cb + 0x78);
            if (!IsAddressInGameRegion(f1) || (f2 != IntPtr.Zero && !IsAddressInGameRegion(f2)))
                return false;
            return IsJumpInstruction(f1) || IsJumpInstruction(f2);
        }

        public delegate void QueueDependencyDelegate(IntPtr dependency);

        public static void QueueDependency(IntPtr dependency)
        {
            if (IsUnwantedDependency(dependency))
            {
                LOG.INFO($"Caught unwanted dependency: RDR2.exe+{(Marshal.ReadIntPtr(dependency).ToInt64() - GetModuleHandle(null).ToInt64()):X}");
                return;
            }
            BaseHook.Get<DetourHook<QueueDependencyDelegate>>(QueueDependency).Original(dependency);
        }
    }
}
