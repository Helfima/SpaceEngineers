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

namespace AppsInventory.Common
{
    public class BlockFilter<T> where T : class
    {
        public string Value;
        public string Filter;
        public VRage.Game.ModAPI.Ingame.IMyCubeGrid CubeGrid;
        public bool ByContains = false;
        public bool ByGroup = false;
        public bool MultiGrid = false;
        public bool HasInventory = false;

        public static BlockFilter<T> Create(Sandbox.ModAPI.Ingame.IMyTerminalBlock parent, string filter)
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
                if (!filter.Equals("*")) blockFilter.Filter = filter;
            }
            return blockFilter;
        }
        public Func<T, bool> BlockVisitor()
        {
            return delegate (T block) {
                Sandbox.ModAPI.Ingame.IMyTerminalBlock tBlock = (Sandbox.ModAPI.Ingame.IMyTerminalBlock)block;
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

        public Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, bool> GroupVisitor()
        {
            return delegate (Sandbox.ModAPI.Ingame.IMyBlockGroup group) {
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
