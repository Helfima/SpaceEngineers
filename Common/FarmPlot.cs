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
}
