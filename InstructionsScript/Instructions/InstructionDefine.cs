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
        public class InstructionDefine : Instruction
        {
            public const string Name = "Define";

            public override void Execute()
            {
                // var <var name> <float value>
                float value = 0f;
                string varName = this.Args[1];
                ParseFieldFloat(this.Args[2], out value);
                Main.SetVar(varName, value);
                Main.Log($"Try Define {varName}={value}");
                NextIndex();
            }
        }
    }
}
