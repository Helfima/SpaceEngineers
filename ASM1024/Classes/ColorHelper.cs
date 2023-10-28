using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class ColorHelper
        {
            public static Color Rainbow(double progress)
            {
                double div = (Math.Abs(progress % 1) * 6);
                int ascending = (int)((div % 1) * 255);
                int descending = 255 - ascending;

                switch ((int)div)
                {
                    case 0:
                        return new Color(255, 255, ascending, 0);
                    case 1:
                        return new Color(255, descending, 255, 0);
                    case 2:
                        return new Color(255, 0, 255, ascending);
                    case 3:
                        return new Color(255, 0, descending, 255);
                    case 4:
                        return new Color(255, ascending, 0, 255);
                    default: // case 5:
                        return new Color(255, 255, 0, descending);
                }
            }
        }
    }
}
