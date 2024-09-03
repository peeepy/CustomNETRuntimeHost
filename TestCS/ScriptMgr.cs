using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestCS
{
    public class ScriptHandle : IDisposable
    {
        internal NativeScriptMgr.CallbackDelegate Callback { get; }
        private GCHandle _handle;

        public ScriptHandle(Action userCallback)
        {
            Callback = () =>
            {
                try
                {
                    userCallback();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in script callback: {ex.Message}");
                }
            };
            _handle = GCHandle.Alloc(Callback);
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }
    }

    public class ScriptManager : IDisposable
    {
        private List<ScriptHandle> _scriptHandles = new List<ScriptHandle>();
        private bool _disposed = false;
        private static ScriptManager _instance;
        private ScriptManager()
        {
        }

        private static ScriptManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ScriptManager();
            }
            return _instance;
        }

        public static void Init()
        {
            GetInstance().InitImpl();
        }

        private void InitImpl()
        {
            NativeScriptMgr.ScriptMgr_Init();
        }

        public static void AddScript(Action script)
        {
            GetInstance().AddScriptImpl(script);
        }

        private void AddScriptImpl(Action script)
        {
            var handle = new ScriptHandle(script);
            _scriptHandles.Add(handle);
            NativeScriptMgr.ScriptMgr_AddScript(handle.Callback);
        }

        public static void Tick()
        {
            NativeScriptMgr.ScriptMgr_Tick();
        }

        public static void Yield(TimeSpan? duration = null)
        {
            long durationNanoseconds = duration?.Ticks * 100 ?? -1;
            NativeScriptMgr.ScriptMgr_Yield(durationNanoseconds);
        }

        public static bool CanTick()
        {
            return NativeScriptMgr.ScriptMgr_CanTick();
        }

        public static void Destroy()
        {
            GetInstance().Dispose();
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                NativeScriptMgr.ScriptMgr_Destroy();

                foreach (var handle in _scriptHandles)
                {
                    handle.Dispose();
                }
                _scriptHandles.Clear();

                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~ScriptManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    foreach (var handle in _scriptHandles)
                    {
                        handle.Dispose();
                    }
                    _scriptHandles.Clear();
                }

                // Dispose unmanaged resources
                NativeScriptMgr.ScriptMgr_Destroy();

                _disposed = true;
            }
        }
    }

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
}