using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCS.Utils
{
    public enum ERenderingType : uint
    {
        DX12 = 1,
	    Unk,
	    Vulkan
    };

    public class RenderingInfo
    {

	    public uint unk_0008; //0x0008
        ERenderingType _RenderingType; //0x0009

        public bool is_rendering_type(ERenderingType type)
        {
            return _RenderingType == type;
        }
        public void set_rendering_type(ERenderingType type)
        {
            if (!is_rendering_type(type))
            {
                _RenderingType = type;
            }
        }
    };
}
