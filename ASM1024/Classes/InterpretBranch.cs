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
        public class InterpretBranch
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (BranchWords item in Enum.GetValues(typeof(BranchWords)))
                {
                    words.Add(item.ToString(), InstructionType.Branch);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                BranchWords word;
                Enum.TryParse(instruction.Name, out word);
                var a = instruction.GetArgumentDouble(1);
                var b = 0d;
                var c = 0d;
                int address;
                if (instruction.Args.Length == 5)
                {
                    b = instruction.GetArgumentDouble(2);
                    c = instruction.GetArgumentDouble(3);
                    address = instruction.GetArgumentAdress(4);
                }
                else if (instruction.Args.Length == 4)
                {
                    b = instruction.GetArgumentDouble(2);
                    address = instruction.GetArgumentAdress(3);
                }
                else
                {
                    address = instruction.GetArgumentAdress(3);
                }
                //instruction.Parent.Log($"{instruction.Name} {a} {b} {c}");
                var isRedirect = false;
                var isRelative = false;
                var valid = false;
                switch (word)
                {
                    case BranchWords.bap:
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bapal:
                        isRedirect = true;
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bapz:
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bapzal:
                        isRedirect = true;
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.beq:
                        valid = a == b;
                        break;
                    case BranchWords.beqal:
                        isRedirect = true;
                        valid = a == b;
                        break;
                    case BranchWords.beqz:
                        valid = a == b;
                        break;
                    case BranchWords.beqzal:
                        isRedirect = true;
                        valid = a == b;
                        break;
                    case BranchWords.bge:
                        valid = a >= b;
                        break;
                    case BranchWords.bgeal:
                        isRedirect = true;
                        valid = a >= b;
                        break;
                    case BranchWords.bgez:
                        valid = a >= b;
                        break;
                    case BranchWords.bgezal:
                        isRedirect = true;
                        valid = a >= b;
                        break;
                    case BranchWords.bgt:
                        valid = a > b;
                        break;
                    case BranchWords.bgtal:
                        isRedirect = true;
                        valid = a > b;
                        break;
                    case BranchWords.bgtz:
                        valid = a > b;
                        break;
                    case BranchWords.bgtzal:
                        isRedirect = true;
                        valid = a > b;
                        break;
                    case BranchWords.ble:
                        valid = a <= b;
                        break;
                    case BranchWords.bleal:
                        isRedirect = true;
                        valid = a <= b;
                        break;
                    case BranchWords.blez:
                        valid = a <= b;
                        break;
                    case BranchWords.blezal:
                        isRedirect = true;
                        valid = a <= b;
                        break;
                    case BranchWords.blt:
                        valid = a < b;
                        break;
                    case BranchWords.bltal:
                        isRedirect = true;
                        valid = a < b;
                        break;
                    case BranchWords.bltz:
                        valid = a < b;
                        break;
                    case BranchWords.bltzal:
                        isRedirect = true;
                        valid = a < b;
                        break;
                    case BranchWords.bna:
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bnaal:
                        isRedirect = true;
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bnaz:
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bnazal:
                        isRedirect = true;
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.bne:
                        valid = a != b;
                        break;
                    case BranchWords.bneal:
                        isRedirect = true;
                        valid = a != b;
                        break;
                    case BranchWords.bnez:
                        valid = a != b;
                        break;
                    case BranchWords.bnezal:
                        isRedirect = true;
                        valid = a != b;
                        break;
                    case BranchWords.brap:
                        isRelative = true;
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.brapz:
                        isRelative = true;
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.breq:
                        isRelative = true;
                        valid = a == b;
                        break;
                    case BranchWords.breqz:
                        isRelative = true;
                        valid = a == b;
                        break;
                    case BranchWords.brge:
                        isRelative = true;
                        valid = a >= b;
                        break;
                    case BranchWords.brgez:
                        isRelative = true;
                        valid = a >= b;
                        break;
                    case BranchWords.brgt:
                        isRelative = true;
                        valid = a > b;
                        break;
                    case BranchWords.brgtz:
                        isRelative = true;
                        valid = a > b;
                        break;
                    case BranchWords.brle:
                        isRelative = true;
                        valid = a <= b;
                        break;
                    case BranchWords.brlez:
                        isRelative = true;
                        valid = a <= b;
                        break;
                    case BranchWords.brlt:
                        isRelative = true;
                        valid = a < b;
                        break;
                    case BranchWords.brltz:
                        isRelative = true;
                        valid = a <= b;
                        break;
                    case BranchWords.brna:
                        isRelative = true;
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.brnaz:
                        isRelative = true;
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case BranchWords.brne:
                        isRelative = true;
                        valid = a != b;
                        break;
                    case BranchWords.brnez:
                        isRelative = true;
                        valid = a != b;
                        break;
                }
                if (valid)
                {
                    if (isRelative)
                    {
                        instruction.NextIndex = instruction.Index + address;
                    }
                    else
                    {
                        instruction.NextIndex = address;
                    }
                    if (isRedirect)
                    {
                        instruction.SetVar("ra", instruction.Index + 1);
                    }
                }
                else
                {
                    instruction.NextIndex = instruction.Index + 1;
                }
            }
        }
        
        enum BranchWords
        {
            bap,
            bapal,
            bapz,
            bapzal,
            beq,
            beqal,
            beqz,
            beqzal,
            bge,
            bgeal,
            bgez,
            bgezal,
            bgt,
            bgtal,
            bgtz,
            bgtzal,
            ble,
            bleal,
            blez,
            blezal,
            blt,
            bltal,
            bltz,
            bltzal,
            bna,
            bnaal,
            bnaz,
            bnazal,
            bne,
            bneal,
            bnez,
            bnezal,
            brap,
            brapz,
            breq,
            breqz,
            brge,
            brgez,
            brgt,
            brgtz,
            brle,
            brlez,
            brlt,
            brltz,
            brna,
            brnaz,
            brne,
            brnez
        }
    }
}
