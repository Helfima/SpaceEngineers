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
        public class InstructionAction : Instruction
        {
            public const string Name = "Action";

            public override void Execute()
            {
                string varName = this.Args[1];
                string action = this.Args[2];
                object value = Main.Vars[varName];
                if (action == "On") action = "OnOff_On";
                if (action == "Off") action = "OnOff_Off";
                if (value is List<IMyTerminalBlock>){
                    List<IMyTerminalBlock> devices = value as List<IMyTerminalBlock>;
                    foreach(IMyTerminalBlock device in devices)
                    {
                        device.ApplyAction(action);
                    }
                }
                NextIndex();
            }
        }
    }
}
