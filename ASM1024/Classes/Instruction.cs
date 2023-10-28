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
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        public class Instruction
        {
            public Instructions Parent { get; set; }
            public string Command { get; set; }
            public string Name { get; set; }
            public InstructionType Type { get; set; }
            public int Index { get; set; }
            public int NextIndex { get; set; }
            public string[] Args { get; set; }

            public float GetArgumentFloat(int index)
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    if (Parent.Vars.ContainsKey(arg))
                    {
                        return (float)Parent.Vars[arg];
                    }
                    else
                    {
                        return float.Parse(arg);
                    }
                }
                return 0f;
            }
            public double GetArgumentDouble(int index)
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    if (Parent.Vars.ContainsKey(arg))
                    {
                        return (double)Parent.Vars[arg];
                    }
                    else
                    {
                        return double.Parse(arg);
                    }
                }
                return 0;
            }
            public int GetArgumentInt(int index)
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    if (Parent.Vars.ContainsKey(arg))
                    {
                        return (int)Parent.Vars[arg];
                    }
                    else
                    {
                        return int.Parse(arg);
                    }
                }
                return 0;
            }
            public string GetArgumentString(int index)
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    return arg;
                }
                return null;
            }
            public int GetArgumentAdress(int index)
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    if (Parent.Labels.ContainsKey(arg))
                    {
                        return Parent.Labels[arg] + 1;
                    }
                    else if (Parent.Vars.ContainsKey(arg))
                    {
                        return (int)Parent.Vars[arg];
                    }
                    else
                    {
                        return int.Parse(arg);
                    }
                }
                return -1;
            }
            public BlockSystem<T> GetArgumentDevice<T>(int index) where T : class
            {
                if (index > 0 && index < Args.Length)
                {
                    var arg = Args[index];
                    if (Parent.Devices.ContainsKey(arg))
                    {
                        return (BlockSystem<T>)Parent.Devices[arg];
                    }
                }
                return null;
            }
            public void SetVar(string name, object value)
            {
                Parent.SetVar(name, value);
            }
            public void SetDevice(string name, object value)
            {
                Parent.SetDevice(name, value);
            }
            public void ParseFieldString(string field, out string value)
            {
                object var;
                if (Parent.Vars.TryGetValue(field, out var))
                {
                    value = var.ToString();
                }
                else
                {
                    value = field;
                }
            }
            public void ParseFieldFloat(string field, out float value)
            {
                if (!float.TryParse(field, out value))
                {
                    object var;
                    if (Parent.Vars.TryGetValue(field, out var))
                    {
                        value = (float)var;
                    }
                }
            }
            public void ParseFieldInt(string field, out int value)
            {
                if (!int.TryParse(field, out value))
                {
                    object var;
                    if (Parent.Vars.TryGetValue(field, out var))
                    {
                        value = (int)var;
                    }
                }
            }
            public void ParseFieldBool(string field, out bool value)
            {
                if (!bool.TryParse(field, out value))
                {
                    object var;
                    if (Parent.Vars.TryGetValue(field, out var))
                    {
                        value = (bool)var;
                    }
                }
            }

            public override string ToString()
            {
                return $"{Name}:{Index}";
            }
        }
    }
}
