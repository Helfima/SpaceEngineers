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
        public class Instruction
        {
            public Instructions Parent;
            public InstructionType Type;
            public int Index;
            public string[] Args { get; set; }
            public void Execute()
            {

            }

            protected int NextIndex()
            {
                return Index + 1;
            }
            protected void Return()
            {
                Parent.Log($"Return {this.GetType().Name}: {NextIndex()}");

                Parent.Index = NextIndex();
            }
            protected void ParseFieldString(string field, out string value)
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
            protected void ParseFieldFloat(string field, out float value)
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
            protected void ParseFieldBool(string field, out bool value)
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
                return $"{this.GetType().Name}:{Index}";
            }
        }
    }
}
