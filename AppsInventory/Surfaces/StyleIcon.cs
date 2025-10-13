using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInventory.Surfaces
{
    public class StyleIcon : Style
    {
        public string path { get; set; }
        public GaugeThresholds Thresholds { get; set; } = new GaugeThresholds();
    }
}
