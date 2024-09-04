using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Silk.NET.Vulkan;
using TestCS.Utils;
using static TestCS.Memory.Functions;

namespace TestCS.Memory
{
    public enum eThreadState : uint
    {
        Idle = 0,
        Running = 1,
        Killed = 2,
        Unk3 = 3,
        Unk4 = 4
    }

    public static class Functions
{
    public delegate IntPtr GetNativeHandler(uint hash);
    public delegate IntPtr GetRendererInfo();
    public delegate void FixVectors(IntPtr call_ctx);
    public delegate IntPtr HandleToPtr(int handle);
    public delegate void SendEventAck(IntPtr eventMgr, IntPtr evnt, IntPtr sourcePlayer, IntPtr targetPlayer, int eventIndex, int handledBitset);
    public delegate int PtrToHandle(IntPtr pointer);
    public delegate IntPtr GetLocalPed();
    public delegate IntPtr GetSyncTreeForType(IntPtr netObjectMgr, ushort type);
    public delegate IntPtr GetNetworkPlayerFromPid(byte player);
    public delegate bool WorldToScreen(IntPtr world_coords, out float out_x, out float out_y);
    public delegate IntPtr GetNetObjectById(ushort id);
    public delegate bool RequestControlOfNetObject(ref IntPtr netId, bool unk);
    public delegate bool SendPacket(IntPtr mgr, IntPtr adde, int connection_id, IntPtr data, int size, int flags);
    public delegate bool QueuePacket(IntPtr mgr, int msg_id, int connection_id, IntPtr data, int size, int flags, IntPtr unk);
    public delegate bool PostPresenceMessage(int localGamerIndex, IntPtr recipients, int numRecipients, string msg, uint ttlSeconds);
    public delegate bool SendNetInfoToLobby(IntPtr player, long a2, long a3, IntPtr a4);
    public delegate bool ReadBitBufferArray(IntPtr buffer, IntPtr read, int bits, int unk);
    public delegate bool WriteBitBufferArray(IntPtr buffer, IntPtr val, int bits, int unk);
    public delegate bool ReadBitBufferString(IntPtr buffer, StringBuilder read, int bits);
    public delegate IntPtr GetAnimSceneFromHandle(IntPtr scene, int handle);
    public delegate IntPtr InventoryEventConstructor(IntPtr existingItem, uint reward_hash, uint model_hash, bool a4, bool a5, IntPtr a6);
    public delegate void TriggerWeaponDamageEvent(IntPtr source, IntPtr target, IntPtr unk, IntPtr position, IntPtr a5, IntPtr a6, bool override_dmg, IntPtr weapon_hash, float damage, float f10, int tire_index, int suspension_index, ulong flags, IntPtr action_result, bool hit_entity_weapon, bool hit_ammo_attachment, bool silenced, bool a18, bool a19, int a20, int a21, int a22, int a23, int a24, int a25);
    public delegate void TriggerGiveControlEvent(IntPtr player, IntPtr obj, int type);
    public delegate uint ScriptVM(IntPtr stack, IntPtr globals, [MarshalAs(UnmanagedType.Bool)] bool globals_enabled, IntPtr program, IntPtr ctx);
    public delegate uint FwScriptGuidCreateGuid(IntPtr arg);
 }

public static class PointerData
{
    // RDR
    public static IntPtr IsSessionStarted { get; set; } // bool*
    public static IntPtr ScriptGlobals { get; set; } // std::int64_t**
    public static IntPtr NativeRegistrationTable { get; set; } // void*
    public static GetNativeHandler GetNativeHandler { get; set; }
    public static FixVectors FixVectors { get; set; }
    public static IntPtr ScriptThreads { get; set; } // TODO: Find out how to handle rage::atArray<rage::scrThread*>* return
    public static IntPtr ScriptPrograms { get; set; } // rage::scrProgram**
    public static IntPtr RunScriptThreads { get; set; } // PVOID
    public static IntPtr CurrentScriptThread { get; set; } // rage::scrThread**
    public static ScriptVM ScriptVM { get; set; } // rage::eThreadState (*)(void* stack, int64_t** globals, bool* globals_enabled, rage::scrProgram* program, rage::scrThreadContext* ctx)
    public static GetLocalPed GetLocalPed { get; set; } // CPed* (*)()
    public static SendPacket SendPacket { get; set; } // bool (*)(rage::netConnectionManager* mgr, rage::netPeerAddress* adde, int connection_id, void* data, int size, int flags)
    public static QueuePacket QueuePacket { get; set; } // bool (*)(rage::netConnectionManager* mgr, int msg_id, int connection_id, void* data, int size, int flags, void* unk)
    public static IntPtr HandlePresenceEvent { get; set; } // PVOID
    public static PostPresenceMessage PostPresenceMessage { get; set; } // bool (*)(int localGamerIndex, rage::rlGamerInfo* recipients, int numRecipients, const char* msg, unsigned int ttlSeconds)
    public static SendNetInfoToLobby SendNetInfoToLobby { get; set; } // bool (*)(rage::rlGamerInfo* player, int64_t a2, int64_t a3, DWORD* a4)
    public static ReadBitBufferArray ReadBitBufferArray { get; set; } // bool (*)(rage::datBitBuffer* buffer, PVOID read, int bits, int unk)
    public static WriteBitBufferArray WriteBitBufferArray { get; set; } // bool (*)(rage::datBitBuffer* buffer, void* val, int bits, int unk)
    public static ReadBitBufferString ReadBitBufferString { get; set; } // bool (*)(rage::datBitBuffer* buffer, char* read, int bits)
    public static IntPtr InitNativeTables { get; set; } // PVOID
    public static TriggerWeaponDamageEvent TriggerWeaponDamageEvent { get; set; } // void (*)(rage::netObject* source, rage::netObject* target, rage::netObject* unk, rage::fvector3* position, void* a5, void* a6, bool override_dmg, std::uint32_t* weapon_hash, float damage, float f10, int tire_index, int suspension_index, std::uint64_t flags, void* action_result, bool hit_entity_weapon, bool hit_ammo_attachment, bool silenced, bool a18, bool a19, int a20, int a21, int a22, int a23, int a24, int a25)
    public static IntPtr ScSession { get; set; } // CNetworkScSession**

    public static IntPtr PedPool { get; set; } // PoolEncryption*
    public static IntPtr ObjectPool { get; set; } // PoolEncryption*
    public static IntPtr VehiclePool { get; set; } // PoolEncryption*
    public static IntPtr PickupPool { get; set; } // PoolEncryption*
    public static FwScriptGuidCreateGuid FwScriptGuidCreateGuid { get; set; }
    public static IntPtr NetworkObjectMgr { get; set; } // CNetworkObjectMgr**

    // Security
    public static IntPtr SendMetric { get; set; } // PVOID
    public static IntPtr RageSecurityInitialized { get; set; } // bool*
    public static IntPtr VmDetectionCallback { get; set; } // PVOID*
    public static IntPtr QueueDependency { get; set; } // PVOID
    public static IntPtr UnkFunction { get; set; } // PVOID

    // Protections
    public static IntPtr HandleNetGameEvent { get; set; } // PVOID
    public static SendEventAck SendEventAck { get; set; } // void (*)(rage::netEventMgr* eventMgr, void* event, CNetGamePlayer* sourcePlayer, CNetGamePlayer* targetPlayer, int eventIndex, int handledBitset)
    public static IntPtr HandleCloneCreate { get; set; } // PVOID
    public static IntPtr HandleCloneSync { get; set; } // PVOID
    public static IntPtr CanApplyData { get; set; } // PVOID
    public static GetSyncTreeForType GetSyncTreeForType { get; set; } // rage::netSyncTree* (*)(void* netObjMgr, uint16_t type)
    public static IntPtr ResetSyncNodes { get; set; } // PVOID
    public static IntPtr HandleScriptedGameEvent { get; set; } // PVOID
    public static IntPtr AddObjectToCreationQueue { get; set; } // PVOID
    public static IntPtr ReceiveNetMessage { get; set; } // PVOID
    public static IntPtr ReceiveServerMessage { get; set; } // PVOID
    public static IntPtr SerializeServerRPC { get; set; } // PVOID
    public static IntPtr ReceiveArrayUpdate { get; set; } // PVOID
    public static IntPtr CreatePoolItem { get; set; } // PVOID

    // Player
    public static IntPtr PlayerHasJoined { get; set; } // PVOID
    public static IntPtr PlayerHasLeft { get; set; } // PVOID
    public static GetNetworkPlayerFromPid GetNetPlayerFromPid { get; set; } // CNetGamePlayer* (*)(uint8_t player)

    // Voice
    public static IntPtr EnumerateAudioDevices { get; set; } // PVOID
    public static IntPtr DirectSoundCaptureCreate { get; set; } // PVOID

    // Native Handles
    public static HandleToPtr HandleToPtr { get; set; } // void* (*)(int handle)
    public static PtrToHandle PtrToHandle { get; set; } // int (*)(void* pointer)
    public static WorldToScreen WorldToScreen { get; set; } // bool (*)(float* world_coords, float* out_x, float* out_y)
    public static GetNetObjectById GetNetObjectById { get; set; } // rage::netObject* (*)(uint16_t id)
    public static RequestControlOfNetObject RequestControlOfNetObject { get; set; } // bool (*)(rage::netObject** netId, bool unk)
    public static GetAnimSceneFromHandle GetAnimSceneFromHandle { get; set; } // CAnimScene** (*)(CAnimScene** scene, int handle)

    // Misc
    public static IntPtr ThrowFatalError { get; set; } // PVOID
    public static IntPtr IsAnimSceneInScope { get; set; } // PVOID
    public static IntPtr BroadcastNetArray { get; set; } // PVOID
    public static IntPtr NetArrayPatch { get; set; } // std::uint8_t*
    public static InventoryEventConstructor InventoryEventConstructor { get; set; } // CEventInventoryItemPickedUp* (*)(CEventInventoryItemPickedUp*, std::uint32_t reward_hash, std::uint32_t model_hash, bool a4, bool a5, void* a6)
    public static IntPtr EventGroupNetwork { get; set; } // CEventGroup**
    public static TriggerGiveControlEvent TriggerGiveControlEvent { get; set; } // void (*)(CNetGamePlayer* player, rage::netObject* object, int type)

    // DX12
    public static IntPtr SwapChain { get; set; } // IDXGISwapChain1**
    public static IntPtr CommandQueue { get; set; } // ID3D12CommandQueue**

    // Misc Renderer
    public static IntPtr Hwnd { get; set; } // HWND

    public static GetRendererInfo GetRendererInfo { get; set; } // RenderingInfo* (*)()
    public static IntPtr GameGraphicsOptions { get; set; } // TODO: Figure out how to handle GraphicsOptions class return type

    public static IntPtr WndProc { get; set; } // PVOID
    public static bool IsVulkan { get; set; } // BOOL

    public static uint ScreenResX { get; set; } // uint32_t
    public static uint ScreenResY { get; set; } // uint32_t

    public static IntPtr NetworkRequest { get; set; } // PVOID

    public static IntPtr NetworkPlayerMgr { get; set; } // CNetworkPlayerMgr*

    public static IntPtr WritePlayerHealthData { get; set; } // PVOID
    public static IntPtr WriteVPMData { get; set; } // PVOID

    // Patches
    public static IntPtr ExplosionBypass { get; set; } // bool*
}


    public class Pointers
    {
       
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public static bool Init()
        {
            Module rdr2 = ModuleManager.Get("RDR2.exe");
            if (rdr2 == null)
            {
                LOG.ERROR("Could not find RDR2.exe.");
                return false;
            }

            PatternScanner scanner = new PatternScanner(rdr2);

            Pattern SwapchainPtrn = new("IDXGISwapChain1", "48 8B 58 60 48 8B 0D");
            scanner.AddPattern(SwapchainPtrn, ptr =>
            {
                try
                {

                    // This is equivalent to IDXGISwapChain1**
                    IntPtr SwapChainPtr = ptr.Add(4).Add(3).Rip().AsIntPtr();
                    PointerData.SwapChain = SwapChainPtr;
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SwapChain pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern CommandQueuePtrn = new("ID3D12CommandQueue", "FF 50 10 48 8B 0D ? ? ? ? 48 8B 01");
            scanner.AddPattern(CommandQueuePtrn, ptr =>
            {
                try
                {
                    PointerData.CommandQueue = ptr.Add(3).Add(3).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with CommandQueue pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern GetRendererInfoPtrn = new("GetRendererInfo", "E8 ? ? ? ? 38 58 09");
            scanner.AddPattern(GetRendererInfoPtrn, ptr =>
            {
                try
                {
                    IntPtr functionPtr = ptr.Add(1).Rip().AsIntPtr();

                    if (functionPtr != IntPtr.Zero)
                    {
                        PointerData.GetRendererInfo = Marshal.GetDelegateForFunctionPointer<Functions.GetRendererInfo>(functionPtr);
                    }
                    else
                    {
                        LOG.ERROR("GetRendererInfo function pointer is null");
                    }
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error in GetRendererInfoPtrn: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            //TODO: Figure out how to handle gfx
            //    Pattern GfxInformationPtrn = new("48 8D 0D ? ? ? ? 48 8B F8 E8 ? ? ? ? 45 33 ED 45 84 FF", "GFXInformation");
            //    scanner.AddPattern(GfxInformationPtrn, ptr =>
            //    {
            //        IntPtr gfx = ptr.Add(3).Rip().AsIntPtr();
            //        if (gfx->_hdr)
            //        {
            //            LOG.WARNING("Turn HDR off if using DX12.")
            //        }
            //        if (gfx->_unk)
            //        {
            //            PointerData.ScreenResX = gfx->_screen_resolution_x;
            //            PointerData.ScreenResY = gfx->_screen_resolution_y;
            //            LOG.INFO($"Screen resolution: {PointerData.ScreenResX} x {PointerData.ScreenResY}");
            //        }
            //    });

            Pattern KeyboardHookPtrn = new("KeyboardHook", "41 83 E9 5B 44 3B C8 76 15");
            scanner.AddPattern(KeyboardHookPtrn, ptr =>
            {
                try
                {
                    UnhookWindowsHookEx(Marshal.ReadIntPtr(ptr.Add(0xC).Rip().AsIntPtr()));
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with KeyboardHook pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern WndProcPtrn = new("WndProc", "48 89 5C 24 ? 4C 89 4C 24 ? 48 89 4C 24 ? 55 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 60");
            scanner.AddPattern(WndProcPtrn, ptr =>
            {
                try
                {
                    PointerData.WndProc = ptr.AsIntPtr(); //PVOID
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with WndProc pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern IsSessionStartedPtrn = new("IsSessionStarted", "40 38 35 ? ? ? ? 74 4D");
            scanner.AddPattern(IsSessionStartedPtrn, ptr =>
            {
                try
                {
                    ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with IsSessionStarted pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern GetNativeHandlerPtrn = new Pattern("GetNativeHandler", "E8 ? ? ? ? 42 8B 9C FE");
            scanner.AddPattern(GetNativeHandlerPtrn, ptr =>
            {
                try
                {
                    PointerData.GetNativeHandler = Marshal.GetDelegateForFunctionPointer<Functions.GetNativeHandler>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetNativeHandler pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern FixVectorsPtrn = new Pattern("FixVectors", "8B 41 18 4C 8B C1 85");
            scanner.AddPattern(FixVectorsPtrn, ptr =>
            {
                try
                {
                    PointerData.FixVectors = Marshal.GetDelegateForFunctionPointer<Functions.FixVectors>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with FixVectors pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern ScriptThreadsPtrn = new Pattern("ScriptThreads&RunScriptThreads", "48 8D 0D ? ? ? ? E8 ? ? ? ? EB 0B 8B 0D");
            scanner.AddPattern(ScriptThreadsPtrn, ptr =>
            {
                try
                {
                    PointerData.ScriptThreads = ptr.Add(3).Rip().AsIntPtr();
                    PointerData.RunScriptThreads = ptr.Add(8).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ScriptThreads&RunScriptThreads pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern ScriptProgramsPtrn = new Pattern("ScriptPrograms", "C1 EF 0E 85 FF 74 21");
            scanner.AddPattern(ScriptProgramsPtrn, ptr =>
            {
                try
                {
                    PointerData.ScriptPrograms = ptr.Sub(0x16).Add(3).Rip().Add(0xC8).AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ScriptPrograms pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern CurrentScriptThreadPtrn = new Pattern("CurrentScriptThread&ScriptVM", "48 89 2D ? ? ? ? 48 89 2D ? ? ? ? 48 8B 04 F9");
            scanner.AddPattern(CurrentScriptThreadPtrn, ptr =>
            {
                try
                {
                    PointerData.CurrentScriptThread = ptr.Add(3).Rip().AsIntPtr();
                    PointerData.ScriptVM = Marshal.GetDelegateForFunctionPointer<Functions.ScriptVM>(ptr.Add(0x28).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with CurrentScriptThread&ScriptVM pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern SendMetricPtrn = new Pattern("SendMetric", "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8B F1 48 8B FA B1");
            scanner.AddPattern(SendMetricPtrn, ptr =>
            {
                try
                {
                    PointerData.SendMetric = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SendMetric pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern QueueDependencyPtrn = new Pattern("QueueDependency", "E8 ? ? ? ? EB 43 8A 43 54");
            scanner.AddPattern(QueueDependencyPtrn, ptr =>
            {
                try
                {
                    PointerData.QueueDependency = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with QueueDependency pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern UnkFunctionPtrn = new Pattern("UnkFunction", "40 53 48 83 EC 20 48 8B 59 20 48 8B 43 08 48 8B 4B");
            scanner.AddPattern(UnkFunctionPtrn, ptr =>
            {
                try
                {
                    PointerData.UnkFunction = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with UnkFunction pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern ScriptGlobalsPtrn = new Pattern("ScriptGlobals", "48 8D 15 ? ? ? ? 48 8B 1D ? ? ? ? 8B 3D");
            scanner.AddPattern(ScriptGlobalsPtrn, ptr =>
            {
                try
                {
                    PointerData.ScriptGlobals = ptr.Add(3).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ScriptGlobals pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern HandleNetGameEventPtrn = new Pattern("HandleNetGameEvent", "E8 ? ? ? ? F6 43 28 01 74 05 8B 7B 0C EB 03 8B 7B 14 48 8B CB E8 ? ? ? ? 2B F8 83 FF 28 0F 8D C9 FE FF FF");
            scanner.AddPattern(HandleNetGameEventPtrn, ptr =>
            {
                try
                {
                    PointerData.HandleNetGameEvent = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandleNetGameEvent pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern SendEventAckPtrn = new Pattern("SendEventAck", "E8 ? ? ? ? F6 43 32 01 74 4B");
            scanner.AddPattern(SendEventAckPtrn, ptr =>
            {
                try
                {
                    PointerData.SendEventAck = Marshal.GetDelegateForFunctionPointer<Functions.SendEventAck>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SendEventAck pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern EnumerateAudioDevicesPtrn = new Pattern("EnumerateAudioDevices", "48 89 5C 24 08 48 89 74 24 10 48 89 7C 24 18 55 48 8B EC 48 83 EC 60 33 C0 41");
            scanner.AddPattern(EnumerateAudioDevicesPtrn, ptr =>
            {
                try
                {
                    PointerData.EnumerateAudioDevices = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with EnumerateAudioDevices pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern DirectSoundCaptureCreatePtrn = new Pattern("DirectSoundCaptureCreate", "E8 ? ? ? ? 85 C0 79 08 48 83 23 00 32 C0 EB 7B");
            scanner.AddPattern(DirectSoundCaptureCreatePtrn, ptr =>
            {
                try
                {
                    PointerData.DirectSoundCaptureCreate = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with DirectSoundCaptureCreate pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern HwndPtrn = new Pattern("Hwnd", "4C 8B 05 ? ? ? ? 4C 8D 0D ? ? ? ? 48 89 54 24");
            scanner.AddPattern(HwndPtrn, ptr =>
            {
                try
                {
                    PointerData.Hwnd = Marshal.ReadIntPtr(ptr.Add(3).Rip().AsIntPtr());
                    LOG.INFO($"HWND: {PointerData.Hwnd}");
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with Hwnd pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern HandleToPtrPtrn = new Pattern("HandleToPtr", "E8 ? ? ? ? 45 8D 47 04");
            scanner.AddPattern(HandleToPtrPtrn, ptr =>
            {
                try
                {
                    PointerData.HandleToPtr = Marshal.GetDelegateForFunctionPointer<Functions.HandleToPtr>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandleToPtr pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern PtrToHandlePtrn = new Pattern("PtrToHandle", "E8 ? ? ? ? F3 0F 10 0D ? ? ? ? 48 8D 4D DF 8B 5B 40");
            scanner.AddPattern(PtrToHandlePtrn, ptr =>
            {
                try
                {
                    PointerData.PtrToHandle = Marshal.GetDelegateForFunctionPointer<Functions.PtrToHandle>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PtrToHandle pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern GetLocalPedPtrn = new Pattern("GetLocalPed", "8A 05 ? ? ? ? 33 D2 84 C0 74 39 48 8B 0D ? ? ? ? 4C 8B 05 ? ? ? ? 48 C1 C9 05 48 C1 C1 20 4C 33 C1 8B C1 83 E0 1F 49 C1 C0 20 FF C0 8A C8 8A 05 ? ? ? ? 49 D3 C0 84 C0 74 06 49 8B D0 48 F7 D2 48 8B 42");
            scanner.AddPattern(GetLocalPedPtrn, ptr =>
            {
                try
                {
                    PointerData.GetLocalPed = Marshal.GetDelegateForFunctionPointer<Functions.GetLocalPed>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetLocalPed pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern HandleCloneCreatePtrn = new Pattern("HandleCloneCreate", "48 8B C4 48 89 58 08 48 89 68 10 48 89 70 20 66 44 89 40 18 57 41 54 41 55 41 56 41 57 48 83");
            scanner.AddPattern(HandleCloneCreatePtrn, ptr =>
            {
                try
                {
                    PointerData.HandleCloneCreate = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandleCloneCreate pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern HandleCloneSyncPtrn = new Pattern("HandleCloneSync", "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 56 41 57 48 83 EC 40 4C 8B F2");
            scanner.AddPattern(HandleCloneSyncPtrn, ptr =>
            {
                try
                {
                    PointerData.HandleCloneSync = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandleCloneSync pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern CanApplyDataPtrn = new Pattern("CanApplyData", "48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 4C 89 70 20 41 57 48 83 EC 30 4C 8B FA");
            scanner.AddPattern(CanApplyDataPtrn, ptr =>
            {
                try
                {
                    PointerData.CanApplyData = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with CanApplyData pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern GetSyncTreeForTypePtrn = new Pattern("GetSyncTreeForType", "0F B7 CA 83 F9");
            scanner.AddPattern(GetSyncTreeForTypePtrn, ptr =>
            {
                try
                {
                    PointerData.GetSyncTreeForType = Marshal.GetDelegateForFunctionPointer<Functions.GetSyncTreeForType>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetSyncTreeForType pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern resetSyncNodesPtrn = new Pattern("ResetSyncNodes", "E8 ? ? ? ? E8 ? ? ? ? B9 0E 00 00 00 E8 ? ? ? ? 48 8B CB E8 ? ? ? ? E8 ? ? ? ? B9 0F 00 00 00 E8 ? ? ? ? E8");
            scanner.AddPattern(resetSyncNodesPtrn, ptr =>
            {
                try
                {
                    PointerData.ResetSyncNodes = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ResetSyncNodes pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern throwFatalErrorPtrn = new Pattern("ThrowFatalError", "48 83 EC 28 45 33 C9 E8 ? ? ? ? CC");
            scanner.AddPattern(throwFatalErrorPtrn, ptr =>
            {
                try
                {
                    PointerData.ThrowFatalError = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ThrowFatalError pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern isAnimSceneInScopePtrn = new Pattern("IsAnimSceneInScope", "74 78 4C 8B 03 48 8B CB");
            scanner.AddPattern(isAnimSceneInScopePtrn, ptr =>
            {
                try
                {
                    PointerData.IsAnimSceneInScope = ptr.Sub(0x37).AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with IsAnimSceneInScope pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern broadcastNetArrayPtrn = new Pattern("BroadcastNetArray", "48 89 5C 24 ? 48 89 54 24 ? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ? 48 8B 81 ? ? ? ? 4C 8B F1");
            scanner.AddPattern(broadcastNetArrayPtrn, ptr =>
            {
                try
                {
                    PointerData.BroadcastNetArray = ptr.AsIntPtr();
                    PointerData.NetArrayPatch = ptr.Add(0x23B).AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with BroadcastNetArray pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern inventoryEventCtorPtrn = new Pattern("InventoryEventConstructor", "C7 41 10 55 2B 70 40");
            scanner.AddPattern(inventoryEventCtorPtrn, ptr =>
            {
                try
                {
                    PointerData.InventoryEventConstructor = Marshal.GetDelegateForFunctionPointer<Functions.InventoryEventConstructor>(ptr.Sub(0x81).AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with InventoryEventConstructor pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern eventGroupNetworkPtrn = new Pattern("EventGroupNetwork", "80 78 47 00 75 52 48 8B 35");
            scanner.AddPattern(eventGroupNetworkPtrn, ptr =>
            {
                try
                {
                    PointerData.EventGroupNetwork = ptr.Add(0x9).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with EventGroupNetwork pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern networkRequestPtrn = new Pattern("NetworkRequest", "4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 73 18 57 48 81 EC ? ? ? ? 48 8B 01");
            scanner.AddPattern(networkRequestPtrn, ptr =>
            {
                try
                {
                    PointerData.NetworkRequest = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with NetworkRequest pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern handleScriptedGameEventPtrn = new Pattern("HandleScriptedGameEvent", "40 53 48 81 EC 10 02 00 00 48 8B D9 48 8B");
            scanner.AddPattern(handleScriptedGameEventPtrn, ptr =>
            {
                try
                {
                    PointerData.HandleScriptedGameEvent = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandleScriptedGameEvent pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern addObjectToCreationQueuePtrn = new Pattern("AddObjectToCreationQueue", "48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 55 41 54 41 55 41 56 41 57 48 8D 68 98 48 81 EC 50");
            scanner.AddPattern(addObjectToCreationQueuePtrn, ptr =>
            {
                try
                {
                    PointerData.AddObjectToCreationQueue = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with AddObjectToCreationQueue pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern playerHasJoinedPtrn = new Pattern("PlayerHasJoined", "E8 ? ? ? ? 8A 4B 19 48 8B 45 38");
            scanner.AddPattern(playerHasJoinedPtrn, ptr =>
            {
                try
                {
                    PointerData.PlayerHasJoined = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PlayerHasJoined pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern playerHasLeftPtrn = new Pattern("PlayerHasLeft", "E8 ? ? ? ? 48 8B 0D ? ? ? ? 48 8B 57 08");
            scanner.AddPattern(playerHasLeftPtrn, ptr =>
            {
                try
                {
                    PointerData.PlayerHasLeft = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PlayerHasLeft pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern networkPlayerMgrPtrn = new Pattern("NetworkPlayerMgr", "48 89 5C 24 08 57 48 83 EC 30 48 8B ? ? ? ? 01 8A D9 80 F9 20");
            scanner.AddPattern(networkPlayerMgrPtrn, ptr =>
            {
                try
                {
                    PointerData.NetworkPlayerMgr = Marshal.ReadIntPtr(ptr.Add(0xD).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with NetworkPlayerMgr pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern getNetworkPlayerFromPidPtrn = new Pattern("GetNetworkPlayerFromPid", "E8 ? ? ? ? B2 01 8B CB 48 8B F8");
            scanner.AddPattern(getNetworkPlayerFromPidPtrn, ptr =>
            {
                try
                {
                    PointerData.GetNetPlayerFromPid = Marshal.GetDelegateForFunctionPointer<Functions.GetNetworkPlayerFromPid>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetNetworkPlayerFromPid pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern getNetObjectByIdPtrn = new Pattern("GetNetObjectById", "E8 ? ? ? ? 48 85 C0 74 20 80 78 47 00");
            scanner.AddPattern(getNetObjectByIdPtrn, ptr =>
            {
                try
                {
                    PointerData.GetNetObjectById = Marshal.GetDelegateForFunctionPointer<Functions.GetNetObjectById>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetNetObjectById pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern addExplosionBypass = new Pattern("ExplosionBypass", "0F 84 ? ? ? ? 44 38 3D ? ? ? ? 75 14");
            scanner.AddPattern(addExplosionBypass, ptr =>
            {
                try
                {
                    PointerData.ExplosionBypass = ptr.Add(9).Rip().AsIntPtr(); //bool*
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ExplosionBypass pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern worldToScreenPtrn = new Pattern("WorldToScreen", "E8 ? ? ? ? 84 C0 74 19 F3 0F 10 44 24");
            scanner.AddPattern(worldToScreenPtrn, ptr =>
            {
                try
                {
                    PointerData.WorldToScreen = Marshal.GetDelegateForFunctionPointer<Functions.WorldToScreen>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with WorldToScreen pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern writePlayerHealthDataPtrn = new Pattern("WritePlayerHealthData", "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 48 8B B1 A8");
            scanner.AddPattern(writePlayerHealthDataPtrn, ptr =>
            {
                try
                {
                    PointerData.WritePlayerHealthData = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with WritePlayerHealthData pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern requestControlPtrn = new Pattern("RequestControl", "E8 ? ? ? ? 32 C0 48 83 C4 ? 5B C3 B0 ? EB ? 48 8D 0D");
            scanner.AddPattern(requestControlPtrn, ptr =>
            {
                try
                {
                    PointerData.RequestControlOfNetObject = Marshal.GetDelegateForFunctionPointer<Functions.RequestControlOfNetObject>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with RequestControl pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern getAnimSceneFromHandlePtrn = new Pattern("GetAnimSceneFromHandle", "00 83 F9 04 7C 0F");
            scanner.AddPattern(getAnimSceneFromHandlePtrn, ptr =>
            {
                try
                {
                    PointerData.GetAnimSceneFromHandle = Marshal.GetDelegateForFunctionPointer<Functions.GetAnimSceneFromHandle>(ptr.Sub(0x13).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with GetAnimSceneFromHandle pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern networkObjectMgrPtrn = new Pattern("NetworkObjectMgr", "74 44 0F B7 56 40");
            scanner.AddPattern(networkObjectMgrPtrn, ptr =>
            {
                try
                {
                    PointerData.NetworkObjectMgr = ptr.Add(0xC).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with NetworkObjectMgr pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern sendPacketPtrn = new Pattern("SendPacket", "8B 44 24 60 48 8B D6 48 8B CD");
            scanner.AddPattern(sendPacketPtrn, ptr =>
            {
                try
                {
                    PointerData.SendPacket = Marshal.GetDelegateForFunctionPointer<Functions.SendPacket>(ptr.Add(0xE).Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SendPacket pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern queuePacketPtrn = new Pattern("QueuePacket", "E8 ?? ?? ?? ?? FF C6 49 83 C6 08 3B B7 88 40 00 00");
            scanner.AddPattern(queuePacketPtrn, ptr =>
            {
                try
                {
                    PointerData.QueuePacket = Marshal.GetDelegateForFunctionPointer<Functions.QueuePacket>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with QueuePacket pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern receiveNetMessagePtrn = new Pattern("ReceiveNetMessage", "E8 ?? ?? ?? ?? EB 24 48 8D B7 90 02 00 00");
            scanner.AddPattern(receiveNetMessagePtrn, ptr =>
            {
                try
                {
                    PointerData.ReceiveNetMessage = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ReceiveNetMessage pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern handlePresenceEventPtrn = new Pattern("HandlePresenceEvent", "E8 ?? ?? ?? ?? 4C 8D 9C 24 B0 10 00 00 49 8B 5B 10");
            scanner.AddPattern(handlePresenceEventPtrn, ptr =>
            {
                try
                {
                    PointerData.HandlePresenceEvent = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with HandlePresenceEvent pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern postMessagePtrn = new Pattern("PostPresenceMessage", "E8 ?? ?? ?? ?? EB 35 C7 44 24 20 D9 7A 70 E1");
            scanner.AddPattern(postMessagePtrn, ptr =>
            {
                try
                {
                    PointerData.PostPresenceMessage = Marshal.GetDelegateForFunctionPointer<Functions.PostPresenceMessage>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PostPresenceMessage pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern sendNetInfoToLobbyPtrn = new Pattern("SendNetInfoToLobby", "E8 ?? ?? ?? ?? 32 DB 84 C0 74 1B 44 8B 84 24 40 01 00 00");
            scanner.AddPattern(sendNetInfoToLobbyPtrn, ptr =>
            {
                try
                {
                    PointerData.SendNetInfoToLobby = Marshal.GetDelegateForFunctionPointer<Functions.SendNetInfoToLobby>(ptr.Add(1).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SendNetInfoToLobby pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern pedPoolPtrn = new Pattern("PedPool", "0F 28 F0 48 85 DB 74 56 8A 05 ? ? ? ? 84 C0 75 05");
            scanner.AddPattern(pedPoolPtrn, ptr =>
            {
                try
                {
                    PointerData.PedPool = ptr.Add(10).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PedPool pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern objectPoolPtrn = new Pattern("ObjectPool", "3C 05 75 67");
            scanner.AddPattern(objectPoolPtrn, ptr =>
            {
                try
                {
                    PointerData.ObjectPool = ptr.Add(20).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ObjectPool pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern vehiclePoolPtrn = new Pattern("VehiclePool", "48 83 EC 20 8A 05 ? ? ? ? 45 33 E4");
            scanner.AddPattern(vehiclePoolPtrn, ptr =>
            {
                try
                {
                    PointerData.VehiclePool = ptr.Add(6).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with VehiclePool pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern pickupPoolPtrn = new Pattern("PickupPool", "0F 84 ? ? ? ? 8A 05 ? ? ? ? 48 85");
            scanner.AddPattern(pickupPoolPtrn, ptr =>
            {
                try
                {
                    PointerData.PickupPool = ptr.Add(8).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with PickupPool pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern fwScriptGuidCreateGuidPtrn = new Pattern("FwScriptGuidCreateGuid", "E8 ? ? ? ? B3 01 8B 15");
            scanner.AddPattern(fwScriptGuidCreateGuidPtrn, ptr =>
            {
                try
                {
                    PointerData.FwScriptGuidCreateGuid = Marshal.GetDelegateForFunctionPointer<FwScriptGuidCreateGuid>(ptr.Sub(141).AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with FwScriptGuidCreateGuid pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern receiveServerMessagePtrn = new Pattern("ReceiveServerMessage", "48 89 5C 24 08 57 48 83 EC 20 48 8B 02 48 8B F9 48 8B CA 48 8B DA FF 50 ?? 48 8B C8");
            scanner.AddPattern(receiveServerMessagePtrn, ptr =>
            {
                try
                {
                    PointerData.ReceiveServerMessage = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ReceiveServerMessage pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern serializeServerRPCPtrn = new Pattern("SerializeServerRPC", "48 89 5C 24 08 57 48 83 EC 30 48 8B 44 24 70");
            scanner.AddPattern(serializeServerRPCPtrn, ptr =>
            {
                try
                {
                    PointerData.SerializeServerRPC = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with SerializeServerRPC pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern readBBArrayPtrn = new Pattern("ReadBitBufferArray", "48 89 5C 24 08 57 48 83 EC 30 41 8B F8 4C");
            scanner.AddPattern(readBBArrayPtrn, ptr =>
            {
                try
                {
                    PointerData.ReadBitBufferArray = Marshal.GetDelegateForFunctionPointer<Functions.ReadBitBufferArray>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ReadBitBufferArray pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern writeBBArrayPtrn = new Pattern("WriteBitBufferArray", "48 89 5C 24 08 57 48 83 EC 30 F6 41 28");
            scanner.AddPattern(writeBBArrayPtrn, ptr =>
            {
                try
                {
                    PointerData.WriteBitBufferArray = Marshal.GetDelegateForFunctionPointer<Functions.WriteBitBufferArray>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with WriteBitBufferArray pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern readBBStringPtrn = new Pattern("ReadBitBufferString", "48 89 5C 24 08 48 89 6C 24 18 56 57 41 56 48 83 EC 20 45 8B");
            scanner.AddPattern(readBBStringPtrn, ptr =>
            {
                try
                {
                    PointerData.ReadBitBufferString = Marshal.GetDelegateForFunctionPointer<Functions.ReadBitBufferString>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ReadBitBufferString pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern initNativeTablesPtrn = new Pattern("InitNativeTables", "41 B0 01 44 39 51 2C 0F");
            scanner.AddPattern(initNativeTablesPtrn, ptr =>
            {
                try
                {
                    PointerData.InitNativeTables = ptr.Sub(0x10).AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with InitNativeTables pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern triggerWeaponDamageEventPtrn = new Pattern("TriggerWeaponDamageEvent", "89 44 24 58 8B 47 F8 89");
            scanner.AddPattern(triggerWeaponDamageEventPtrn, ptr =>
            {
                try
                {
                    PointerData.TriggerWeaponDamageEvent = Marshal.GetDelegateForFunctionPointer<Functions.TriggerWeaponDamageEvent>(ptr.Add(0x39).Rip().AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with TriggerWeaponDamageEvent pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern scSessionPtrn = new Pattern("ScSession", "3B 1D ? ? ? ? 76 60");
            scanner.AddPattern(scSessionPtrn, ptr =>
            {
                try
                {
                    PointerData.ScSession = ptr.Add(0xB).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ScSession pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern receiveArrayUpdatePtrn = new Pattern("ReceiveArrayUpdate", "48 89 5C 24 10 55 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 50 48 8B D9 45");
            scanner.AddPattern(receiveArrayUpdatePtrn, ptr =>
            {
                try
                {
                    PointerData.ReceiveArrayUpdate = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with ReceiveArrayUpdate pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern writeVPMDataPtrn = new Pattern("WriteVehicleProximityMigrationData", "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 48 89 48 08 57 41 54 41 55 41 56 41 57 48 83 EC 30 4C 8B A9");
            scanner.AddPattern(writeVPMDataPtrn, ptr =>
            {
                try
                {
                    PointerData.WriteVPMData = ptr.AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with WriteVehicleProximityMigrationData pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern triggerGiveControlEventPtrn = new Pattern("TriggerGiveControlEvent", "48 8B C4 48 89 58 ? 48 89 68 ? 48 89 70 ? 48 89 78 ? 41 54 41 56 41 57 48 83 EC ? 65 4C 8B 0C 25");
            scanner.AddPattern(triggerGiveControlEventPtrn, ptr =>
            {
                try
                {
                    PointerData.TriggerGiveControlEvent = Marshal.GetDelegateForFunctionPointer<Functions.TriggerGiveControlEvent>(ptr.AsIntPtr());
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with TriggerGiveControlEvent pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern createPoolItemPtrn = new Pattern("CreatePoolItem", "E8 ? ? ? ? 48 85 C0 74 ? 44 8A 4C 24 ? 48 8B C8 44 0F B7 44 24 ? 0F B7 54 24");
            scanner.AddPattern(createPoolItemPtrn, ptr =>
            {
                try
                {
                    PointerData.CreatePoolItem = ptr.Add(1).Rip().AsIntPtr();
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error with CreatePoolItem pointer: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            if (scanner.ScanAsync())
            {
                LOG.INFO("Patterns scanned successfully.");
                
            }
            else
            {
                LOG.ERROR("Some patterns could not be found. Stopping here.");
                return false;
            }

            if (PointerData.GetRendererInfo != null)
            {
                try
                {
                    IntPtr rendererInfoPtr = PointerData.GetRendererInfo();
                    if (rendererInfoPtr != IntPtr.Zero)
                    {
                        RenderingInfo rendererInfo = RenderingInfo.FromIntPtr(rendererInfoPtr);

                        if (rendererInfo.IsRenderingType(ERenderingType.DX12))
                        {
                            PointerData.IsVulkan = false;
                            LOG.INFO("Renderer type: DirectX 12");
                        }
                        else if (rendererInfo.IsRenderingType(ERenderingType.Vulkan))
                        {
                            PointerData.IsVulkan = true;
                            LOG.INFO("Renderer type: Vulkan");
                        }
                        else
                        {
                            LOG.ERROR("Unknown renderer type!");
                            return false;
                        }
                    }
                    else
                    {
                        LOG.ERROR("GetRendererInfo returned null pointer");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error retrieving or processing RenderingInfo: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            else
            {
                LOG.ERROR("GetRendererInfo delegate is not initialized!");
                return false;
            }

            return true;
        }
    }
}