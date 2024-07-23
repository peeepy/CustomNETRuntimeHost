using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestCS
{
    internal static class NativeScriptMgr
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void CallbackDelegate();

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_Init();

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_Destroy();

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_Tick();

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_Yield(long duration);

        [DllImport("RDONatives.dll")]
        internal static extern bool ScriptMgr_CanTick();

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_AddScript(CallbackDelegate callback);

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_RegisterCallback(string eventName, CallbackDelegate callback);

        [DllImport("RDONatives.dll")]
        internal static extern void ScriptMgr_TriggerCallback(string eventName);
    }

    public class ScriptManager : IDisposable
    {
        private static readonly Lazy<ScriptManager> _instance = new(() => new ScriptManager());

        private static List<GCHandle> _callbackHandles = new List<GCHandle>();

        private bool _disposed = false; // To detect redundant calls

        // Private constructor to prevent instantiation
        private ScriptManager()
        {
            NativeScriptMgr.ScriptMgr_Init();
        }

        public static ScriptManager Instance => _instance.Value;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                NativeScriptMgr.ScriptMgr_Destroy();
                foreach (var handle in _callbackHandles)
                {
                    if (handle.IsAllocated)
                        handle.Free();
                }
                _callbackHandles.Clear();
            }
            _disposed = true;
        }

        ~ScriptManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            NativeScriptMgr.ScriptMgr_Init();
        }

        public void Tick()
        {
            NativeScriptMgr.ScriptMgr_Tick();
        }

        public void Yield(TimeSpan? duration = null)
        {
            long durationNanoseconds = duration?.Ticks * 100 ?? -1;
            NativeScriptMgr.ScriptMgr_Yield(durationNanoseconds);
        }

        public bool CanTick()
        {
            return NativeScriptMgr.ScriptMgr_CanTick();
        }

        public void AddScript(Action script)
        {
            NativeScriptMgr.CallbackDelegate nativeCallback = () =>
            {
                try
                {
                    script();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Exception in script callback: {ex.Message}");
                }
            };

            GCHandle handle = GCHandle.Alloc(nativeCallback);
            _callbackHandles.Add(handle);

            NativeScriptMgr.ScriptMgr_AddScript(nativeCallback);
        }

        public void RegisterCallback(string eventName, Action callback)
        {
            NativeScriptMgr.CallbackDelegate nativeCallback = () =>
            {
                try
                {
                    callback();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Exception in callback for event {eventName}: {ex.Message}");
                }
            };

            GCHandle handle = GCHandle.Alloc(nativeCallback);
            _callbackHandles.Add(handle);

            NativeScriptMgr.ScriptMgr_RegisterCallback(eventName, nativeCallback);
        }

        public void TriggerCallback(string eventName)
        {
            NativeScriptMgr.ScriptMgr_TriggerCallback(eventName);
        }
    }
}
