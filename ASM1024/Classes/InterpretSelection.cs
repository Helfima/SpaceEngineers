using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
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
        public class InterpretSelection
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (SelectionWords item in Enum.GetValues(typeof(SelectionWords)))
                {
                    words.Add(item.ToString(), InstructionType.Selection);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                SelectionWords word;
                Enum.TryParse(instruction.Name, out word);
                var r = instruction.GetArgumentString(1);
                var a = instruction.GetArgumentDouble(2);
                var b = instruction.GetArgumentDouble(3);
                var c = instruction.GetArgumentDouble(4);
                double value = 0;
                
                var valid = false;
                switch (word)
                {
                    case SelectionWords.sap:
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case SelectionWords.sapz:
                        valid = Math.Abs(a - b) <= Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case SelectionWords.select:
                        valid = a != 0;
                        break;
                    case SelectionWords.seq:
                        valid = a == b;
                        break;
                    case SelectionWords.seqz:
                        valid = a == b;
                        break;
                    case SelectionWords.sge:
                        valid = a >= b;
                        break;
                    case SelectionWords.sgez:
                        valid = a >= b;
                        break;
                    case SelectionWords.sgt:
                        valid = a > b;
                        break;
                    case SelectionWords.sgtz:
                        valid = a > b;
                        break;
                    case SelectionWords.sle:
                        valid = a <= b;
                        break;
                    case SelectionWords.slez:
                        valid = a <= b;
                        break;
                    case SelectionWords.slt:
                        valid = a < b;
                        break;
                    case SelectionWords.sltz:
                        valid = a < b;
                        break;
                    case SelectionWords.sna:
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case SelectionWords.snaz:
                        valid = Math.Abs(a - b) > Math.Max(c * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8);
                        break;
                    case SelectionWords.sne:
                        valid = a != b;
                        break;
                    case SelectionWords.snez:
                        valid = a != b;
                        break;

                }
                if (word == SelectionWords.select)
                {
                    value = valid ? b : c;
                }
                else
                {
                    value = valid ? 1d : 0d;
                }
                instruction.SetVar(r, value);
            }
        }
        
        enum SelectionWords
        {
            sap,
            sapz,
            select,
            seq,
            seqz,
            sge,
            sgez,
            sgt,
            sgtz,
            sle,
            slez,
            slt,
            sltz,
            sna,
            snaz,
            sne,
            snez
        }
    }
}
