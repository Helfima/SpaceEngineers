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

            public string Search_Lcd_Ore;
            public string Search_Lcd_Ingot;
            public string Search_Lcd_Component;
            public string Search_Lcd_Ammo;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                Search_Lcd_Ore = MyIni.Get("Search", "LcdOre").ToString("ORE");
                Search_Lcd_Ingot = MyIni.Get("Search", "LcdIngot").ToString("INGOT");
                Search_Lcd_Component = MyIni.Get("Search", "LcdComponent").ToString("COMPONENT");
                Search_Lcd_Ammo = MyIni.Get("Search", "LcdAmmo").ToString("AMMO");
                if (program.Me.CustomData.Equals(""))
                {
                    Save();
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

            public Color GetColor(string section, string key)
            {
                string colorValue = MyIni.Get(section, key).ToString("128,128,128,255");
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

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Search", "LcdOre", Search_Lcd_Ore);
                MyIni.Set("Search", "LcdIngot", Search_Lcd_Ingot);
                MyIni.Set("Search", "LcdComponent", Search_Lcd_Component);
                MyIni.Set("Search", "LcdAmmo", Search_Lcd_Ammo);
                MyIni.Set("Limit", "default", 10000);
                MyIni.Set("Color", "default", "128,128,128,255");
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
