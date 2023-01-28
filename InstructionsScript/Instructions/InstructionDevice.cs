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
        public class InstructionDevice : Instruction
        {
            public const string Name = "Device";

            public override void Execute()
            {
                List<IMyTerminalBlock> devices = new List<IMyTerminalBlock>();

                string varName = this.Args[1]; 
                string filter = Args[2];
                string name = Args[3];
                switch (filter)
                {
                    case "ByGroup":
                        IMyBlockGroup group =  Main.myProgram.GridTerminalSystem.GetBlockGroupWithName(name);
                        //Main.Log($"Group: {group?.Name}");
                        group.GetBlocksOfType(devices);
                        //Main.Log($"Device: {devices?.Count}");
                        break;
                    case "ByName":
                    default:
                        Main.myProgram.GridTerminalSystem.SearchBlocksOfName(name, devices);
                        break;
                }
                Main.SetVar(varName, devices);
                NextIndex();
            }
        }
    }
}
