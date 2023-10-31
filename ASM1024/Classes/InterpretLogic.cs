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
        public class InterpretLogic
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (LogicWords item in Enum.GetValues(typeof(LogicWords)))
                {
                    words.Add(item.ToString(), InstructionType.Logic);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                LogicWords word;
                Enum.TryParse(instruction.Name, out word);
                var r = instruction.GetArgumentString(1);
                var a = instruction.GetArgumentDouble(2) >= 1;
                var b = instruction.GetArgumentDouble(3) >= 1;
                bool value = false;
                switch (word)
                {
                    case LogicWords.and:
                        // and r? a(r ?| num) b(r ?| num)
                        value = a & b;
                        break;
                    case LogicWords.nor:
                        // non et
                        // nor r? a(r ?| num) b(r ?| num)
                        value = !a & !b;
                        break;
                    case LogicWords.or:
                        // or r? a(r ?| num) b(r ?| num)
                        value = a | b;
                        break;
                    case LogicWords.xor:
                        // ou eclusif
                        // xor r? a(r ?| num) b(r ?| num)
                        value = a ^ b;
                        break;
                    case LogicWords.not:
                        // negation
                        // not r? a(r ?| num)
                        value = !a;
                        break;

                }
                if (value)
                {
                    instruction.SetVar(r, 1d);
                }
                else
                {
                    instruction.SetVar(r, 0d);
                }
                
            }
        }
        
        enum LogicWords
        {
           and,
           nor,
           or,
           xor,
           not
        }
}
}
