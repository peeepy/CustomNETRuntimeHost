using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCS.Hooking.Hooks.GUI;
using TestCS.GUI;

namespace TestCS.GUI
{
    public class GUIMgr
    {
        private static GUIMgr _Instance;
        private bool _IsOpen = false;
        //private static POINT CursorCoords;

        private GUIMgr()
        {
            //Renderer.GetInstance().AddWindowProcedureCallback((hWnd, msg, wParam, lParam) =>
            //{
            //    GetInstance().WndProc(hWnd, msg, wParam, lParam);
            //});

            //Renderer.AddRendererCallback(() =>
            //{
            //    Notifications.Draw();
            //});
            //GUI.Menu.SetupFonts();
            GUI.Menu.SetupStyle();
            GUI.Menu.Init();
        }

        private static GUIMgr GetInstance()
        {
            _Instance = new GUIMgr();
            return _Instance;
        }

        public static GUIMgr Init()
        {
            return GetInstance();
        }

        public static bool IsOpen()
        {
            return GetInstance()._IsOpen;
        }

        public static void Toggle()
        {
            GetInstance()._IsOpen ^= true;
            LOG.INFO("Toggled GUI");
        }

        public static bool IsUsingKeyboard()
        {
            return ImGui.GetIO().WantTextInput;
        }

        public static void ToggleMouse()
        {
            var io = ImGui.GetIO();
            io.MouseDrawCursor = IsOpen();
            if (IsOpen())
                io.ConfigFlags &= ~(int)ImGuiConfigFlags.NoMouse;
            else
                io.ConfigFlags |= (int)ImGuiConfigFlags.NoMouse;
        }

        //public static void WndProc(IntPtr hWnd, uint msg, IntPtr wparam, IntPtr lparam)
        //{

        //    if (msg == (int)Keys.Up && wparam == new IntPtr((int)Keys.F11))
        //    {
        //        //if (_IsOpen)
        //        //{
        //        //    GetCursorPos(out CursorCoords);
        //        //}
        //        //else if (CursorCoords.X != 0 || CursorCoords.Y != 0)
        //        //{
        //        //    SetCursorPos(CursorCoords.X, CursorCoords.Y);
        //        //}
        //        Toggle();
        //        ToggleMouse();
        //    }
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct POINT
        //{
        //    public int X;
        //    public int Y;
        //}

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetCursorPos(out POINT lpPoint);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool SetCursorPos(int X, int Y);
    }
}
