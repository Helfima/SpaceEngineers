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
        public class InterpretStack
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (StackWords item in Enum.GetValues(typeof(StackWords)))
                {
                    words.Add(item.ToString(), InstructionType.Stack);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                StackWords word;
                Enum.TryParse(instruction.Name, out word);
                switch (word)
                {
                    case StackWords.peek:
                        break;
                    case StackWords.pop:
                        break;
                    case StackWords.push:
                        break;

                }
            }
        }
        
        enum StackWords
        {
            peek,
            pop,
            push
        }
    }
}
