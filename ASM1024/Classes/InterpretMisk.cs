﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
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
        public class InterpretMisk
        {
            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (MiskWords item in Enum.GetValues(typeof(MiskWords)))
                {
                    if (item != MiskWords.label)
                    {
                        words.Add(item.ToString(), InstructionType.Misc);
                    }
                }
            }

            public static void Interpret(Instruction instruction)
            {
                MiskWords word;
                Enum.TryParse(instruction.Name, out word);
                var r = instruction.GetArgumentString(1);
                var a = instruction.GetArgumentDouble(2);
                switch (word)
                {
                    case MiskWords.define:
                        {
                            instruction.Parent.SetVar(r, a);
                        }
                        break;
                    case MiskWords.yield:
                        break;
                    case MiskWords.move:
                        {
                            instruction.Parent.SetVar(r, a);
                        }
                        break;
                    case MiskWords.print:
                        {
                            a = instruction.GetArgumentDouble(1);
                            instruction.Parent.Log($"{r}: {a}");
                        }
                        break;
                }
            }
        }

        

        enum MiskWords
        {
            define,
            yield,
            move,
            print,
            label
        }
    }
}
