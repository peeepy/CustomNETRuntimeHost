using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Hooking
{
    public abstract class BaseHook
    {
        private readonly string _name;
        protected bool _enabled;

        public string Name => _name;
        public bool IsEnabled => _enabled;

        protected BaseHook(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public abstract bool Enable();
        public abstract bool Disable();

        // Static members
        private static readonly Dictionary<Delegate, BaseHook> _hooks = new Dictionary<Delegate, BaseHook>();

        public static void Add<T>(Delegate hookFunc, T hook) where T : BaseHook
        {
            _hooks[hookFunc] = hook;
        }

        public static T Get<T>(Delegate hookFunc) where T : BaseHook
        {
            return _hooks.TryGetValue(hookFunc, out var hook) ? (T)hook : null;
        }

        public static IReadOnlyCollection<BaseHook> Hooks => _hooks.Values;

        public static void EnableAll()
        {
            foreach (var hook in _hooks.Values)
            {
                hook.Enable();
            }
        }

        public static void DisableAll()
        {
            foreach (var hook in _hooks.Values)
            {
                hook.Disable();
            }
        }
    }
}
