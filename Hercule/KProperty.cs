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

            public string color_default;

            public float elevator_position_max;
            public float elevator_position_1;
            public float elevator_position_2;
            public float elevator_position_3;
            public float elevator_velocity_max;
            public float elevator_velocity_medium;
            public float elevator_velocity_min;

            public float locker_epsilon;
            public float locker_velocity;
            public float locker_position_max;

            public float tool_velocity;
            public float tool_position_max;
            public int tool_count_1;
            public int tool_count_2;
            public int tool_count_3;
            public int tool_count_4;
            public int tool_count_5;
            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                elevator_position_max = MyIni.Get("Elevator", "position_max").ToSingle(7.6f);
                elevator_position_1 = MyIni.Get("Elevator", "position_1").ToSingle(0.5f);
                elevator_position_2 = MyIni.Get("Elevator", "position_2").ToSingle(2.6f);
                elevator_position_3 = MyIni.Get("Elevator", "position_3").ToSingle(5.2f);
                elevator_velocity_max = MyIni.Get("Elevator", "velocity_max").ToSingle(1f);
                elevator_velocity_medium = MyIni.Get("Elevator", "velocity_medium").ToSingle(0.5f);
                elevator_velocity_min = MyIni.Get("Elevator", "velocity_min").ToSingle(0.2f);

                locker_epsilon = MyIni.Get("Locker", "epsilon").ToSingle(0.01f);
                locker_velocity = MyIni.Get("Locker", "velocity").ToSingle(1.2f);
                locker_position_max = MyIni.Get("Locker", "position_max").ToSingle(2.35f);

                tool_position_max = MyIni.Get("Tool", "position_max").ToSingle(10f);
                tool_velocity = MyIni.Get("Tool", "velocity").ToSingle(4f);
                tool_count_1 = MyIni.Get("Tool", "count_1").ToInt32(10);
                tool_count_2 = MyIni.Get("Tool", "count_2").ToInt32(9);
                tool_count_3 = MyIni.Get("Tool", "count_3").ToInt32(8);
                tool_count_4 = MyIni.Get("Tool", "count_4").ToInt32(7);
                tool_count_5 = MyIni.Get("Tool", "count_5").ToInt32(5);
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
                MyIni.Set("Elevator", "position_1", elevator_position_1);
                MyIni.Set("Elevator", "position_2", elevator_position_2);
                MyIni.Set("Elevator", "position_3", elevator_position_3);
                MyIni.Set("Elevator", "velocity_max", elevator_velocity_max);
                MyIni.Set("Elevator", "velocity_medium", elevator_velocity_medium);
                MyIni.Set("Elevator", "velocity_min", elevator_velocity_min);

                MyIni.Set("Locker", "epsilon", locker_epsilon);
                MyIni.Set("Locker", "velocity", locker_velocity);
                MyIni.Set("Locker", "position_max", locker_position_max);

                MyIni.Set("Tool", "position_max", tool_position_max);
                MyIni.Set("Tool", "velocity", tool_velocity);
                MyIni.Set("Tool", "count_1", tool_count_1);
                MyIni.Set("Tool", "count_2", tool_count_2);
                MyIni.Set("Tool", "count_3", tool_count_3);
                MyIni.Set("Tool", "count_4", tool_count_4);
                MyIni.Set("Tool", "count_5", tool_count_5);
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
