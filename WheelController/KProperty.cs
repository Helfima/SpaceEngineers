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
using System.Text.RegularExpressions;

namespace IngameScript
{
    partial class Program
    {
        public class KProperty
        {
            protected MyIni MyIni = new MyIni();
            protected Program program;

            public string color_default = "128,128,128,255";

            public string cockpit_filter;
            public string wheel_filter;
            public float wheel_handbrake;
            public string stator_filter;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                cockpit_filter = MyIni.Get("Cockpit", "filter").ToString("M:*");
                wheel_filter = MyIni.Get("Wheel", "filter").ToString("M:*");
                wheel_handbrake = MyIni.Get("Wheel", "hand_brake").ToSingle(0.1f);
                stator_filter = MyIni.Get("Stator", "filter").ToString("M:*");


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
                MyIni.Set("Cockpit", "filter", cockpit_filter);
                MyIni.Set("Wheel", "filter", wheel_filter);
                MyIni.Set("Wheel", "hand_brake", wheel_handbrake);
                MyIni.Set("Stator", "filter", stator_filter);

                program.Me.CustomData = MyIni.ToString();
            }
            public string Get(string section, string key, string default_value = "")
            {
                return MyIni.Get(section, key).ToString(default_value);
            }

            public int GetInt(string section, string key, int default_value = 0)
            {
                return MyIni.Get(section, key).ToInt32(default_value);
            }

            public Color GetColor(string section, string key, string default_value = null)
            {
                if (default_value == null) default_value = color_default;
                string colorValue = MyIni.Get(section, key).ToString(default_value);
                Color color = Color.Gray;
                // Find matches.
                //program.drawingSurface.WriteText($"{section}/{key}={colorValue}", true);
                if (!colorValue.Equals(""))
                {
                    string[] colorSplit = colorValue.Split(',');
                    color = new Color(int.Parse(colorSplit[0]), int.Parse(colorSplit[1]), int.Parse(colorSplit[2]), int.Parse(colorSplit[3]));
                }
                return color;
            }

            
        }
    }
}
