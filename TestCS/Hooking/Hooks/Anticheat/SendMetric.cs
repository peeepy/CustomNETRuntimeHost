using rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Hooking.Hooks.Anticheat
{
    public class SendMetricHook
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate bool SendMetricDelegate(IntPtr manager, rage.rlMetric* metric);
        public static DetourHook<SendMetricDelegate> SendMetricDelegateHook { get; }
        public static unsafe bool SendMetric(IntPtr manager, rage.rlMetric* metric)
        {
            char[] buffer = new char[256];

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = handle.AddrOfPinnedObject();
            uint bufferSize = (uint)(buffer.Length * sizeof(char));

            rlJson serializer = new rlJson(bufferPtr, bufferSize);

            LOG.INFO($"METRIC: {metric->GetName()}; DATA: {Marshal.PtrToStringUni(serializer.GetBuffer())}");

            handle.Free();

            return true;
        }

    }
}
