using System;
using System;
using System.Runtime.InteropServices;
using EasyHook;

namespace TestCS.Hooking
{
    public class DetourHook<T> : BaseHook where T : Delegate
    {
        private IntPtr _targetFunc;
        private T _detourFunc;
        private T _originalFunc;
        private LocalHook _hook;

        public DetourHook(string name, IntPtr target, T detour) : base(name)
        {
            _targetFunc = target;
            _detourFunc = detour ?? throw new ArgumentNullException(nameof(detour));

            try
            {
                _hook = LocalHook.Create(_targetFunc, _detourFunc, this);
                _originalFunc = Marshal.GetDelegateForFunctionPointer<T>(_hook.HookBypassAddress);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create hook!", ex);
            }
        }

        public override bool Enable()
        {
            if (_enabled)
                return false;

            try
            {
                _hook.ThreadACL.SetExclusiveACL(new int[0]);
                _enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to enable hook.", ex);
            }
        }

        public override bool Disable()
        {
            if (!_enabled)
                return false;

            try
            {
                _hook.ThreadACL.SetInclusiveACL(new int[0]);
                _enabled = false;
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to disable hook.", ex);
            }
        }

        public T Original => _originalFunc;
    }
}