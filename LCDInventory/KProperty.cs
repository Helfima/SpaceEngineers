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

            public string limit_default;
            public string color_default;

            public bool multigrid_inventory;
            public bool multigrid_lcd;
            public bool multigrid_drills;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                limit_default = MyIni.Get("Limit", "default").ToString("10000");
                color_default = MyIni.Get("Color", "default").ToString("128,128,128,255");

                multigrid_inventory = MyIni.Get("MultiGrid", "inventory").ToBoolean(false);
                multigrid_lcd = MyIni.Get("MultiGrid", "lcd").ToBoolean(false);
                multigrid_drills = MyIni.Get("MultiGrid", "drills").ToBoolean(true);

                if (program.Me.CustomData.Equals(""))
                {
                    Save(true);
                }
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

            public void Save(bool prepare = false)
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("MultiGrid", "inventory", multigrid_inventory);
                MyIni.Set("MultiGrid", "lcd", multigrid_lcd);
                MyIni.Set("MultiGrid", "drills", multigrid_drills);

                MyIni.Set("Limit", "default", limit_default);
                if (prepare)
                {
                    MyIni.Set("Limit", "Cobalt", "1000");
                    MyIni.Set("Limit", "Iron", "100000");
                    MyIni.Set("Limit", "Gold", "1000");
                    MyIni.Set("Limit", "Platinum", "1000");
                    MyIni.Set("Limit", "Silver", "1000");
                }
                MyIni.Set("Color", "default", color_default);
                if (prepare)
                {
                    MyIni.Set("Color", "Cobalt", "000,080,080,255");
                    MyIni.Set("Color", "Gold", "255,153,000,255");
                    MyIni.Set("Color", "Ice", "040,130,130,255");
                    MyIni.Set("Color", "Iron", "040,040,040,255");
                    MyIni.Set("Color", "Nickel", "110,080,080,255");
                    MyIni.Set("Color", "Platinum", "120,150,120,255");
                    MyIni.Set("Color", "Silicon", "150,150,150,255");
                    MyIni.Set("Color", "Silver", "120,120,150,255");
                    MyIni.Set("Color", "Stone", "120,040,000,200");
                    MyIni.Set("Color", "Uranium", "040,130,000,200");
                }
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
