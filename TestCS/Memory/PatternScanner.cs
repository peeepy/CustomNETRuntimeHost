using System;
using System.Runtime.InteropServices;

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

        public async Task<bool> ScanAsync()
        {
            if (m_Module == null || !m_Module.Valid())
                return false;

            var tasks = m_Patterns.Select(pair =>
                Task.Run(() => ScanInternal(pair.Key, pair.Value))).ToList();

            var results = await Task.WhenAll(tasks);

            bool scanSuccess = results.All(result => result);

            if (!scanSuccess)
            {
                LOG.ERROR("Some patterns have not been found, continuing would be foolish.");
                // In a real application, you might want to use a proper logging framework
                // and possibly throw an exception here.
            }

            return scanSuccess;
        }

        public unsafe bool ScanInternal(IPattern pattern, PatternFunc func)
        {
            ReadOnlySpan<byte?> signature = pattern.Signature;
            for (nuint i = m_Module.Base; i < m_Module.End; i += 1)
            {
                if (i + (nuint)signature.Length > m_Module.End)
                    break;
                byte* instruction = (byte*)i.ToPointer();
                bool found = true;
                for (int instructionIdx = 0; instructionIdx < signature.Length; ++instructionIdx)
                {
                    if (signature[instructionIdx].HasValue && signature[instructionIdx].Value != instruction[instructionIdx])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    LOG.INFO($"Found pattern [{pattern.Name}] : [0x{i:X}]");
                    PointerCalculator ptr = new PointerCalculator((IntPtr)i);
                    func.Invoke(ptr);
                    return true;
                }
            }
            LOG.WARNING($"Failed to find pattern [{pattern.Name}]");
            return false;
        }
    }
}
