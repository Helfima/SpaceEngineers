using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.CodeDom;
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
using static VRage.Game.MyObjectBuilder_CurveDefinition;

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
            public void SetColor(BlockSystem<IMyTerminalBlock> devices, string name, int r, int g, int b, int a)
            {
                if (devices.IsEmpty) return;
                foreach (IMyTerminalBlock block in devices.List)
                {
                    var color = new Color(r, g, b, a);
                    block.SetValueColor(name, color);
                }
            }
            public void ApplyAction(BlockSystem<IMyTerminalBlock> devices, string name)
            {
                if (devices.IsEmpty) return;
                foreach (IMyTerminalBlock block in devices.List)
                {
                    block.ApplyAction(name);
                }
            }
            public void SetProperty(BlockSystem<IMyTerminalBlock> devices, string name, double value)
            {
                if (devices.IsEmpty) return;
                foreach (IMyTerminalBlock block in devices.List)
                {
                    switch (name)
                    {
                        default:
                            var property = block.GetProperty(name);
                            if (property != null)
                            {
                                instruction.Parent.Log($"Property: {property.Id} / {property.TypeName} / {value}");
                                switch (property.TypeName)
                                {
                                    case "Boolean":
                                        block.SetValueBool(property.Id, value >= 1);
                                        break;
                                    case "Single":
                                        block.SetValueFloat(property.Id, (float)value);
                                        break;
                                    case "Color":
                                        var r = 1;
                                        var g = 1;
                                        var b = 1;
                                        var color = new Color(r, g, b);
                                        block.SetValueColor(property.Id, color);
                                        break;
                                }
                            }
                            else
                            {
                                throw new Exception("Wrong property name");
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
                                if (block is IMyMotorStator)
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
                        case "Position":
                            {
                                if (block is IMyPistonBase)
                                {
                                    var piston = (IMyPistonBase)block;
                                    var value = (double)piston.CurrentPosition;
                                    values.Add(value);
                                }
                                else
                                {
                                    throw new Exception("Position not a property");
                                }

                            }
                            break;
                        case "IsConnected":
                            {
                                if (block is IMyShipMergeBlock)
                                {
                                    var piston = (IMyShipMergeBlock)block;
                                    var value = piston.IsConnected ? 1d : 0d;
                                    values.Add(value);
                                }
                                else
                                {
                                    throw new Exception("IsConnected not a property");
                                }
                            }
                            break;
                        case "IsActive":
                            {
                                if (block is IMySensorBlock)
                                {
                                    var piston = (IMySensorBlock)block;
                                    var value = piston.IsActive ? 1d : 0d;
                                    values.Add(value);
                                }
                                else
                                {
                                    throw new Exception("IsActive not a property");
                                }
                            }
                            break;
                        case "LockMode":
                            {
                                if(block is IMyLandingGear)
                                {
                                    var piston = (IMyLandingGear)block;
                                    var value = (double)piston.LockMode;
                                    values.Add(value);
                                }
                                else
                                {
                                    throw new Exception("IsActive not a property");
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
            public double InventoryDevice(BlockSystem<IMyTerminalBlock> devices, int index, string property, AggregationType aggregation)
            {
                if (devices.IsEmpty) return 0;
                var values = new List<double>();
                foreach (IMyTerminalBlock block in devices.List)
                {
                    double count = 0;
                    IMyInventory drill_inventory = block.GetInventory(index);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    drill_inventory.GetItems(items);
                    //instruction.Parent.Log($"MyInventoryItem: {items.Count}");
                    foreach (MyInventoryItem item in items)
                    {
                        switch (property)
                        {
                            case "Amount":
                                double amount = 0;
                                Double.TryParse(item.Amount.ToString(), out amount);
                                count += amount;
                                //instruction.Parent.Log($"Amount: {amount}");
                                break;
                        }
                    }
                    values.Add(count);
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
                switch (property)
                {
                    default:
                        return GetProperty(devices, property, aggregation);
                }
            }
            public void StoreDevice(BlockSystem<IMyTerminalBlock> devices, string property, double value)
            {
                switch (property)
                {
                    default:
                        SetProperty(devices, property, value);
                        break;
                }
            }
            public void ActionDevice(BlockSystem<IMyTerminalBlock> devices, string property)
            {
                switch (property)
                {
                    default:
                        {
                            var isAction = devices.First.HasAction(property);
                            if (isAction)
                            {
                                ApplyAction(devices, property);
                            }
                            else
                            {
                                throw new Exception("Wrong action name");
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
