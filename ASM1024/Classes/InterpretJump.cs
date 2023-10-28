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
        public class InterpretJump
        {
            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (JumpWords item in Enum.GetValues(typeof(JumpWords)))
                {
                    words.Add(item.ToString(), InstructionType.Jump);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                JumpWords word;
                Enum.TryParse(instruction.Name, out word);
                switch (word)
                {
                    case JumpWords.j:
                    case JumpWords.jal:
                        {
                            var address = instruction.GetArgumentAdress(1);
                            instruction.NextIndex = address;
                            if (word == JumpWords.jal)
                            {
                                instruction.SetVar("ra", instruction.Index + 1);
                            }
                        }
                        break;
                    case JumpWords.jr:
                        {
                            int address = instruction.GetArgumentInt(1);
                            instruction.NextIndex = address;
                        }
                        break;

                }
            }
        }
        enum JumpWords
        {
            j,
            jal,
            jr
        }
    }
}
