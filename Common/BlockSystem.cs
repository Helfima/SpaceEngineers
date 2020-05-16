using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class BlockSystem<T> where T: class
        {
            protected Program program;
            public List<T> List = new List<T>();

            public BlockSystem(){
                List = new List<T>();
            }

            public static BlockSystem<T> SearchBlocks(Program program, Func<T, bool> collect = null, string info = null)
            {
                List<T> list = new List<T>();
                program.GridTerminalSystem.GetBlocksOfType<T>(list, collect);
                if(info == null) program.Echo(String.Format("List <{0}> count: {1}", typeof(T).Name, list.Count));
                else program.Echo(String.Format("List <{0}> count: {1}", info, list.Count));

                return new BlockSystem<T>()
                {
                    program = program,
                    List = list
                };
            }
            public static BlockSystem<T> SearchByTag(Program program, string tag)
            {
                return BlockSystem<T>.SearchBlocks(program, block => ((IMyTerminalBlock)block).CustomName.Contains(tag), tag);
            }
            public static BlockSystem<T> SearchByName(Program program, string name)
            {
                return BlockSystem<T>.SearchBlocks(program, block => ((IMyTerminalBlock)block).CustomName.Equals(name), name);
            }
            public static BlockSystem<T> SearchByGroup(Program program, string name)
            {
                List<T> list = new List<T>();
                IMyBlockGroup group = program.GridTerminalSystem.GetBlockGroupWithName(name);
                group.GetBlocksOfType<T>(list);
                program.Echo(String.Format("List <{0}> count: {1}", name, list.Count));

                return new BlockSystem<T>()
                {
                    program = program,
                    List = list
                };
            }
            public static BlockSystem<T> SearchByGrid(Program program, IMyCubeGrid cubeGrid)
            {
                
                return BlockSystem<T>.SearchBlocks(program, block => ((IMyTerminalBlock)block).CubeGrid == cubeGrid);

            }

            public static BlockSystem<T> SearchByMode(Program program, string mode, string search)
            {
                switch (mode)
                {
                    case "tag":
                        return BlockSystem<T>.SearchByTag(program, search);
                    case "name":
                        return BlockSystem<T>.SearchByName(program, search);
                    case "group":
                        return BlockSystem<T>.SearchByGroup(program, search);
                }
                return null;
            }

            public static void ApplyAction(BlockSystem<T> blockSystem, string action, float value = 0f)
            {
                switch (action)
                {
                    case "on":
                        blockSystem.On();
                        break;
                    case "off":
                        blockSystem.Off();
                        break;
                    case "lock":
                        blockSystem.Lock();
                        break;
                    case "unlock":
                        blockSystem.Unlock();
                        break;
                    case "reverse":
                        blockSystem.ApplyAction("Reverse");
                        break;
                    case "velocity":
                        blockSystem.Velocity(value);
                        break;
                }
            }
            public void ForEach(Action<T> action)
            {
                if (!IsEmpty)
                {
                    List.ForEach(action);
                }
            }

            public object GetProperty(string name)
            {
                if (!IsEmpty)
                {
                    if (List is List<IMyTerminalBlock>)
                    {
                        IMyTerminalBlock block = (IMyTerminalBlock)List[0];
                        ITerminalProperty property = block.GetProperty(name);
                        program.drawingSurface.WriteText($"\nProperty type {name}={property.TypeName}", true);
                        switch (property.TypeName)
                        {
                            case "Single":
                                return block.GetValueFloat(name);
                            case "Boolean":
                                return block.GetValueBool(name);
                            default:
                                return "null";
                        }
                    }
                }
                return null;
            }

            public bool IsPosition(float position, float epsilon = 0.1f)
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            float value = block.CurrentPosition - position;
                            if (Math.Abs(value) > epsilon) isState = false;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            float value = block.Angle - float.Parse(Util.DegToRad(position).ToString());
                            if (Math.Abs(value) > epsilon) isState = false;
                        }
                    }
                }
                return isState;
            }

            public bool IsPositionMax(float epsilon = 0.1f)
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            float value = block.CurrentPosition - block.MaxLimit;
                            if (Math.Abs(value) > epsilon) isState = false;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            float value = block.Angle - block.UpperLimitRad;
                            if (Math.Abs(value) > epsilon/100) isState = false;
                        }
                    }
                }
                return isState;
            }

            public bool IsPositionMin(float epsilon = 0.1f)
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            float value = block.CurrentPosition - block.MinLimit;
                            if (Math.Abs(value) > epsilon) isState = false;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            float value = block.Angle - block.LowerLimitRad;
                            if (Math.Abs(value) > epsilon / 100) isState = false;
                        }
                    }
                }
                return isState;
            }

            public void Velocity(float velocity)
            {
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            block.Velocity = velocity;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            block.TargetVelocityRPM = velocity;
                        }
                    }
                }
            }

            public void ApplyAction(string action)
            {
                if (!IsEmpty)
                {
                    foreach (IMyTerminalBlock block in List)
                    {
                        block.ApplyAction(action);
                    }
                }
            }
            public void On()
            {
                ApplyAction("OnOff_On");
            }
            public void Off()
            {
                ApplyAction("OnOff_Off");
            }

            public void Lock()
            {
                if (!IsEmpty)
                {
                    foreach (IMyMotorStator block in List)
                    {
                        block.RotorLock = true;
                    }
                }
            }
            public void Unlock()
            {
                if (!IsEmpty)
                {
                    foreach (IMyMotorStator block in List)
                    {
                        block.RotorLock = false;
                    }
                }
            }
            public void Merge(BlockSystem<T> blockSystem)
            {
                List.AddList(blockSystem.List);
            }

            public bool IsEmpty
            {
                get
                {
                    if (List != null && List.Count > 0)
                    {
                        return false;
                    }
                    return true;
                }
            }

            public T First
            {
                get
                {
                    if (!IsEmpty)
                    {
                        return List.First();
                    }
                    return null;
                }
            }
        }
    }
}
