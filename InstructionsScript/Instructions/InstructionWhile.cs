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
        public class InstructionWhile : Instruction
        {
            public const string Name = "While";

            public override void Execute()
            {
                float value1 = 0;
                ParseFieldFloat(this.Args[1], out value1);
                string condition = this.Args[2];
                float value2 = 0;
                ParseFieldFloat(this.Args[3], out value2);
                Main.Log($"!While {value1} {condition} {value2}");
                switch (condition)
                {
                    case "==":
                        if (value1 == value2) NextIndex();
                        else Return();
                        break;
                    case "!=":
                        if (value1 != value2) NextIndex();
                        else Return();
                        break;
                    case ">":
                        if (value1 > value2) NextIndex();
                        else Return();
                        break;
                    case ">=":
                        if (value1 >= value2) NextIndex();
                        else Return();
                        break;
                    case "<":
                        if (value1 < value2) NextIndex();
                        else Return();
                        break;
                    case "<=":
                        if (value1 <= value2) NextIndex();
                        else Return();
                        break;
                    default:
                        NextIndex();
                        break;
                }
            }
        }

        public class InstructionEndWhile : Instruction
        {
            public const string Name = "EndWhile";

            public override void Execute()
            {
                Return();
            }
        }
    }
}
