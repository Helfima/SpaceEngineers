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

            public float elevator_position_max;
            public float elevator_position_min;
            public float elevator_velocity_max;
            public float elevator_velocity_medium;
            public float elevator_velocity_min;

            public float locker_velocity;
            public float locker_position_min_1;
            public float locker_position_max_1;
            public float locker_position_min_2;
            public float locker_position_max_2;

            public float grinder_velocity;
            public float grinder_position_min;
            public float grinder_position_max;

            public float welder_velocity;
            public float welder_position_min;
            public float welder_position_max;

            public float connector_velocity;
            public float connector_position_min;
            public float connector_position_max;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                elevator_position_max = MyIni.Get("Elevator", "position_max").ToSingle(8.6f);
                elevator_position_min = MyIni.Get("Elevator", "position_min").ToSingle(1.1f);
                elevator_velocity_max = MyIni.Get("Elevator", "velocity_max").ToSingle(1f);
                elevator_velocity_medium = MyIni.Get("Elevator", "velocity_medium").ToSingle(0.5f);
                elevator_velocity_min = MyIni.Get("Elevator", "velocity_min").ToSingle(0.1f);

                locker_velocity = MyIni.Get("Locker", "velocity").ToSingle(0.4f);
                locker_position_min_1 = MyIni.Get("Locker", "position_min_1").ToSingle(75f);
                locker_position_max_1 = MyIni.Get("Locker", "position_max_1").ToSingle(89.5f);
                locker_position_min_2 = MyIni.Get("Locker", "position_min_2").ToSingle(270.5f);
                locker_position_max_2 = MyIni.Get("Locker", "position_max_2").ToSingle(285f);

                grinder_velocity = MyIni.Get("Grinder", "velocity").ToSingle(1f);
                grinder_position_min = MyIni.Get("Grinder", "position_min").ToSingle(80f);
                grinder_position_max = MyIni.Get("Grinder", "position_max").ToSingle(95f);

                welder_velocity = MyIni.Get("Welder", "velocity").ToSingle(1f);
                welder_position_min = MyIni.Get("Welder", "position_min").ToSingle(265f);
                welder_position_max = MyIni.Get("Welder", "position_max").ToSingle(280f);

                connector_velocity = MyIni.Get("Connector", "velocity").ToSingle(.8f);
                connector_position_min = MyIni.Get("Connector", "position_min").ToSingle(15f);
                connector_position_max = MyIni.Get("Connector", "position_max").ToSingle(89f);

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

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Elevator", "position_max", elevator_position_max);
                MyIni.Set("Elevator", "position_min", elevator_position_min);
                MyIni.Set("Elevator", "velocity_max", elevator_velocity_max);
                MyIni.Set("Elevator", "velocity_medium", elevator_velocity_medium);
                MyIni.Set("Elevator", "velocity_min", elevator_velocity_min);

                MyIni.Set("Locker", "velocity", locker_velocity);
                MyIni.Set("Locker", "position_min_1", locker_position_min_1);
                MyIni.Set("Locker", "position_max_1", locker_position_max_1);
                MyIni.Set("Locker", "position_min_2", locker_position_min_2);
                MyIni.Set("Locker", "position_max_2", locker_position_max_2);

                MyIni.Set("Grinder", "velocity", grinder_velocity);
                MyIni.Set("Grinder", "position_min", grinder_position_min);
                MyIni.Set("Grinder", "position_max", grinder_position_max);

                MyIni.Set("Welder", "velocity", welder_velocity);
                MyIni.Set("Welder", "position_min", welder_position_min);
                MyIni.Set("Welder", "position_max", welder_position_max);

                MyIni.Set("Connector", "velocity", connector_velocity);
                MyIni.Set("Connector", "position_min", connector_position_min);
                MyIni.Set("Connector", "position_max", connector_position_max);

                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
