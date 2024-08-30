using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestCS.Memory;

namespace TestCS.GUI
{
    public class Menu
    {
        public static ImFont g_DefaultFont { get; set; }
        public static ImFont g_OptionsFont { get; set; }
        public static ImFont g_ChildTitleFont { get; set; }
        public static ImFont g_ChatFont { get; set; }

        public static void Init()
        {
            UIManager.AddSubmenu(new Submenu("Self"));
            //Renderer.AddRendererCallback(() =>
            //);
        }

        public static void Draw()
        {
            if (!GUIMgr.IsOpen()) { return; }

            //ImGui.PushFont(g_DefaultFont);
            ImGui.PushStyleColorVec4((int)ImGuiCol.WindowBg, new ImVec4 { X = 15.0f, W = 15.0f, Y = 15.0f, Z = 0.0f });

            ImGui.SetNextWindowSize(new ImVec2
            {
                X = (float)(PointerData.ScreenResX / 2.5f),
                Y =
                (float)(PointerData.ScreenResY / 2.5f)
            }, (int)ImGuiCond.Once);

            bool dummy = false;
            if (ImGui.Begin("TestCS", ref dummy, (int)ImGuiWindowFlags.NoDecoration)) // p_open true adds a window-clicking widget that sets p_open to false when clicked
            {
                //if (ImGui.Button("Unload", new ImVec2 { X = 120, Y = 0 }))
                //{
                //    if (ScriptManager.Instance.CanTick())
                //    {
                //        FiberPool.Push(() =>
                //        {
                //            //Commands.Shutdown()
                //            g_IsRunning = false;
                //        });
                //    }
                //    else
                //    {
                //        g_IsRunning = false;
                //    }
                //}
                UIManager.Draw();
                ImGui.End();
            }
            ImGui.PopStyleColor(1);
            //ImGui.PopFont();
        }


            public static void SetupStyle()
        {
            ImGuiStyle style = ImGui.GetStyle();
            //var text = new ImVec4();
            style.Colors[(int)ImGuiCol.Text] = new ImVec4 { X = 1.00f, W = 1.00f, Y = 1.00f, Z = 1.00f };
            style.Colors[(int)ImGuiCol.TextDisabled] = new ImVec4 { X = 0.50f, W = 0.50f, Y = 0.50f, Z = 1.00f };
            style.Colors[(int)ImGuiCol.WindowBg] = new ImVec4 { X = 0.13f, Y = 0.14f, Z = 0.15f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ChildBg] = new ImVec4 { X = 0.13f, Y = 0.14f, Z = 0.15f, W = 1.00f };
            style.Colors[(int)ImGuiCol.PopupBg] = new ImVec4 { X = 0.13f, Y = 0.14f, Z = 0.15f, W = 1.00f };
            style.Colors[(int)ImGuiCol.Border] = new ImVec4 { X = 0.43f, Y = 0.43f, Z = 0.50f, W = 0.50f };
            style.Colors[(int)ImGuiCol.BorderShadow] = new ImVec4 { X = 0.00f, Y = 0.00f, Z = 0.00f, W = 0.00f };
            style.Colors[(int)ImGuiCol.FrameBg] = new ImVec4 { X = 0.25f, Y = 0.25f, Z = 0.25f, W = 1.00f };
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new ImVec4 { X = 0.38f, Y = 0.38f, Z = 0.38f, W = 1.00f };
            style.Colors[(int)ImGuiCol.FrameBgActive] = new ImVec4 { X = 0.67f, Y = 0.67f, Z = 0.67f, W = 0.39f };
            style.Colors[(int)ImGuiCol.TitleBg] = new ImVec4 { X = 0.08f, Y = 0.08f, Z = 0.09f, W = 1.00f };
            style.Colors[(int)ImGuiCol.TitleBgActive] = new ImVec4 { X = 0.08f, Y = 0.08f, Z = 0.09f, W = 1.00f };
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new ImVec4 { X = 0.00f, Y = 0.00f, Z = 0.00f, W = 0.51f };
            style.Colors[(int)ImGuiCol.MenuBarBg] = new ImVec4 { X = 0.14f, Y = 0.14f, Z = 0.14f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new ImVec4 { X = 0.02f, Y = 0.02f, Z = 0.02f, W = 0.53f };
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new ImVec4 { X = 0.31f, Y = 0.31f, Z = 0.31f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new ImVec4 { X = 0.41f, Y = 0.41f, Z = 0.41f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new ImVec4 { X = 0.51f, Y = 0.51f, Z = 0.51f, W = 1.00f };
            style.Colors[(int)ImGuiCol.CheckMark] = new ImVec4 { X = 0.11f, Y = 0.64f, Z = 0.92f, W = 1.00f };
            style.Colors[(int)ImGuiCol.SliderGrab] = new ImVec4 { X = 0.11f, Y = 0.64f, Z = 0.92f, W = 1.00f };
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new ImVec4 { X = 0.08f, Y = 0.50f, Z = 0.72f, W = 1.00f };
            style.Colors[(int)ImGuiCol.Button] = new ImVec4 { X = 0.25f, Y = 0.25f, Z = 0.25f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ButtonHovered] = new ImVec4 { X = 0.38f, Y = 0.38f, Z = 0.38f, W = 1.00f };
            style.Colors[(int)ImGuiCol.ButtonActive] = new ImVec4 { X = 0.67f, Y = 0.67f, Z = 0.67f, W = 0.39f };
            style.Colors[(int)ImGuiCol.Header] = new ImVec4 { X = 0.22f, Y = 0.22f, Z = 0.22f, W = 1.00f };
            style.Colors[(int)ImGuiCol.HeaderHovered] = new ImVec4 { X = 0.25f, Y = 0.25f, Z = 0.25f, W = 1.00f };
            style.Colors[(int)ImGuiCol.HeaderActive] = new ImVec4 { X = 0.67f, Y = 0.67f, Z = 0.67f, W = 0.39f };
            style.Colors[(int)ImGuiCol.Separator] = style.Colors[(int)ImGuiCol.Border];
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new ImVec4 { X = 0.41f, Y = 0.42f, Z = 0.44f, W = 1.00f };
            style.Colors[(int)ImGuiCol.SeparatorActive] = new ImVec4 { X = 0.26f, Y = 0.59f, Z = 0.98f, W = 0.95f };
            style.Colors[(int)ImGuiCol.ResizeGrip] = new ImVec4 { X = 0.00f, Y = 0.00f, Z = 0.00f, W = 0.00f };
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new ImVec4 { X = 0.29f, Y = 0.30f, Z = 0.31f, W = 0.67f };
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new ImVec4 { X = 0.26f, Y = 0.59f, Z = 0.98f, W = 0.95f };
            style.Colors[(int)ImGuiCol.Tab] = new ImVec4 { X = 0.08f, Y = 0.08f, Z = 0.09f, W = 0.83f };
            style.Colors[(int)ImGuiCol.TabHovered] = new ImVec4 { X = 0.33f, Y = 0.34f, Z = 0.36f, W = 0.83f };
            style.Colors[(int)ImGuiCol.TabSelected] = new ImVec4 { X = 0.23f, Y = 0.23f, Z = 0.24f, W = 1.00f };
            style.Colors[(int)ImGuiCol.PlotLines] = new ImVec4 { X = 0.61f, Y = 0.61f, Z = 0.61f, W = 1.00f };
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new ImVec4 { X = 1.00f, Y = 0.43f, Z = 0.35f, W = 1.00f };
            style.Colors[(int)ImGuiCol.PlotHistogram] = new ImVec4 { X = 0.90f, Y = 0.70f, Z = 0.00f, W = 1.00f };
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new ImVec4 { X = 1.00f, Y = 0.60f, Z = 0.00f, W = 1.00f };
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new ImVec4 { X = 0.26f, Y = 0.59f, Z = 0.98f, W = 0.35f };
            style.Colors[(int)ImGuiCol.DragDropTarget] = new ImVec4 { X = 0.11f, Y = 0.64f, Z = 0.92f, W = 1.00f };
            style.Colors[(int)ImGuiCol.NavHighlight] = new ImVec4 { X = 0.26f, Y = 0.59f, Z = 0.98f, W = 1.00f };
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new ImVec4 { X = 1.00f, Y = 1.00f, Z = 1.00f, W = 0.70f };
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new ImVec4 { X = 0.80f, Y = 0.80f, Z = 0.80f, W = 0.20f };
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new ImVec4 { X = 0.80f, Y = 0.80f, Z = 0.80f, W = 0.35f };
            style.GrabRounding = style.FrameRounding = style.ChildRounding = style.WindowRounding = 2.3f;
        }

        //public static void SetupFonts()
        //{
        //    var IO = ImGui.GetIO();
        //    ImFontConfig fontConfig = new();
        //    fontConfig.FontDataOwnedByAtlas = false;

        //    //g_DefaultFont = Renderer.("D:\\Coding\\csharp\\TestCS\\Utils\\Fonts\\DroidSans.ttf\\", 12.0f);
        //    //g_OptionsFont = IO.Fonts.AddFontFromFileTTF("D:\\Coding\\csharp\\TestCS\\Utils\\Fonts\\Roboto-Medium.ttf\\", 12.0f);
        //    //g_ChildTitleFont = IO.Fonts.AddFontFromFileTTF("D:\\Coding\\csharp\\TestCS\\Utils\\Fonts\\Roboto-Medium.ttf\\", 12.0f);
        //    //g_ChatFont = IO.Fonts.AddFontFromFileTTF("D:\\Coding\\csharp\\TestCS\\Utils\\Fonts\\DroidSans.ttf\\", 11.0f);

        //}
    }
}
