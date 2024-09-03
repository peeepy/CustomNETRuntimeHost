using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TestCS.GUI
{
    public class Category
    {
        public string Name { get; }
        private List<UIItem> _items = new List<UIItem>();
        private int? _length;

        public Category(string name)
        {
            Name = name;
        }

        public void AddItem(UIItem item)
        {
            _items.Add(item);
        }

        public void Draw()
        {
            foreach (var item in _items)
            {
                item.Draw();
            }
        }

        public unsafe int GetLength()
        {
            if (_length.HasValue)
                return _length.Value;
            var vecInternal = new ImVec2.__Internal();
            ImGui.__Internal.CalcTextSize((IntPtr)(&vecInternal), Name, null, false, -1.0f);
            _length = (int)Math.Max(vecInternal.x + 25.0f, 75.0f);
            //_length = (int)Math.Max(ImGui.CalcTextSize(Name).X + 25.0f, 75.0f);
            return _length.Value;
        }
    }
}
