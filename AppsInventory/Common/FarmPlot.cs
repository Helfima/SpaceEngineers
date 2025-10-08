using System;
using System.Collections.Generic;
using AppsInventory.Common;
using AppsInventory.Extensions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory.Common
{
    public class FarmPlot
    {
        public readonly IMyTerminalBlock FarmBlock;
        public readonly IMyFarmPlotLogic FarmLogic;
        public readonly IMyResourceStorageComponent WaterTank;
        public readonly MyResourceSinkComponent WaterSink;

        private FarmPlot(IMyTerminalBlock block, IMyFarmPlotLogic farmLogic, IMyResourceStorageComponent waterTank, MyResourceSinkComponent waterSink)
        {
            FarmBlock = block;
            FarmLogic = farmLogic;
            WaterTank = waterTank;
            WaterSink = waterSink;
        }
        public static bool TryCreateFarmBlock(IMyTerminalBlock block, out FarmPlot farmPlot)
        {
            IMyFarmPlotLogic farmLogic;
            IMyResourceStorageComponent waterTank;
            MyResourceSinkComponent waterSink;


            //ToDo: improve to get both water and power sinks
            if (block.Components.TryGet(out farmLogic)
                && block.Components.TryGet(out waterTank)
                && block.Components.TryGet(out waterSink)
                )
            {
                farmPlot = new FarmPlot(block, farmLogic, waterTank, waterSink);
                return true;
            }
            farmPlot = null;
            return false;
        }
    }
}
