using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class KProperty
        {
            protected MyIni MyIni = new MyIni();
            protected Program program;

            public bool refinery_rotation;
            public string refinery_priority;
            public string refinery_filter;
            public string lcd_filter;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                refinery_filter = MyIni.Get("Refinery", "filter").ToString("refinery");
                refinery_rotation = MyIni.Get("Refinery", "rotation").ToBoolean(false);
                refinery_priority = MyIni.Get("Refinery", "priority").ToString("stone");
                lcd_filter = MyIni.Get("Lcd", "filter").ToString("refinery");
                if (program.Me.CustomData.Equals(""))
                {
                    Save();
                }
            }

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Refinery", "filter", refinery_filter);
                MyIni.Set("Refinery", "rotation", refinery_rotation);
                MyIni.Set("Refinery", "priority", refinery_priority);
                MyIni.Set("Lcd", "filter", lcd_filter);
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
