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
    // Equivalent to: using GetNativeHandler = rage::scrNativeHandler (*)(rage::scrNativeHash hash);
    public delegate IntPtr GetNativeHandler(uint hash);
    public delegate Utils.RenderingInfo GetRendererInfo(); // returns RenderingInfo instance
    public delegate void FixVectors(IntPtr call_ctx); // rage::scrNativeCallContext* call_Ctx
    public delegate IntPtr HandleToPtr(int handle); //returns void*
    public delegate void SendEventAck(IntPtr eventMgr, IntPtr evnt, IntPtr sourcePlayer, IntPtr targetPlayer, int eventIndex, int handledBitset); // rage::netEventMgr* eventMgr, void* event, CNetGamePlayer* sourcePlayer, CNetGamePlayer* targetPlayer, int eventIndex, int handledBitset
    public delegate int PtrToHandle(IntPtr pointer); // void*
    public delegate IntPtr GetLocalPed(); // returns CPed*
    public delegate IntPtr GetSyncTreeForType(IntPtr netObjectMgr, UInt16 type); // returns rage::netSyncTree*, param void* netObjectMgr
    public delegate IntPtr GetNetworkPlayerFromPid(byte player); // returns CNetGamePlayer*, param uint8_t player
    public delegate bool WorldToScreen(IntPtr world_coords, out float out_x, out float out_y); // returns bool, params float* world_coords, float* out_x, float* out_y
    public delegate IntPtr GetNetObjectById(ushort id); // returns rage::netObject*, param uint16_t id
    public delegate bool RequestControlOfNetObject(ref IntPtr netId, bool unk); // returns bool, params rage::netObject** netId, bool unk
    public delegate bool SendPacket(IntPtr mgr, IntPtr adde, int connection_id, IntPtr data, int size, int flags); // returns bool, params rage::netConnectionManager* mgr, rage::netPeerAddress* adde, void* data
    public delegate bool QueuePacket(IntPtr mgr, int msg_id, int connection_id, IntPtr data, int size, int flags, IntPtr unk); // returns bool, params rage::netConnectionManager* mgr, void* data, void* unk
    public delegate bool PostPresenceMessage(int localGamerIndex, IntPtr recipients, int numRecipients, string msg, uint ttlSeconds); // returns bool, params rage::rlGamerInfo* recipients, const char* msg
    public delegate bool SendNetInfoToLobby(IntPtr player, long a2, long a3, IntPtr a4); // returns bool, params rage::rlGamerInfo* player, DWORD* a4
    public delegate bool ReadBitBufferArray(IntPtr buffer, IntPtr read, int bits, int unk); // returns bool, params rage::datBitBuffer* buffer, PVOID read
    public delegate bool WriteBitBufferArray(IntPtr buffer, IntPtr val, int bits, int unk); // returns bool, params rage::datBitBuffer* buffer, void* val
    public delegate bool ReadBitBufferString(IntPtr buffer, StringBuilder read, int bits); // returns bool, params rage::datBitBuffer* buffer, char* read
    public delegate IntPtr GetAnimSceneFromHandle(IntPtr scene, int handle); // returns CAnimScene**, params CAnimScene** scene
    public delegate IntPtr InventoryEventConstructor(IntPtr existingItem, uint reward_hash, uint model_hash, bool a4, bool a5, IntPtr a6); // returns CEventInventoryItemPickedUp*, params CEventInventoryItemPickedUp*
    public delegate void TriggerWeaponDamageEvent(IntPtr source, IntPtr target, IntPtr unk, IntPtr position, IntPtr a5, IntPtr a6, bool override_dmg, IntPtr weapon_hash, float damage, float f10, int tire_index, int suspension_index, ulong flags, IntPtr action_result, bool hit_entity_weapon, bool hit_ammo_attachment, bool silenced, bool a18, bool a19, int a20, int a21, int a22, int a23, int a24, int a25); // returns void, params rage::netObject* source, rage::netObject* target, rage::netObject* unk, rage::fvector3* position, std::uint32_t* weapon_hash, void* action_result
    public delegate void TriggerGiveControlEvent(IntPtr player, IntPtr obj, int type); // returns void, params CNetGamePlayer* player, rage::netObject* object
    public delegate eThreadState ScriptVM(IntPtr stack, IntPtr globals, [MarshalAs(UnmanagedType.Bool)] bool globals_enabled, IntPtr program, IntPtr ctx);
    public delegate uint FwScriptGuidCreateGuid(IntPtr arg); // uint32_t (*)(void*)
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

    // Vulkan
    public static IntPtr QueuePresentKHR { get; set; } // PVOID - Init in Renderer
    public static IntPtr CreateSwapchainKHR { get; set; } // PVOID - Init in Renderer
    public static IntPtr AcquireNextImageKHR { get; set; } // PVOID - Init in Renderer
    public static IntPtr AcquireNextImage2KHR { get; set; } // PVOID - Init in Renderer

    public static IntPtr VkDevicePtr { get; set; } // VkDevice*

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
                    //LOG.INFO($"Swapchain pattern found, initial pointer: {ptr.AsIntPtr():X}");

                    // This is equivalent to IDXGISwapChain1**
                    IntPtr SwapChainPtr = ptr.Add(4).Add(3).Rip().AsIntPtr();
                    PointerData.SwapChain = SwapChainPtr;
                    //LOG.INFO($"SwapChain pointer to pointer (IDXGISwapChain1**): {PointerData.SwapChain:X}");
                }
                catch (Exception ex)
                {
                    LOG.ERROR($"Error in swapchain delegate: {ex.Message}");
                    LOG.ERROR($"Stack trace: {ex.StackTrace}");
                }
            });

            Pattern CommandQueuePtrn = new("ID3D12CommandQueue", "FF 50 10 48 8B 0D ? ? ? ? 48 8B 01");
            scanner.AddPattern(CommandQueuePtrn, ptr => { PointerData.CommandQueue = ptr.Add(3).Add(3).Rip().AsIntPtr(); });

            Pattern GetRendererInfoPtrn = new("GetRendererInfo", "E8 ? ? ? ? 38 58 09");
            scanner.AddPattern(GetRendererInfoPtrn, ptr => { PointerData.GetRendererInfo = Marshal.GetDelegateForFunctionPointer<GetRendererInfo>(ptr.Add(1).Rip().AsIntPtr()); });

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
                UnhookWindowsHookEx(Marshal.ReadIntPtr(ptr.Add(0xC).Rip().AsIntPtr()));
            });

            Pattern WndProcPtrn = new("WndProc", "48 89 5C 24 ? 4C 89 4C 24 ? 48 89 4C 24 ? 55 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 60");
            scanner.AddPattern(WndProcPtrn, ptr =>
            {
                PointerData.WndProc = ptr.AsIntPtr(); //PVOID
            });

            Pattern IsSessionStartedPtrn = new("IsSessionStarted", "40 38 35 ? ? ? ? 74 4D");
            scanner.AddPattern(IsSessionStartedPtrn, ptr => { ptr.Add(1).Rip().AsIntPtr(); });

            Pattern GetNativeHandlerPtrn = new Pattern("GetNativeHandler", "E8 ? ? ? ? 42 8B 9C FE");
            scanner.AddPattern(GetNativeHandlerPtrn, ptr =>
            {
                PointerData.GetNativeHandler = Marshal.GetDelegateForFunctionPointer<Functions.GetNativeHandler>(ptr.Add(1).Rip().AsIntPtr());
            });


            Pattern FixVectorsPtrn = new Pattern("FixVectors", "8B 41 18 4C 8B C1 85");
            scanner.AddPattern(FixVectorsPtrn, ptr =>
            {
                PointerData.FixVectors = Marshal.GetDelegateForFunctionPointer<Functions.FixVectors>(ptr.AsIntPtr());
            });

            Pattern ScriptThreadsPtrn = new Pattern("ScriptThreads&RunScriptThreads", "48 8D 0D ? ? ? ? E8 ? ? ? ? EB 0B 8B 0D");
            scanner.AddPattern(ScriptThreadsPtrn, ptr =>
            {
                PointerData.ScriptThreads = ptr.Add(3).Rip().AsIntPtr();
                PointerData.RunScriptThreads = ptr.Add(8).Rip().AsIntPtr();
            });

            Pattern ScriptProgramsPtrn = new Pattern("ScriptPrograms", "C1 EF 0E 85 FF 74 21");
            scanner.AddPattern(ScriptProgramsPtrn, ptr =>
            {
                PointerData.ScriptPrograms = ptr.Sub(0x16).Add(3).Rip().Add(0xC8).AsIntPtr();
            });

            Pattern CurrentScriptThreadPtrn = new Pattern("CurrentScriptThread&ScriptVM", "48 89 2D ? ? ? ? 48 89 2D ? ? ? ? 48 8B 04 F9");
            scanner.AddPattern(CurrentScriptThreadPtrn, ptr =>
            {
                PointerData.CurrentScriptThread = ptr.Add(3).Rip().AsIntPtr();
                PointerData.ScriptVM = Marshal.GetDelegateForFunctionPointer<Functions.ScriptVM>(ptr.Add(0x28).Rip().AsIntPtr());
            });

            Pattern SendMetricPtrn = new Pattern("SendMetric", "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8B F1 48 8B FA B1");
            scanner.AddPattern(SendMetricPtrn, ptr =>
            {
                PointerData.SendMetric = ptr.AsIntPtr();
            });

            //Pattern VmDetectionCallbackPtrn = new Pattern("VMDetectionCallback", "48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B 0D ? ? ? ? 8D 7E");
            //scanner.AddPattern(VmDetectionCallbackPtrn, ptr =>
            //{
            //    var loc = ptr.Add(3).Rip().AsIntPtr();
            //    PointerData.VmDetectionCallback = loc;
            //    PointerData.RageSecurityInitialized = (IntPtr)(loc - 6); // bool*
            //});

            Pattern QueueDependencyPtrn = new Pattern("QueueDependency", "E8 ? ? ? ? EB 43 8A 43 54");
            scanner.AddPattern(QueueDependencyPtrn, ptr =>
            {
                PointerData.QueueDependency = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern UnkFunctionPtrn = new Pattern("UnkFunction", "40 53 48 83 EC 20 48 8B 59 20 48 8B 43 08 48 8B 4B");
            scanner.AddPattern(UnkFunctionPtrn, ptr =>
            {
                PointerData.UnkFunction = ptr.AsIntPtr();
            });

            Pattern ScriptGlobalsPtrn = new Pattern("ScriptGlobals", "48 8D 15 ? ? ? ? 48 8B 1D ? ? ? ? 8B 3D");
            scanner.AddPattern(ScriptGlobalsPtrn, ptr =>
            {
                PointerData.ScriptGlobals = ptr.Add(3).Rip().AsIntPtr();
            });

            Pattern HandleNetGameEventPtrn = new Pattern("HandleNetGameEvent", "E8 ? ? ? ? F6 43 28 01 74 05 8B 7B 0C EB 03 8B 7B 14 48 8B CB E8 ? ? ? ? 2B F8 83 FF 28 0F 8D C9 FE FF FF");
            scanner.AddPattern(HandleNetGameEventPtrn, ptr =>
            {
                PointerData.HandleNetGameEvent = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern SendEventAckPtrn = new Pattern("SendEventAck", "E8 ? ? ? ? F6 43 32 01 74 4B");
            scanner.AddPattern(SendEventAckPtrn, ptr =>
            {
                PointerData.SendEventAck = Marshal.GetDelegateForFunctionPointer<Functions.SendEventAck>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern EnumerateAudioDevicesPtrn = new Pattern("EnumerateAudioDevices", "48 89 5C 24 08 48 89 74 24 10 48 89 7C 24 18 55 48 8B EC 48 83 EC 60 33 C0 41");
            scanner.AddPattern(EnumerateAudioDevicesPtrn, ptr =>
            {
                PointerData.EnumerateAudioDevices = ptr.AsIntPtr();
            });

            Pattern DirectSoundCaptureCreatePtrn = new Pattern("DirectSoundCaptureCreate", "E8 ? ? ? ? 85 C0 79 08 48 83 23 00 32 C0 EB 7B");
            scanner.AddPattern(DirectSoundCaptureCreatePtrn, ptr =>
            {
                PointerData.DirectSoundCaptureCreate = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern HwndPtrn = new Pattern("Hwnd", "4C 8B 05 ? ? ? ? 4C 8D 0D ? ? ? ? 48 89 54 24");
            scanner.AddPattern(HwndPtrn, ptr =>
            {
                PointerData.Hwnd = Marshal.ReadIntPtr(ptr.Add(3).Rip().AsIntPtr());
                LOG.INFO($"HWND: {PointerData.Hwnd}");
            });

            Pattern HandleToPtrPtrn = new Pattern("HandleToPtr", "E8 ? ? ? ? 45 8D 47 04");
            scanner.AddPattern(HandleToPtrPtrn, ptr =>
            {
                PointerData.HandleToPtr = Marshal.GetDelegateForFunctionPointer<Functions.HandleToPtr>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern PtrToHandlePtrn = new Pattern("PtrToHandle", "E8 ? ? ? ? F3 0F 10 0D ? ? ? ? 48 8D 4D DF 8B 5B 40");
            scanner.AddPattern(PtrToHandlePtrn, ptr =>
            {
                PointerData.PtrToHandle = Marshal.GetDelegateForFunctionPointer<Functions.PtrToHandle>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern GetLocalPedPtrn = new Pattern("GetLocalPed", "8A 05 ? ? ? ? 33 D2 84 C0 74 39 48 8B 0D ? ? ? ? 4C 8B 05 ? ? ? ? 48 C1 C9 05 48 C1 C1 20 4C 33 C1 8B C1 83 E0 1F 49 C1 C0 20 FF C0 8A C8 8A 05 ? ? ? ? 49 D3 C0 84 C0 74 06 49 8B D0 48 F7 D2 48 8B 42");
            scanner.AddPattern(GetLocalPedPtrn, ptr =>
            {
                PointerData.GetLocalPed = Marshal.GetDelegateForFunctionPointer<Functions.GetLocalPed>(ptr.AsIntPtr());
            });

            Pattern HandleCloneCreatePtrn = new Pattern("HandleCloneCreate", "48 8B C4 48 89 58 08 48 89 68 10 48 89 70 20 66 44 89 40 18 57 41 54 41 55 41 56 41 57 48 83");
            scanner.AddPattern(HandleCloneCreatePtrn, ptr =>
            {
                PointerData.HandleCloneCreate = ptr.AsIntPtr();
            });

            Pattern HandleCloneSyncPtrn = new Pattern("HandleCloneSync", "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 56 41 57 48 83 EC 40 4C 8B F2");
            scanner.AddPattern(HandleCloneSyncPtrn, ptr =>
            {
                PointerData.HandleCloneSync = ptr.AsIntPtr();
            });

            Pattern CanApplyDataPtrn = new Pattern("CanApplyData", "48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 4C 89 70 20 41 57 48 83 EC 30 4C 8B FA");
            scanner.AddPattern(CanApplyDataPtrn, ptr =>
            {
                PointerData.CanApplyData = ptr.AsIntPtr();
            });

            Pattern GetSyncTreeForTypePtrn = new Pattern("GetSyncTreeForType", "0F B7 CA 83 F9");
            scanner.AddPattern(GetSyncTreeForTypePtrn, ptr =>
            {
                PointerData.GetSyncTreeForType = Marshal.GetDelegateForFunctionPointer<Functions.GetSyncTreeForType>(ptr.AsIntPtr());
            });

            // Define patterns and add to scanner
            Pattern resetSyncNodesPtrn = new Pattern("ResetSyncNodes", "E8 ? ? ? ? E8 ? ? ? ? B9 0E 00 00 00 E8 ? ? ? ? 48 8B CB E8 ? ? ? ? E8 ? ? ? ? B9 0F 00 00 00 E8 ? ? ? ? E8");
            scanner.AddPattern(resetSyncNodesPtrn, ptr =>
            {
                PointerData.ResetSyncNodes = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern throwFatalErrorPtrn = new Pattern("ThrowFatalError", "48 83 EC 28 45 33 C9 E8 ? ? ? ? CC");
            scanner.AddPattern(throwFatalErrorPtrn, ptr =>
            {
                PointerData.ThrowFatalError = ptr.AsIntPtr();
            });

            Pattern isAnimSceneInScopePtrn = new Pattern("IsAnimSceneInScope", "74 78 4C 8B 03 48 8B CB");
            scanner.AddPattern(isAnimSceneInScopePtrn, ptr =>
            {
                PointerData.IsAnimSceneInScope = ptr.Sub(0x37).AsIntPtr();
            });

            Pattern broadcastNetArrayPtrn = new Pattern("BroadcastNetArray", "48 89 5C 24 ? 48 89 54 24 ? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC ? 48 8B 81 ? ? ? ? 4C 8B F1");
            scanner.AddPattern(broadcastNetArrayPtrn, ptr =>
            {
                PointerData.BroadcastNetArray = ptr.AsIntPtr();
                PointerData.NetArrayPatch = ptr.Add(0x23B).AsIntPtr();
            });

            Pattern inventoryEventCtorPtrn = new Pattern("InventoryEventConstructor", "C7 41 10 55 2B 70 40");
            scanner.AddPattern(inventoryEventCtorPtrn, ptr =>
            {
                PointerData.InventoryEventConstructor = Marshal.GetDelegateForFunctionPointer<Functions.InventoryEventConstructor>(ptr.Sub(0x81).AsIntPtr());
            });

            Pattern eventGroupNetworkPtrn = new Pattern("EventGroupNetwork", "80 78 47 00 75 52 48 8B 35");
            scanner.AddPattern(eventGroupNetworkPtrn, ptr =>
            {
                PointerData.EventGroupNetwork = ptr.Add(0x9).Rip().AsIntPtr();
            });

            Pattern networkRequestPtrn = new Pattern("NetworkRequest", "4C 8B DC 49 89 5B 08 49 89 6B 10 49 89 73 18 57 48 81 EC ? ? ? ? 48 8B 01");
            scanner.AddPattern(networkRequestPtrn, ptr =>
            {
                PointerData.NetworkRequest = ptr.AsIntPtr();
            });

            Pattern handleScriptedGameEventPtrn = new Pattern("HandleScriptedGameEvent", "40 53 48 81 EC 10 02 00 00 48 8B D9 48 8B");
            scanner.AddPattern(handleScriptedGameEventPtrn, ptr =>
            {
                PointerData.HandleScriptedGameEvent = ptr.AsIntPtr();
            });

            Pattern addObjectToCreationQueuePtrn = new Pattern("AddObjectToCreationQueue", "48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 55 41 54 41 55 41 56 41 57 48 8D 68 98 48 81 EC 50");
            scanner.AddPattern(addObjectToCreationQueuePtrn, ptr =>
            {
                PointerData.AddObjectToCreationQueue = ptr.AsIntPtr();
            });

            Pattern playerHasJoinedPtrn = new Pattern("PlayerHasJoined", "E8 ? ? ? ? 8A 4B 19 48 8B 45 38");
            scanner.AddPattern(playerHasJoinedPtrn, ptr =>
            {
                PointerData.PlayerHasJoined = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern playerHasLeftPtrn = new Pattern("PlayerHasLeft", "E8 ? ? ? ? 48 8B 0D ? ? ? ? 48 8B 57 08");
            scanner.AddPattern(playerHasLeftPtrn, ptr =>
            {
                PointerData.PlayerHasLeft = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern networkPlayerMgrPtrn = new Pattern("NetworkPlayerMgr", "48 89 5C 24 08 57 48 83 EC 30 48 8B ? ? ? ? 01 8A D9 80 F9 20");
            scanner.AddPattern(networkPlayerMgrPtrn, ptr =>
            {
                PointerData.NetworkPlayerMgr = Marshal.ReadIntPtr(ptr.Add(0xD).Rip().AsIntPtr());
            });

            Pattern getNetworkPlayerFromPidPtrn = new Pattern("GetNetworkPlayerFromPid", "E8 ? ? ? ? B2 01 8B CB 48 8B F8");
            scanner.AddPattern(getNetworkPlayerFromPidPtrn, ptr =>
            {
                PointerData.GetNetPlayerFromPid = Marshal.GetDelegateForFunctionPointer<Functions.GetNetworkPlayerFromPid>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern getNetObjectByIdPtrn = new Pattern("GetNetObjectById", "E8 ? ? ? ? 48 85 C0 74 20 80 78 47 00");
            scanner.AddPattern(getNetObjectByIdPtrn, ptr =>
            {
                PointerData.GetNetObjectById = Marshal.GetDelegateForFunctionPointer<Functions.GetNetObjectById>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern addExplosionBypass = new Pattern("ExplosionBypass", "0F 84 ? ? ? ? 44 38 3D ? ? ? ? 75 14");
            scanner.AddPattern(addExplosionBypass, ptr =>
            {
                PointerData.ExplosionBypass = ptr.Add(9).Rip().AsIntPtr(); //bool*
            });

            Pattern worldToScreenPtrn = new Pattern("WorldToScreen", "E8 ? ? ? ? 84 C0 74 19 F3 0F 10 44 24");
            scanner.AddPattern(worldToScreenPtrn, ptr =>
            {
                PointerData.WorldToScreen = Marshal.GetDelegateForFunctionPointer<Functions.WorldToScreen>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern writePlayerHealthDataPtrn = new Pattern("WritePlayerHealthData", "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 30 48 8B B1 A8");
            scanner.AddPattern(writePlayerHealthDataPtrn, ptr =>
            {
                PointerData.WritePlayerHealthData = ptr.AsIntPtr();
            });

            Pattern requestControlPtrn = new Pattern("RequestControl", "E8 ? ? ? ? 32 C0 48 83 C4 ? 5B C3 B0 ? EB ? 48 8D 0D");
            scanner.AddPattern(requestControlPtrn, ptr =>
            {
                PointerData.RequestControlOfNetObject = Marshal.GetDelegateForFunctionPointer<Functions.RequestControlOfNetObject>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern getAnimSceneFromHandlePtrn = new Pattern("GetAnimSceneFromHandle", "00 83 F9 04 7C 0F");
            scanner.AddPattern(getAnimSceneFromHandlePtrn, ptr =>
            {
                PointerData.GetAnimSceneFromHandle = Marshal.GetDelegateForFunctionPointer<Functions.GetAnimSceneFromHandle>(ptr.Sub(0x13).Rip().AsIntPtr());
            });

            Pattern networkObjectMgrPtrn = new Pattern("NetworkObjectMgr", "74 44 0F B7 56 40");
            scanner.AddPattern(networkObjectMgrPtrn, ptr =>
            {
                PointerData.NetworkObjectMgr = ptr.Add(0xC).Rip().AsIntPtr();
            });

            Pattern sendPacketPtrn = new Pattern("SendPacket", "8B 44 24 60 48 8B D6 48 8B CD");
            scanner.AddPattern(sendPacketPtrn, ptr =>
            {
                PointerData.SendPacket = Marshal.GetDelegateForFunctionPointer<Functions.SendPacket>(ptr.Add(0xE).Add(1).Rip().AsIntPtr());
            });

            Pattern queuePacketPtrn = new Pattern("QueuePacket", "E8 ?? ?? ?? ?? FF C6 49 83 C6 08 3B B7 88 40 00 00");
            scanner.AddPattern(queuePacketPtrn, ptr =>
            {
                PointerData.QueuePacket = Marshal.GetDelegateForFunctionPointer<Functions.QueuePacket>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern receiveNetMessagePtrn = new Pattern("ReceiveNetMessage", "E8 ?? ?? ?? ?? EB 24 48 8D B7 90 02 00 00");
            scanner.AddPattern(receiveNetMessagePtrn, ptr =>
            {
                PointerData.ReceiveNetMessage = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern handlePresenceEventPtrn = new Pattern("HandlePresenceEvent", "E8 ?? ?? ?? ?? 4C 8D 9C 24 B0 10 00 00 49 8B 5B 10");
            scanner.AddPattern(handlePresenceEventPtrn, ptr =>
            {
                PointerData.HandlePresenceEvent = ptr.Add(1).Rip().AsIntPtr();
            });

            Pattern postMessagePtrn = new Pattern("PostPresenceMessage", "E8 ?? ?? ?? ?? EB 35 C7 44 24 20 D9 7A 70 E1");
            scanner.AddPattern(postMessagePtrn, ptr =>
            {
                PointerData.PostPresenceMessage = Marshal.GetDelegateForFunctionPointer<Functions.PostPresenceMessage>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern sendNetInfoToLobbyPtrn = new Pattern("SendNetInfoToLobby", "E8 ?? ?? ?? ?? 32 DB 84 C0 74 1B 44 8B 84 24 40 01 00 00");
            scanner.AddPattern(sendNetInfoToLobbyPtrn, ptr =>
            {
                PointerData.SendNetInfoToLobby = Marshal.GetDelegateForFunctionPointer<Functions.SendNetInfoToLobby>(ptr.Add(1).Rip().AsIntPtr());
            });

            Pattern pedPoolPtrn = new Pattern("PedPool", "0F 28 F0 48 85 DB 74 56 8A 05 ? ? ? ? 84 C0 75 05");
            scanner.AddPattern(pedPoolPtrn, ptr =>
            {
                PointerData.PedPool = ptr.Add(10).Rip().AsIntPtr();
            });

            Pattern objectPoolPtrn = new Pattern("ObjectPool", "3C 05 75 67");
            scanner.AddPattern(objectPoolPtrn, ptr =>
            {
                PointerData.ObjectPool = ptr.Add(20).Rip().AsIntPtr();
            });

            Pattern vehiclePoolPtrn = new Pattern("VehiclePool", "48 83 EC 20 8A 05 ? ? ? ? 45 33 E4");
            scanner.AddPattern(vehiclePoolPtrn, ptr =>
            {
                PointerData.VehiclePool = ptr.Add(6).Rip().AsIntPtr();
            });

            Pattern pickupPoolPtrn = new Pattern("PickupPool", "0F 84 ? ? ? ? 8A 05 ? ? ? ? 48 85");
            scanner.AddPattern(pickupPoolPtrn, ptr =>
            {
                PointerData.PickupPool = ptr.Add(8).Rip().AsIntPtr();
            });

            Pattern fwScriptGuidCreateGuidPtrn = new Pattern("FwScriptGuidCreateGuid", "E8 ? ? ? ? B3 01 8B 15");
            scanner.AddPattern(fwScriptGuidCreateGuidPtrn, ptr =>
            {
                PointerData.FwScriptGuidCreateGuid = Marshal.GetDelegateForFunctionPointer<FwScriptGuidCreateGuid>(ptr.Sub(141).AsIntPtr());
            });

            Pattern receiveServerMessagePtrn = new Pattern("ReceiveServerMessage", "48 89 5C 24 08 57 48 83 EC 20 48 8B 02 48 8B F9 48 8B CA 48 8B DA FF 50 ?? 48 8B C8");
            scanner.AddPattern(receiveServerMessagePtrn, ptr =>
            {
                PointerData.ReceiveServerMessage = ptr.AsIntPtr();
            });

            Pattern serializeServerRPCPtrn = new Pattern("SerializeServerRPC", "48 89 5C 24 08 57 48 83 EC 30 48 8B 44 24 70");
            scanner.AddPattern(serializeServerRPCPtrn, ptr =>
            {
                PointerData.SerializeServerRPC = ptr.AsIntPtr();
            });

            Pattern readBBArrayPtrn = new Pattern("ReadBitBufferArray", "48 89 5C 24 08 57 48 83 EC 30 41 8B F8 4C");
            scanner.AddPattern(readBBArrayPtrn, ptr =>
            {
                PointerData.ReadBitBufferArray = Marshal.GetDelegateForFunctionPointer<Functions.ReadBitBufferArray>(ptr.AsIntPtr());
            });

            Pattern writeBBArrayPtrn = new Pattern("WriteBitBufferArray", "48 89 5C 24 08 57 48 83 EC 30 F6 41 28");
            scanner.AddPattern(writeBBArrayPtrn, ptr =>
            {
                PointerData.WriteBitBufferArray = Marshal.GetDelegateForFunctionPointer<Functions.WriteBitBufferArray>(ptr.AsIntPtr());
            });

            Pattern readBBStringPtrn = new Pattern("ReadBitBufferString", "48 89 5C 24 08 48 89 6C 24 18 56 57 41 56 48 83 EC 20 45 8B");
            scanner.AddPattern(readBBStringPtrn, ptr =>
            {
                PointerData.ReadBitBufferString = Marshal.GetDelegateForFunctionPointer<Functions.ReadBitBufferString>(ptr.AsIntPtr());
            });

            Pattern initNativeTablesPtrn = new Pattern("InitNativeTables", "41 B0 01 44 39 51 2C 0F");
            scanner.AddPattern(initNativeTablesPtrn, ptr =>
            {
                PointerData.InitNativeTables = ptr.Sub(0x10).AsIntPtr();
            });

            Pattern triggerWeaponDamageEventPtrn = new Pattern("TriggerWeaponDamageEvent", "89 44 24 58 8B 47 F8 89");
            scanner.AddPattern(triggerWeaponDamageEventPtrn, ptr =>
            {
                PointerData.TriggerWeaponDamageEvent = Marshal.GetDelegateForFunctionPointer<Functions.TriggerWeaponDamageEvent>(ptr.Add(0x39).Rip().AsIntPtr());
            });

            Pattern scSessionPtrn = new Pattern("ScSession", "3B 1D ? ? ? ? 76 60");
            scanner.AddPattern(scSessionPtrn, ptr =>
            {
                PointerData.ScSession = ptr.Add(0xB).Rip().AsIntPtr();
            });

            Pattern receiveArrayUpdatePtrn = new Pattern("ReceiveArrayUpdate", "48 89 5C 24 10 55 56 57 41 54 41 55 41 56 41 57 48 8B EC 48 83 EC 50 48 8B D9 45");
            scanner.AddPattern(receiveArrayUpdatePtrn, ptr =>
            {
                PointerData.ReceiveArrayUpdate = ptr.AsIntPtr();
            });

            Pattern writeVPMDataPtrn = new Pattern("WriteVehicleProximityMigrationData", "48 8B C4 48 89 58 10 48 89 68 18 48 89 70 20 48 89 48 08 57 41 54 41 55 41 56 41 57 48 83 EC 30 4C 8B A9");
            scanner.AddPattern(writeVPMDataPtrn, ptr =>
            {
                PointerData.WriteVPMData = ptr.AsIntPtr();
            });

            Pattern triggerGiveControlEventPtrn = new Pattern("TriggerGiveControlEvent", "48 8B C4 48 89 58 ? 48 89 68 ? 48 89 70 ? 48 89 78 ? 41 54 41 56 41 57 48 83 EC ? 65 4C 8B 0C 25");
            scanner.AddPattern(triggerGiveControlEventPtrn, ptr =>
            {
                PointerData.TriggerGiveControlEvent = Marshal.GetDelegateForFunctionPointer<Functions.TriggerGiveControlEvent>(ptr.AsIntPtr());
            });

            Pattern createPoolItemPtrn = new Pattern("CreatePoolItem", "E8 ? ? ? ? 48 85 C0 74 ? 44 8A 4C 24 ? 48 8B C8 44 0F B7 44 24 ? 0F B7 54 24");
            scanner.AddPattern(createPoolItemPtrn, ptr =>
            {
                PointerData.CreatePoolItem = ptr.Add(1).Rip().AsIntPtr();
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
            //scanner.ScanAsync(scanSuccess =>
            //{
            //    if (scanSuccess)
            //    {
            //        LOG.INFO("Patterns scanned successfully.");
            //        return true;
            //    }
            //    else
            //    {
            //        LOG.ERROR("Some patterns could not be found. Stopping here.");
            //        return false;
            //    }
            //});

            // May need to change this
            // Ensure the delegate is not null
            if (PointerData.GetRendererInfo != null)
            {
                // Call the delegate to get the RenderingInfo instance
                Utils.RenderingInfo rendererInfo = PointerData.GetRendererInfo();

                // Check the rendering type using the method
                if (rendererInfo.is_rendering_type(ERenderingType.DX12))
                {
                    PointerData.IsVulkan = false;
                }
                else if (rendererInfo.is_rendering_type(ERenderingType.Vulkan))
                {
                    PointerData.IsVulkan = true;
                }
                else
                {
                    LOG.INFO("Unknown renderer type!");
                    return false;
                }
            }
            else
            {
                LOG.INFO("GetRendererInfo delegate is not initialized!");
                return false;
            }

            return true;
        }
    }
}