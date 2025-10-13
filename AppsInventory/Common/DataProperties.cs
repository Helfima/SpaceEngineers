using System;
using System.Collections.Generic;
using AppsInventory.Extensions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRage.Game.ModAPI.Ingame.Utilities;
using AppsInventory.Surfaces;
using Sandbox.ModAPI;

namespace AppsInventory.Common
{
    internal class DataProperties
    {
        protected MyIni MyIni = new MyIni();
        protected IMyTerminalBlock terminalBlock;
        public DataProperties(IMyTerminalBlock terminalBlock) {
            this.terminalBlock = terminalBlock;
        }
        public void Load()
        {
            MyIniParseResult result;
            if (!MyIni.TryParse(this.terminalBlock.CustomData, out result))
                throw new Exception(result.ToString());
        }
        public void Save()
        {
            this.terminalBlock.CustomData = MyIni.ToString();
        }

        public string limit_default;
        public string color_default;
        public GaugeThresholds LoadThresholds(string section, bool overflowDefault)
        {
            if (MyIni.ContainsSection(section))
            {
                var thresholds = new GaugeThresholds();
                var found = true;
                var index = 1;
                while (found)
                {
                    var thresholdName = $"threshold_{index}";
                    var threshold = MyIni.Get(section, thresholdName);
                    if (threshold.IsEmpty == false)
                    {
                        var value = threshold.ToString();
                        var values = value.Split(':');
                        var gaugeThreshold = new GaugeThreshold();
                        gaugeThreshold.Value = float.Parse(values[0]);
                        gaugeThreshold.Color = ParseColor(values[1]);
                        thresholds.Thresholds.Add(gaugeThreshold);
                    }
                    else
                    {
                        found = false;
                    }
                    index++;
                }

            }
            return null;
        }
        public bool HasSection(string section)
        {
            return MyIni.ContainsSection(section);
        }
        public string Get(string section, string key, string default_value = "")
        {
            return MyIni.Get(section, key).ToString(default_value);
        }

        public int GetInt(string section, string key, int default_value = 0)
        {
            return MyIni.Get(section, key).ToInt32(default_value);
        }

        public bool GetBoolean(string section, string key, bool default_value = false)
        {
            return MyIni.Get(section, key).ToBoolean(default_value);
        }
        public float GetSingle(string section, string key, float default_value = 0f)
        {
            return MyIni.Get(section, key).ToSingle(default_value);
        }
        public void Set(string section, string key, string value)
        {
            MyIni.Set(section, key, value);
        }
        public void Set(string section, string key, int value)
        {
            MyIni.Set(section, key, value);
        }
        public void Set(string section, string key, bool value)
        {
            MyIni.Set(section, key, value);
        }
        public void Set(string section, string key, float value)
        {
            MyIni.Set(section, key, value);
        }

        public Color GetColor(string section, string key, string default_value = null)
        {
            if (key == null) return Color.Gray;
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

        public static Color ParseColor(string colorValue)
        {
            Color color = Color.Gray;
            // Find matches.
            //program.drawingSurface.WriteText($"{section}/{key}={colorValue}", true);
            if (String.IsNullOrEmpty(colorValue) == false)
            {
                string[] colorSplit = colorValue.Split(',');
                color = new Color(int.Parse(colorSplit[0]), int.Parse(colorSplit[1]), int.Parse(colorSplit[2]), int.Parse(colorSplit[3]));
            }
            return color;
        }
    }
}
