using System;
using System.Collections.Generic;
using System.Threading;

namespace TestCS
{
    public class FiberPool : IDisposable
    {
        private static readonly Lazy<FiberPool> _instance = new Lazy<FiberPool>(() => new FiberPool());
        private readonly object _lock = new object();
        private readonly Queue<Action> _jobs = new Queue<Action>();
        private bool _disposed = false;

        private FiberPool() { }

        private static FiberPool _Instance;
        private static FiberPool GetInstance()
        {
            if (_Instance == null)
            {
                _Instance = new FiberPool();
            }
            return _Instance;
        }
        public static void Init(int numFibers)
        {
            GetInstance().InitImpl(numFibers);
        }

        public static void Push(Action callback)
        {
            GetInstance().PushImpl(callback);
        }

        private void InitImpl(int numFibers)
        {
            for (int i = 0; i < numFibers; i++)
            {
                ScriptManager.AddScript(ScriptEntry);
            }
        }

        private void PushImpl(Action callback)
        {
            lock (_lock)
            {
                _jobs.Enqueue(callback);
            }
        }

        public static void Tick()
        {
            GetInstance().TickImpl();
        }
        private void TickImpl()
        {
            Action job = null;
            lock (_lock)
            {
                if (_jobs.Count > 0)
                {
                    job = _jobs.Dequeue();
                }
            }

            try
            {
                job?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in FiberPool job: {ex.Message}");
            }
        }
        public static void ScriptEntry()
        {
            GetInstance().ScriptEntryImpl();
        }
        private void ScriptEntryImpl()
        {
            while (!_disposed)
            {
                Tick();
                ScriptManager.Yield();
            }
        }

        public static void Destroy()
        {
            GetInstance().Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                lock (_lock)
                {
                    _jobs.Clear();
                }
            }
        }
    }
}