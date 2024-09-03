using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TestCS.GUI
{
    public class UIManager
    {
        //private static readonly Lazy<UIManager> _instance = new Lazy<UIManager>(() => new UIManager());

        private UIManager() { }

        private static UIManager _Instance;

        private Submenu _activeSubmenu;
        private readonly List<Submenu> _submenus = new List<Submenu>();
        private ImFont _optionsFont;
        private static UIManager GetInstance()
        {
            if ( _Instance == null)
            {
                _Instance = new UIManager();
            }
            
            return _Instance;
        }
        public static void AddSubmenu(Submenu submenu)
        {
            GetInstance().AddSubmenuImpl(submenu);
        }
        private void AddSubmenuImpl(Submenu submenu)
        {
            if (_activeSubmenu == null)
                _activeSubmenu = submenu;

            _submenus.Add(submenu);
        }

        public static void SetActiveSubmenu(Submenu submenu)
        {
            GetInstance().SetActiveSubmenuImpl(submenu);
        }
        private void SetActiveSubmenuImpl(Submenu submenu)
        {
            _activeSubmenu = submenu;
        }

        public static void Draw()
        {
            GetInstance().DrawImpl();
        }
        private unsafe void DrawImpl()
        {
            //var cursorPos = new ImVec2();
            //ImGui.GetCursorPos(cursorPos);
            var availContentRegion = new ImVec2.__Internal();
            ImGui.__Internal.GetContentRegionAvail((IntPtr)(&availContentRegion));
            if (ImGui.BeginChildStr("##submenus", new ImVec2 { X = 120, Y = availContentRegion.y - 20 }, (int)ImGuiChildFlags.Border, 0))
            {
                foreach (var submenu in _submenus)
                {
                    if (ImGui.SelectableBool(submenu.Name, submenu == _activeSubmenu, 0, new ImVec2 { X = 0.0f, Y = 0.0f }))
                    {
                        SetActiveSubmenu(submenu);
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Text("TestCS");

            //cursorPos.Y -= 28;
            //ImGui.SetCursorPos(new ImVec2 { X=cursorPos.X + 130, Y=cursorPos.Y });

            if (ImGui.BeginChildStr("##minisubmenus", new ImVec2 { X = 0, Y = 50 }, (int)ImGuiChildFlags.Border, (int)ImGuiWindowFlags.NoScrollbar))
            {
                _activeSubmenu?.DrawCategorySelectors();
            }
            ImGui.EndChild();

            //ImGui.SetCursorPos(new ImVec2 { X = cursorPos.X + 130, Y = cursorPos.Y + 60 });

            //if (ImGui.BeginChildStr("##options", new ImVec2 { X = 0, Y = 50 }, (int)ImGuiChildFlags.Border, null))
            //{
            //    if (_optionsFont)
            //        ImGui.PushFont(_optionsFont);

            //    _activeSubmenu?.Draw();

            //    if (_optionsFont.IsLoaded())
            //        ImGui.PopFont();
            //}
            //ImGui.EndChild();
        }

        public Submenu GetActiveSubmenu() => _activeSubmenu;

        public Category GetActiveCategory() => _activeSubmenu?.GetActiveCategory();

        public void SetOptionsFont(ImFont font)
        {
            _optionsFont = font;
        }
    }
}
