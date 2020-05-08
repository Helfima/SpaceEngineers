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
        public class ControllerInventory
        {
            Program myProgram;
            public List<IMyTerminalBlock> inventory_list = new List<IMyTerminalBlock>();

            public ControllerInventory(Program program)
            {
                myProgram = program;
            }

            public void Search()
            {
                inventory_list.Clear();
                myProgram.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventory_list, block => block.HasInventory);
                myProgram.Echo("Inventory: " + inventory_list.Count);
            }

            public void Count()
            {
                myProgram.model.ResetAmount();
                Dictionary<string, double> count = new Dictionary<string, double>();
                foreach (IMyTerminalBlock block in inventory_list)
                {

                    for (int i = 0; i < block.InventoryCount; i++)
                    {
                        IMyInventory block_inventory = block.GetInventory(i);
                        List<MyInventoryItem> items = new List<MyInventoryItem>();
                        block_inventory.GetItems(items);
                        foreach (MyInventoryItem block_item in items)
                        {

                            string name = Property.GetName(block_item);
                            string type = Property.GetType(block_item);
                            double amount = 0;
                            Double.TryParse(block_item.Amount.ToString(), out amount);
                            myProgram.model.AddAmount(type, name, amount);
                        }
                    }
                }
            }
        }
    }
}
