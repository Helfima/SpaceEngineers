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
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        public class BlockDevice
        {
            private Instruction instruction;
            public BlockDevice(Instruction instruction)
            {
                this.instruction = instruction;
            }

            public void ApplyAction(BlockSystem<IMyTerminalBlock> devices, string name)
            {
                if (devices.IsEmpty) return;
                foreach (IMyTerminalBlock block in devices.List)
                {
                    switch (name)
                    {
                        case "Lock":
                        case "Unlock":
                            {
                                block.SetValueBool("RotorLock", name == "Lock");
                                block.SetValueBool("HingeLock", name == "Lock");
                            }
                            break;
                        default:
                            block.ApplyAction(name);
                            break;
                    }
                }
            }
            public void SetProperty(BlockSystem<IMyTerminalBlock> devices, string name, double value)
            {
                if (devices.IsEmpty) return;
                foreach (IMyTerminalBlock block in devices.List)
                {
                    switch (name)
                    {
                        case "Lock":
                            {
                                block.SetValueBool("RotorLock", value >= 1);
                                block.SetValueBool("HingeLock", value >= 1);
                            }
                            break;
                        default:
                            var property = block.GetProperty(name);
                            instruction.Parent.Log($"Property: {property.Id} / {property.TypeName} / {value}");
                            switch (property.TypeName)
                            {
                                case "Boolean":
                                    block.SetValueBool(property.Id, value >= 1);
                                    break;
                                case "Single":
                                    block.SetValueFloat(property.Id, (float) value);
                                    break;
                                case "Color":
                                    var r = 1;
                                    var g = 1;
                                    var b = 1;
                                    var color = new Color(r, g, b);
                                    block.SetValueColor(property.Id, color);
                                    break;
                            }
                            break;
                    }
                }
            }
            public double GetProperty(BlockSystem<IMyTerminalBlock> devices, string name, AggregationType aggregation)
            {
                if (devices.IsEmpty) return 0;
                var values = new List<double>();
                foreach (IMyTerminalBlock block in devices.List)
                {
                    switch (name)
                    {
                        case "Angle":
                            {
                                if(block is IMyMotorStator)
                                {
                                    var stator = (IMyMotorStator)block;
                                    var value = (double)stator.Angle;
                                    values.Add(value * 180 / Math.PI);
                                }
                                else
                                {
                                    throw new Exception("Angle not a property");
                                }
                                
                            }
                            break;
                        case "Lock":
                            {
                                var property = block.GetProperty("RotorLock");
                                if(property != null)
                                {
                                    var valueBool = block.GetValueBool("RotorLock");
                                    var value = valueBool ? 1d : 0d;
                                    //instruction.Parent.Log($"RotorLock {valueBool} {value}");
                                    values.Add(value);
                                }
                                else
                                {
                                    var valueBool = block.GetValueBool("HingeLock");
                                    var value = valueBool ? 1d : 0d;
                                    values.Add(value);
                                }
                            }
                            break;
                        default:
                            {
                                var property = block.GetProperty(name);
                                if (property != null)
                                {
                                    switch (property.TypeName)
                                    {
                                        case "Boolean":
                                            {
                                                var valueBool = block.GetValueBool(property.Id);
                                                var value = valueBool ? 1d : 0d;
                                                values.Add(value);
                                            }
                                            break;
                                        case "Single":
                                            {
                                                var valueFloat = block.GetValueFloat(property.Id);
                                                var value = (double)valueFloat;
                                                values.Add(value);
                                            }
                                            break;
                                        case "Color":
                                            {
                                                var valueColor = block.GetValueColor(property.Id);
                                                throw new Exception("Color Not Implemented");
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    throw new Exception("Wrong property name");
                                }
                            }
                            break;
                    }
                }
                switch (aggregation)
                {
                    case AggregationType.Average:
                        return values.Average();
                    case AggregationType.Sum:
                        return values.Sum();
                    case AggregationType.Maximum:
                        return values.Max();
                    case AggregationType.Minimum:
                        return values.Min();
                }
                return 0d;
            }

            public double LoadDevice(BlockSystem<IMyTerminalBlock> devices, string property, AggregationType aggregation)
            {
                //property = StringHelper.FirstCharToUpper(property);
                switch (property)
                {
                    default:
                        return GetProperty(devices, property, aggregation);
                }
            }
            public void StoreDevice(BlockSystem<IMyTerminalBlock> devices, string property, double value)
            {
                //property = StringHelper.FirstCharToUpper(property);
                switch (property)
                {
                    default:
                        SetProperty(devices, property, value);
                        break;
                }
            }
            public void ActionDevice(BlockSystem<IMyTerminalBlock> devices, string property, double value)
            {
                switch (property)
                {
                    case "on":
                        {
                            var action = value >= 1 ? "OnOff_On" : "OnOff_Off";
                            ApplyAction(devices, action);
                        }
                        break;
                    case "lock":
                        {
                            var action = value >= 1 ? "Lock" : "Unlock";
                            ApplyAction(devices, action);
                        }
                        break;
                    default:
                        {
                            var isAction = devices.First.HasAction(property);
                            if (isAction)
                            {
                                if (value >= 1)
                                {
                                    ApplyAction(devices, property);
                                }
                            }
                        }
                        break;
                }
            }

            
        }

        public enum AggregationType
        {
            Average,
            Sum,
            Minimum,
            Maximum
        }
    }
}
