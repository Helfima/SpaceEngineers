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
        public class InstructionGet : Instruction
        {
            public const string Name = "Get";

            public override void Execute()
            {
                string varName = this.Args[1];
                string devName = this.Args[2];
                string attribut = this.Args[3];
                List<IMyTerminalBlock> devices = Main.Vars[devName] as List<IMyTerminalBlock>;
                //Main.Log($"Get {varName}:{devices.Count}:{attribut}");
                if (devices != null)
                {
                    List<float> values = new List<float>();
                    foreach (IMyTerminalBlock device in devices)
                    {
                        switch (attribut)
                        {
                            case "Velocity":
                                if (device is IMyPistonBase) values.Add((device as IMyPistonBase).Velocity);
                                if (device is IMyMotorStator) values.Add((device as IMyMotorStator).TargetVelocityRPM);
                                break;
                            case "Position":
                                if (device is IMyPistonBase) values.Add((device as IMyPistonBase).CurrentPosition);
                                if (device is IMyMotorStator) values.Add((device as IMyMotorStator).Angle);
                                break;
                        }
                    }
                    float value = values.Average();
                    Main.SetVar(varName, value);
                    Main.Log($"Get {attribut}:{value}");
                }
                NextIndex();
            }
        }
    }
}
