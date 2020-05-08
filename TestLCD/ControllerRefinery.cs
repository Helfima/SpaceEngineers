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
        public class ControllerRefinery
        {
            Program myProgram;
            string TAG_SEARCH = "ROT";
            List<IMyRefinery> refinery_list = new List<IMyRefinery>();

            public ControllerRefinery(Program program, string tag)
            {
                myProgram = program;
                TAG_SEARCH = tag;
            }

            public void Search()
            {
                refinery_list.Clear();
                myProgram.GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refinery_list, block => block.CustomName.Contains(TAG_SEARCH));
                myProgram.Echo("Refinery: " + refinery_list.Count);
            }

            public void RefineryInit()
            {
                foreach (IMyRefinery refinery in refinery_list)
                {
                    refinery.UseConveyorSystem = false;
                }
            }

            public void RefineryCleanup(int inventory_index, List<IMyCargoContainer> cargo_list)
            {
                foreach (IMyRefinery refinery in refinery_list)
                {
                    IMyInventory refinery_inventory = refinery.GetInventory(inventory_index);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    refinery_inventory.GetItems(items);
                    MyInventoryItem[] refinery_items = items.ToArray();
                    for (int idx = 0; idx < refinery_items.Length; idx++)
                    {
                        MyInventoryItem refinery_item = refinery_items[idx];
                        foreach (IMyCargoContainer cargo in cargo_list)
                        {
                            IMyInventory cargo_inventory = cargo.GetInventory(0);
                            if (!cargo_inventory.IsFull) refinery_inventory.TransferItemTo(cargo_inventory, idx, null, true, null);
                            if (refinery_item.Amount.ToIntSafe() < 1) break;
                        }
                    }
                }
            }
        }
    }
}
