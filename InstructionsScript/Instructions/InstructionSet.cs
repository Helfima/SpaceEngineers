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
        public class InstructionSet : Instruction
        {
            public const string Name = "Set";

            public override void Execute()
            {
                string devName = this.Args[1];
                string attribut = this.Args[2];
                float value = 0;
                ParseFieldFloat(this.Args[3], out value);
                List<IMyTerminalBlock> devices = Main.Vars[devName] as List<IMyTerminalBlock>;
                if(devices != null)
                {
                    foreach(IMyTerminalBlock device in devices)
                    {
                        switch (attribut)
                        {
                            case "Velocity":
                                if (device is IMyPistonBase) (device as IMyPistonBase).Velocity = value;
                                if (device is IMyMotorStator) (device as IMyMotorStator).TargetVelocityRPM = value;
                                break;
                        }
                    }
                }
                NextIndex();
            }
        }
    }
}
