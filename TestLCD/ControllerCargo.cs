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
        public class ControllerCargo
        {
            Program myProgram;
            string TAG_SEARCH = "ORE";
            public List<IMyCargoContainer> cargo_list = new List<IMyCargoContainer>();

            public ControllerCargo(Program program, string tag)
            {
                myProgram = program;
                TAG_SEARCH = tag;
            }

            public void Search()
            {
                cargo_list.Clear();
                myProgram.GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargo_list, block => block.CustomName.Contains(TAG_SEARCH));
                myProgram.Echo("Cargo: " + cargo_list.Count);
            }

            public Dictionary<string, double> OreCount()
            {
                return Count("Ore");
            }

            public Dictionary<string, double> IngotCount()
            {
                return Count("Ingot");
            }

            public Dictionary<string, double> Count(string end_with)
            {
                Dictionary<string, double> count = new Dictionary<string, double>();
                foreach (IMyCargoContainer cargo in cargo_list)
                {
                    IMyInventory cargo_inventory = cargo.GetInventory(0);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    cargo_inventory.GetItems(items);
                    foreach (MyInventoryItem refinery_item in items)
                    {
                        if (refinery_item.Type.TypeId.ToString().EndsWith(end_with))
                        {
                            string name = Property.GetName(refinery_item);
                            double amount = 0;
                            Double.TryParse(refinery_item.Amount.ToString(), out amount);
                            if (count.ContainsKey(name))
                            {
                                count[name] = count[name] + amount;
                            }
                            else
                            {
                                count.Add(name, amount);
                            }

                        }
                    }
                }
                return count;
            }
        }
    }
}
