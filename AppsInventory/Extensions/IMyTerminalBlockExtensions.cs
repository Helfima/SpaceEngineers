using System;
using System.Collections.Generic;
using AppsInventory.Common;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;


namespace AppsInventory.Extensions
{
    public static class IMyTerminalBlockExtensions
    {
        public static List<T> SearchBlocks<T>(this Sandbox.ModAPI.IMyTerminalBlock TerminalBlock, BlockFilter<T> blockFilter) where T : class
        {
            List<T> list = new List<T>();
            Sandbox.ModAPI.IMyGridTerminalSystem gridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TerminalBlock.CubeGrid);
            try
            {
                if (blockFilter.ByGroup)
                {
                    List<Sandbox.ModAPI.Ingame.IMyBlockGroup> groups = new List<Sandbox.ModAPI.Ingame.IMyBlockGroup>();
                    gridTerminalSystem.GetBlockGroups(groups, blockFilter.GroupVisitor());
                    List<T> group_list = new List<T>();
                    groups.ForEach(delegate (Sandbox.ModAPI.Ingame.IMyBlockGroup group)
                    {
                        group_list.Clear();
                        group.GetBlocksOfType<T>(list, blockFilter.BlockVisitor());
                        list.AddList(group_list);
                    });
                }
                else
                {
                    gridTerminalSystem.GetBlocksOfType<T>(list, blockFilter.BlockVisitor());
                }
            }
            catch { }
            return list;
        }
        public static List<T> SearchBlocks<T>(this Sandbox.ModAPI.IMyTerminalBlock TerminalBlock) where T : class
        {
            List<T> list = new List<T>();
            Sandbox.ModAPI.IMyGridTerminalSystem gridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TerminalBlock.CubeGrid);
            try
            {
                gridTerminalSystem.GetBlocksOfType<T>(list);
            }
            catch { }
            return list;
        }
    }
}
