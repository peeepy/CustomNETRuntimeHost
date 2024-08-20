using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestCS.Memory
{
    public class PatternScanner
    {
        private readonly Module m_Module;
        private readonly Dictionary<IPattern, PatternFunc> m_Patterns;

        // Define a delegate type for the pattern function
        public delegate void PatternFunc(PointerCalculator ptr);

        public PatternScanner(Module module)
        {
            m_Module = module;
            m_Patterns = new Dictionary<IPattern, PatternFunc>();
        }

        public void AddPattern(IPattern pattern, PatternFunc func)
        {
            m_Patterns[pattern] = func;
        }

        public void ScanAsync(Action<bool> callback)
        {
            if (!m_Module.Valid())
            {
                callback(false);
                return;
            }

                bool allFound = true;
                foreach (var pattern in m_Patterns.Keys)
                {
                    bool found = ScanInternal(pattern);
                    if (!found)
                    {
                        allFound = false;
                        LOG.ERROR($"Failed to find pattern [{pattern.Name}]");
                    }
                }
                callback(allFound);
        }

        private bool ScanInternal(IPattern pattern)
        {
            IntPtr result;
            var (patternBytes, maskBytes) = pattern.GetNativePattern();
            bool found = NativeModuleFuncs.ScanPatternWrapper(m_Module.Base, m_Module.End, patternBytes, patternBytes.Length, maskBytes, out result);

            if (found)
            {
                LOG.INFO($"Found pattern [{pattern.Name}] : [{result:X}]");
                // Create a PointerCalculator instance with the result
                var pointerCalculator = new PointerCalculator(result);

                // Call the PatternFunc associated with this pattern
                if (m_Patterns.TryGetValue(pattern, out var patternFunc))
                {
                    patternFunc(pointerCalculator);
                }

                return true;
            }
            return false;

        }
            //public IntPtr GetPatternResult(IPattern pattern)
            //{
            //    return m_Patterns.TryGetValue(pattern, out IntPtr result) ? result : IntPtr.Zero;
            //}
        }
}