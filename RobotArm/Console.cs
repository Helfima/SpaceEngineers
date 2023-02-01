using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    public class Console
    {
        private IMyTextSurface drawingSurface;
        public Console(IMyTextSurface drawingSurface)
        {
            this.drawingSurface = drawingSurface;
            this.drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
        }
        public void Clear()
        {
            drawingSurface.WriteText($"", false);
        }
        public void WriteLine(string message)
        {
            drawingSurface.WriteText($"{message}\n", true);
        }
        public static string RoundVector(Vector3 vector, int decimals)
        {
            return $"X:{Math.Round(vector.X, decimals)} Y:{Math.Round(vector.Y, decimals)} Z:{Math.Round(vector.Z, decimals)}";
        }
    }
}
