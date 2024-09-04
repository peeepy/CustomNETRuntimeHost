using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCS.Memory;

namespace TestCS.Hooking.Hooks.Protections
{
    public class Protections
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HandleNetGameEventDelegate(IntPtr eventMgr, IntPtr sourcePlayer, IntPtr targetPlayer, NetEventType type, int index, int handledBits, short unk, IntPtr buffer);
        public DetourHook<HandleNetGameEventDelegate> HandleNetGameEventHook { get; }

        //                            rage::netEventMgr*   CNetGamePlayer*    CNetGamePlayer*                                                                rage::datBitBuffer*
        public static void HandleNetGameEvent(IntPtr eventMgr, IntPtr sourcePlayer, IntPtr targetPlayer, NetEventType type, int index, Int16 handledBits, short unk, IntPtr buffer)
        {
            if (type == NetEventType.NETWORK_PTFX_EVENT)
            {
                //LOG.WARNING($"Blocked NETWORK_PTFX_EVENT from {sourcePlayer.GetName}"); //todo add
                LOG.WARNING($"Blocked NETWORK_PTFX_EVENT");
                PointerData.SendEventAck(eventMgr, IntPtr.Zero, sourcePlayer, targetPlayer, index, handledBits);
                return;
            }
            BaseHook.Get<DetourHook<HandleNetGameEventDelegate>>(HandleNetGameEvent).Original(eventMgr, sourcePlayer, targetPlayer, type, index, handledBits, unk, buffer);
        }
    }
}
