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
using static System.Collections.Specialized.BitVector32;

namespace IngameScript
{
    partial class Program
    {
        public class InterpretDevice
        {

            public static void AppendWords(Dictionary<string, InstructionType> words)
            {
                foreach (DeviceWords item in Enum.GetValues(typeof(DeviceWords)))
                {
                    words.Add(item.ToString(), InstructionType.Device);
                }
            }

            public static void Interpret(Instruction instruction)
            {
                var blockDevice = new BlockDevice(instruction);
                DeviceWords word;
                Enum.TryParse(instruction.Name, out word);
                switch (word)
                {
                    case DeviceWords.device:
                        {
                            var device = instruction.GetArgumentString(1);
                            var filter = instruction.GetArgumentString(2);
                            var prog = instruction.Parent.myProgram;
                            BlockFilter<IMyTerminalBlock> block_filter = BlockFilter<IMyTerminalBlock>.Create(prog.Me, filter);
                            var items = BlockSystem<IMyTerminalBlock>.SearchByFilter(prog, block_filter);
                            instruction.SetDevice(device, items);
                        }
                        break;
                    case DeviceWords.load:
                        {
                            var r = instruction.GetArgumentString(1);
                            var device = instruction.GetArgumentDevice<IMyTerminalBlock>(2);
                            var a = instruction.GetArgumentString(3);
                            var b = instruction.GetArgumentInt(4);
                            AggregationType aggregation = (AggregationType)b;
                            var result = blockDevice.LoadDevice(device, a, aggregation);
                            instruction.SetVar(r, result);
                            //instruction.Parent.Log($"device: {device.List.Count} name:{a} mode:{b} {r}:{result}");
                        }
                        break;
                    case DeviceWords.store:
                        {
                            var device = instruction.GetArgumentDevice<IMyTerminalBlock>(1);
                            var a = instruction.GetArgumentString(2);
                            var b = instruction.GetArgumentDouble(3);
                            blockDevice.StoreDevice(device, a, b);
                            //instruction.Parent.Log($"device: {device.List.Count} action:{a} value:{b}");
                        }
                        break;
                    case DeviceWords.action:
                        {
                            var device = instruction.GetArgumentDevice<IMyTerminalBlock>(1);
                            var a = instruction.GetArgumentString(2);
                            var b = instruction.GetArgumentDouble(3);
                            blockDevice.ActionDevice(device, a, b);
                            //instruction.Parent.Log($"device: {device.List.Count} action:{a} value:{b}");
                        }
                        break;
                }
            }

            
        }
        
        enum DeviceWords
        {
            device,
            load,
            store,
            action
        }
    }
}
