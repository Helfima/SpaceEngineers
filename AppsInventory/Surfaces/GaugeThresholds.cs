using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsInventory.Surfaces
{
    public class GaugeThresholds
    {
        public List<GaugeThreshold> Thresholds { get; set; } = new List<GaugeThreshold>();
        public GaugeThreshold GetGaugeThreshold(float value)
        {
            GaugeThreshold gaugeThreshold = Thresholds.First();
            foreach (var threshold in Thresholds)
            {
                if (value >= threshold.Value)
                {
                    gaugeThreshold = threshold;
                }
            }
            return gaugeThreshold;
        }
    }
}
