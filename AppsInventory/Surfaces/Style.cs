using System;
using System.Collections.Generic;
using AppsInventory.Extensions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory.Surfaces
{
    public class Style
    {
        public StylePadding Padding { get; set; } = new StylePadding(2);
        public StyleMargin Margin { get; set; } = new StyleMargin(2);
        public float Width { get; set; } = 50f;
        public float Height { get; set; } = 50f;
        public float RotationOrScale { get; set; } = 1f;
        public Color Color { get; set; } = new Color(100, 100, 100, 128);
        public float ColorSoftening { get; set; } = 1f;
        public virtual void Scale(float scale)
        {
            Width *= scale;
            Height *= scale;
            Padding.Scale(scale);
            Margin.Scale(scale);
        }
    }
}
