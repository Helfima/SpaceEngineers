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
        public class InterpretMath
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (MathWords item in Enum.GetValues(typeof(MathWords)))
                {
                    words.Add(item.ToString(), InstructionType.Math);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                MathWords word;
                Enum.TryParse(instruction.Name, out word);
                var r = instruction.GetArgumentString(1);
                var a = instruction.GetArgumentDouble(2);
                var b = instruction.GetArgumentDouble(3);
                double value = 0;
                switch (word)
                {
                    case MathWords.abs:
                        // abs r? a(r?|num)
                        value = Math.Abs(a);
                        break;
                    case MathWords.acos:
                        // acos r? a(r?|num)
                        value = Math.Acos(a);
                        break;
                    case MathWords.add:
                        // add r? a(r?|num) b(r?|num)
                        value = a + b;
                        break;
                    case MathWords.asin:
                        // asin r? a(r?|num)
                        value = Math.Asin(a);
                        break;
                    case MathWords.atan:
                        // atan r? a(r?|num)
                        value = Math.Atan(a);
                        break;
                    case MathWords.ceil:
                        // ceil r? a(r?|num)
                        value = Math.Ceiling(a);
                        break;
                    case MathWords.cos:
                        // cos r? a(r?|num)
                        value = Math.Cos(a);
                        break;
                    case MathWords.div:
                        // div r? a(r?|num) b(r?|num)
                        value = a / b;
                        break;
                    case MathWords.exp:
                        // exp r? a(r?|num)
                        value = Math.Exp(a);
                        break;
                    case MathWords.floor:
                        // floor r? a(r?|num)
                        value = Math.Floor(a);
                        break;
                    case MathWords.log:
                        // log r? a(r?|num)
                        value = Math.Log(a);
                        break;
                    case MathWords.max:
                        // max r? a(r?|num) b(r?|num)
                        value = Math.Max(a, b);
                        break;
                    case MathWords.min:
                        // min r? a(r?|num) b(r?|num)
                        value = Math.Min(a, b);
                        break;
                    case MathWords.mod:
                        // mod r? a(r?|num) b(r?|num)
                        value = a % b;
                        break;
                    case MathWords.mul:
                        // mul r? a(r?|num) b(r?|num)
                        value = a * b;
                        break;
                    case MathWords.rand:
                        // rand r?
                        var rand = new Random();
                        value = rand.NextDouble();
                        break;
                    case MathWords.round:
                        // round r? a(r?|num)
                        value = Math.Round(a);
                        break;
                    case MathWords.sin:
                        // sin r? a(r?|num)
                        value = Math.Sin(a);
                        break;
                    case MathWords.sqrt:
                        // sqrt r? a(r?|num)
                        value = Math.Sqrt(a);
                        break;
                    case MathWords.sub:
                        // sub r? a(r?|num) b(r?|num)
                        value = a - b;
                        break;
                    case MathWords.tan:
                        // tan r? a(r?|num)
                        value = Math.Tan(a);
                        break;
                    case MathWords.trunc:
                        // trunc r? a(r?|num)
                        value = Math.Truncate(a);
                        break;

                }
                instruction.SetVar(r, value);
            }
        }
        
        enum MathWords
        {
            abs,
            acos,
            add,
            asin,
            atan,
            ceil,
            cos,
            div,
            exp,
            floor,
            log,
            max,
            min,
            mod,
            mul,
            rand,
            round,
            sin,
            sqrt,
            sub,
            tan,
            trunc
        }
    }
}
