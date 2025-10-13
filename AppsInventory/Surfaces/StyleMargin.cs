using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInventory.Surfaces
{
    public class StyleMargin : StylePadding
    {
        public StyleMargin(float x = 2, float y = 2)
        {
            X = x;
            Y = y;
        }
        public StyleMargin(float value)
        {
            X = value;
            Y = value;
        }
    }
}
