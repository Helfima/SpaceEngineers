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
        public class InterpretProgram
        {
            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (ProgramWords item in Enum.GetValues(typeof(ProgramWords)))
                {
                    words.Add(item.ToString(), InstructionType.Program);
                }
            }
            public static void Interpret(Instruction instruction)
            {
                ProgramWords word;
                Enum.TryParse(instruction.Name, out word);
                switch (word)
                {
                    case ProgramWords.Log:
                        var option = instruction.GetArgumentString(1);
                        switch (option)
                        {
                            case "line":
                                {
                                    var line = instruction.GetArgumentInt(2);
                                    instruction.Parent.LogLine = line;
                                }
                                break;
                        }
                        break;
                }
            }
        }
        
        public enum ProgramWords
        {
            Log
        }
    }
}
