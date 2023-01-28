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
        public class InstructionJump : Instruction
        {
            public const string Name = "Jump";

            public override void Execute()
            {
                string label = Args[1] + ":";
                var instruction = Main.Children.Find(x => x.Args[0] == label);
                if(instruction != null)
                {
                    Main.Index = instruction.Index;
                }
                else
                {
                    NextIndex();
                }

            }
        }
    }
}
