using System;
using System.Collections.Generic;
using System.Numerics;

namespace TestCS.GUI
{
    public class Submenu
    {
        public string Name { get; }
        public string Icon { get; } // currently unused

        private Category _activeCategory;
        public List<Category> Categories { get; } = new List<Category>();

        public Submenu(string name, string icon = "")
        {
            Name = name;
            Icon = icon;
        }

        public Category GetActiveCategory() => _activeCategory;

        public void AddCategory(Category category)
        {
            if (_activeCategory == null)
                _activeCategory = category;

            Categories.Add(category);
        }

        public void DrawCategorySelectors()
        {
            foreach (var category in Categories)
            {
                if (category != null)
                {
                    var style = ImGui.GetStyle();
                    var color = style.Colors[(int)ImGuiCol.Button];
                    color.W -= 0.5f;

                    bool active = category == GetActiveCategory();

                    if (!active)
                        ImGui.PushStyleColorVec4((int)ImGuiCol.Button, color);

                    if (ImGui.Button(category.Name, new ImVec2 { X=category.GetLength(), Y = 35 }))
                    {
                        SetActiveCategory(category);
                    }

                    if (!active)
                        ImGui.PopStyleColor(1);

                    if (Categories[Categories.Count - 1] != category)
                        ImGui.SameLine(0.0f, -1.0f);
                }
            }
        }

        public void SetActiveCategory(Category category)
        {
            _activeCategory = category;
        }

        public void Draw()
        {
            _activeCategory?.Draw();
        }
    }
}