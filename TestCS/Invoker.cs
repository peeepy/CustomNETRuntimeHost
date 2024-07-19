// using System;
// using System.Runtime.InteropServices;
//
// public static class NativeInvoker
// {
//     [DllImport("NativeInvokerWrapper.dll")]
//     private static extern void InvokeNative(int index, int argCount, ulong[] args, IntPtr result);
//
//     public static T Invoke<T>(int index, params object[] args)
//     {
//         ulong[] nativeArgs = new ulong[args.Length];
//         for (int i = 0; i < args.Length; i++)
//         {
//             nativeArgs[i] = Convert.ToUInt64(args[i]);
//         }
//
//         if (typeof(T) == typeof(void))
//         {
//             InvokeNative(index, args.Length, nativeArgs, IntPtr.Zero);
//             return default;
//         }
//         else
//         {
//             int size = Marshal.SizeOf<T>();
//             IntPtr resultPtr = Marshal.AllocHGlobal(size);
//             try
//             {
//                 InvokeNative(index, args.Length, nativeArgs, resultPtr);
//                 return Marshal.PtrToStructure<T>(resultPtr);
//             }
//             finally
//             {
//                 Marshal.FreeHGlobal(resultPtr);
//             }
//         }
//     }
//
//     // Example wrappers
//     public static void WAIT(int ms) => Invoke<object>(0, ms);
//     public static int TIMERA() => Invoke<int>(1);
//     public static float GET_RANDOM_FLOAT_IN_RANGE(float min, float max) => Invoke<float>(2, min, max);
//     public static bool IS_ENTITY_DEAD(int entity) => Invoke<bool>(3, entity);
//
//     // You can add more native function wrappers here
// }