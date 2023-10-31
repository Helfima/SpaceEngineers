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

            public BlockSystem()
            {
                List = new List<T>();
            }
            public BlockSystem(Program program)
            {
                this.program = program;
                this.List = new List<T>();
            }

            public static BlockSystem<T> SearchBlocks(Program program, Func<T, bool> collect = null, string info = null)
            {
                List<T> list = new List<T>();
                try
                {
                    program.GridTerminalSystem.GetBlocksOfType<T>(list, collect);
                }
                catch { }
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
            public static List<IMyBlockGroup> SearchGroups(Program program, Func<IMyBlockGroup, bool> collect = null)
            {
                List<IMyBlockGroup> list = new List<IMyBlockGroup>();
                try
                {
                    program.GridTerminalSystem.GetBlockGroups(list, collect);
                }
                catch { }
                program.Echo(String.Format("List <IMyBlockGroup> count: {0}", list.Count));

                return list;
            }
            public static BlockSystem<T> SearchByGroup(Program program, string name)
            {
                List<T> list = new List<T>();
                IMyBlockGroup group = null;
                try
                {
                    group = program.GridTerminalSystem.GetBlockGroupWithName(name);
                }
                catch { }
                if (group != null) group.GetBlocksOfType<T>(list);
                program.Echo(String.Format("List <{0}> count: {1}", name, list.Count));

                return new BlockSystem<T>()
                {
                    program = program,
                    List = list
                };
            }
            public static BlockSystem<T> SearchByGrid(Program program, IMyCubeGrid cubeGrid)
            {
                return BlockSystem<T>.SearchBlocks(program, block => ((IMyCubeBlock)block).CubeGrid == cubeGrid);
            }

            public static BlockSystem<T> SearchByFilter(Program program, BlockFilter<T> filter)
            {
                List<T> list = new List<T>();
                try
                {
                    if (filter.ByGroup)
                    {
                        List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
                        program.GridTerminalSystem.GetBlockGroups(groups, filter.GroupVisitor());
                        List<T> group_list = new List<T>();
                        groups.ForEach(delegate (IMyBlockGroup group)
                        {
                            group_list.Clear();
                            group.GetBlocksOfType<T>(list, filter.BlockVisitor());
                            list.AddList(group_list);
                        });
                    }
                    else
                    {
                        program.GridTerminalSystem.GetBlocksOfType<T>(list, filter.BlockVisitor());
                    }
                }
                catch { }
                program.Echo(String.Format("List<{0}>({1}):{2}", typeof(T).Name, filter.Value, list.Count));
                return new BlockSystem<T>()
                {
                    program = program,
                    List = list
                };
            }

            public static List<IMyBlockGroup> SearchGroupFilter(Program program, BlockFilter<T> filter)
            {
                List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
                try
                {
                    if (filter.ByGroup)
                    {
                        program.GridTerminalSystem.GetBlockGroups(groups, filter.GroupVisitor());
                    }
                }
                catch { }
                program.Echo(String.Format("List <{0}> count: {1}", filter.Value, groups.Count));
                return groups;
            }

            public void ForEach(Action<T> action)
            {
                if (!IsEmpty)
                {
                    List.ForEach(action);
                }
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

            public bool IsMorePosition(float position)
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            if (block.CurrentPosition < position) isState = false;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            if (block.Angle < float.Parse(Util.DegToRad(position).ToString())) isState = false;
                        }
                    }
                }
                return isState;
            }

            public bool IsLessPosition(float position)
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    if (List is List<IMyPistonBase>)
                    {
                        foreach (IMyPistonBase block in List)
                        {
                            if (block.CurrentPosition > position) isState = false;
                        }
                    }
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            if (block.Angle > float.Parse(Util.DegToRad(position).ToString())) isState = false;
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
            public bool IsOn()
            {
                bool isState = true;
                if (!IsEmpty)
                {
                    foreach (IMyTerminalBlock block in List)
                    {
                        if (!block.GetValueBool("OnOff")) isState = false;
                    }
                }
                return isState;
            }
            public void Off()
            {
                ApplyAction("OnOff_Off");
            }

            public bool IsOff()
            {
                return !IsOn();
            }

            public void Lock()
            {
                if (!IsEmpty)
                {
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            block.RotorLock = true;
                        }
                    }
                    else
                    {
                        ApplyAction("Lock");
                    }
                }
            }
            public void Unlock()
            {
                if (!IsEmpty)
                {
                    if (List is List<IMyMotorStator>)
                    {
                        foreach (IMyMotorStator block in List)
                        {
                            block.RotorLock = false;
                        }
                    }
                    else
                    {
                        ApplyAction("Unlock");
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
        public class BlockFilter<T> where T : class
        {
            public string Value;
            public string Filter;
            public IMyCubeGrid CubeGrid;
            public bool ByContains = false;
            public bool ByGroup = false;
            public bool MultiGrid = false;
            public bool HasInventory = false;

            public static BlockFilter<T> Create(IMyTerminalBlock parent, string filter)
            {
                BlockFilter<T> blockFilter = new BlockFilter<T>
                {
                    Value = filter,
                    CubeGrid = parent.CubeGrid
                };
                if (filter.Contains(":"))
                {
                    string[] values = filter.Split(':');
                    if (values[0].Contains("C")) blockFilter.ByContains = true;
                    if (values[0].Contains("G")) blockFilter.ByGroup = true;
                    if (values[0].Contains("M")) blockFilter.MultiGrid = true;
                    if (!values[1].Equals("*")) blockFilter.Filter = values[1];
                }
                else
                {
                    if(!filter.Equals("*")) blockFilter.Filter = filter;
                }
                return blockFilter;
            }
            public Func<T, bool> BlockVisitor()
            {
                return delegate(T block) {
                    IMyTerminalBlock tBlock = (IMyTerminalBlock)block;
                    bool state = true;
                    if (Filter != null && !ByGroup)
                    {
                        if (ByContains) { if (!tBlock.CustomName.Contains(Filter)) state = false; }
                        else { if (!tBlock.CustomName.Equals(Filter)) state = false; }
                    }
                    if (!MultiGrid) { if (tBlock.CubeGrid != CubeGrid) state = false; }
                    if (HasInventory) { if (!tBlock.HasInventory) state = false; }
                    return state;
                };
            }

            public Func<IMyBlockGroup, bool> GroupVisitor()
            {
                return delegate (IMyBlockGroup group) {
                    bool state = true;
                    if (Filter != null && ByGroup)
                    {
                        if (ByContains) { if (!group.Name.Contains(Filter)) state = false; }
                        else { if (!group.Name.Equals(Filter)) state = false; }
                    }
                    return state;
                };
            }
        }
    }
}
