using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInventory.Surfaces
{
    public class StylePadding
    {
        public StylePadding(float x = 2, float y = 2)
        {
            X = x;
            Y = y;
        }
        public StylePadding(float value)
        {
            X = value;
            Y = value;
        }

        public float X = 2;
        public float Y = 2;

        public virtual void Scale(float scale)
        {
            X *= scale;
            Y *= scale;
        }
    }
}
