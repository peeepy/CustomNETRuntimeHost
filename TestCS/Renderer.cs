// using System;
// using System.Collections.Generic;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using ImGuiNET;
//
// namespace RDR2CS
// {
//     public partial class Renderer
//     {
//         // Delegate types
//         [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
//         private delegate void RenderCallbackDelegate(IntPtr userData);
//
//         [UnmanagedFunctionPointer(CallingConvention.StdCall)]
//         private delegate void WindowProcCallbackFunction(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);
//
//         // P/Invoke declarations
//         [LibraryImport("RDONatives.dll", EntryPoint = "AddRendererCallBackWrapper")]
//         [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
//         [return: MarshalAs(UnmanagedType.Bool)]
//         private static partial bool AddRendererCallBackWrapper(RenderCallbackDelegate callback, IntPtr userData, uint priority);
//
//         [LibraryImport("RDONatives.dll", EntryPoint = "AddWindowProcedureCallbackWrapper")]
//         [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
//         static partial void AddWindowProcedureCallbackWrapper(WindowProcCallbackFunction callback);
//
//         [LibraryImport("RDONatives.dll", EntryPoint = "RendererDestroy")]
//         [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
//         private static partial void RendererDestroy();
//
//         [LibraryImport("RDONatives.dll", EntryPoint = "RendererInit")]
//         [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
//         [return: MarshalAs(UnmanagedType.Bool)]
//         private static partial bool RendererInit();
//
//         // Storage for callbacks
//         private static Dictionary<uint, (Action Callback, RenderCallbackDelegate NativeCallback)> _renderCallbacks
//             = new Dictionary<uint, (Action, RenderCallbackDelegate)>();
//
//         private static List<WindowProcCallbackFunction> _windowProcCallbacks = new List<WindowProcCallbackFunction>();
//
//         // Public methods
//         public static bool AddRendererCallback(Action callback, uint priority)
//         {
//             if (_renderCallbacks.ContainsKey(priority))
//             {
//                 return false; // Already exists
//             }
//
//             RenderCallbackDelegate nativeCallback = (IntPtr userData) => { callback(); };
//
//             bool success = AddRendererCallBackWrapper(nativeCallback, IntPtr.Zero, priority);
//
//             if (success)
//             {
//                 _renderCallbacks[priority] = (callback, nativeCallback);
//             }
//
//             return success;
//         }
//
//         public static void AddWindowProcedureCallback(Action<IntPtr, uint, IntPtr, IntPtr> callback)
//         {
//             WindowProcCallbackFunction wrappedCallback = (hwnd, uMsg, wParam, lParam) =>
//             {
//                 callback(hwnd, uMsg, wParam, lParam);
//             };
//             _windowProcCallbacks.Add(wrappedCallback);
//             AddWindowProcedureCallbackWrapper(wrappedCallback);
//         }
//
//         public static void Destroy()
//         {
//             // Call the native Destroy implementation
//             RendererDestroy();
//
//             // After native destruction, destroy the ImGui context
//             ImGui.DestroyContext();
//         }
//
//         public static bool Init()
//         {
//             return RendererInit();
//         }
//     };
// }