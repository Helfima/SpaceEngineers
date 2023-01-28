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
        public class InstructionPrint : Instruction
        {
            public const string Name = "Print";

            public override void Execute()
            {
                bool append = true;
                string message = "\n";
                if (this.Args.Length > 2)
                {
                    List<string> args = new List<string>();
                    for (int i = 1; i < this.Args.Length; i++)
                    {
                        string value;
                        ParseFieldString(this.Args[i], out value);
                        message += value + " ";
                    }
                    Main.myProgram.drawingSurface.WriteText(string.Format(message, args.ToArray()), append);
                }
                else
                {
                    string value;
                    ParseFieldString(this.Args[1], out value);
                    message += value;
                    Main.myProgram.drawingSurface.WriteText(message, append);
                }
                NextIndex();
            }
        }
    }
}
