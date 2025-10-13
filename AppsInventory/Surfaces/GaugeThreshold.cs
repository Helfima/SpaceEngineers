using VRageMath;

namespace AppsInventory.Surfaces
{
    public class GaugeThreshold
    {
        public GaugeThreshold()
        {

        }
        public GaugeThreshold(float value, Color color)
        {
            Value = value;
            Color = color;
        }
        public float Value { get; set; }
        public Color Color { get; set; }

        public override string ToString()
        {
            return $"{Value}:{Color.R},{Color.G},{Color.B},{Color.A}";
        }
    }
}
