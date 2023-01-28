﻿using Sandbox.Game.EntityComponents;
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
    partial class Program : MyGridProgram
    {
        private int Timer = 50;

        KProperty MyProperty;

        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;
        private StateMachine machine_state = StateMachine.Stopped;

        private BlockSystem<IMyRefinery> refinery = null;
        private BlockSystem<IMyCargoContainer> stock = null;
        private int loop;
        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            loop = Timer;
            Prepare();
        }

        private void Prepare()
        {
            BlockFilter<IMyRefinery> filterRefinery = new BlockFilter<IMyRefinery>()
            {
                CubeGrid = Me.CubeGrid
            };
            refinery = BlockSystem<IMyRefinery>.SearchByFilter(this, filterRefinery);
            refinery.ForEach(delegate (IMyRefinery block)
            {
                block.UseConveyorSystem = false;
            });
            BlockFilter<IMyCargoContainer> filterContainer = new BlockFilter<IMyCargoContainer>()
            {
                Filter = "Ore",
                CubeGrid = Me.CubeGrid
            };
            stock = BlockSystem<IMyCargoContainer>.SearchByFilter(this, filterContainer);
        }

        public void Save()
        {
            MyProperty.Save();
        }

        public void Main(string argument, UpdateType updateType)
        {
            loop--;
            if ((updateType & CommandUpdate) != 0)
            {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update100) != 0)
            {
                RunContinuousLogic();
                LoopTime();
            }
        }

        private void RunCommand(string argument)
        {
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "reset":
                        Prepare();
                        break;
                    case "rotate":
                        RefineryRotate();
                        break;
                    case "test":
                        RefineryTest();
                        break;
                }
            }
        }
        private void LoopTime()
        {
            if(loop <= 0)
            {
                loop = Timer;
                RefineryRotate();
            }
            
        }
        void RunContinuousLogic()
        {
            Display();
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{machine_state}\n", false);
            drawingSurface.WriteText($"Rotate in:{loop}", true);
        }
        private void RefineryTest()
        {
            foreach (IMyRefinery refinery in refinery.List)
            {
                
                Echo($"{refinery.CustomName}:{refinery.InventoryCount}");
            }
        }
        private void RefineryRotate()
        {
            foreach (IMyRefinery refinery in refinery.List)
            {
                IMyInventory refinery_inventory = refinery.GetInventory(0);
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                refinery_inventory.GetItems(items);
                Echo($"{refinery.CustomName}:{items.Count}");
                if (items.Count > 1)
                {
                    refinery_inventory.TransferItemTo(refinery_inventory, 0, items.Count - 1);
                }
                refinery.UseConveyorSystem = true;
            }
        }
        //public void RefineryCleanup(int inventory_index, List<IMyCargoContainer> cargo_list)
        //{
        //    foreach (IMyRefinery refinery in refinery_list)
        //    {
        //        IMyInventory refinery_inventory = refinery.GetInventory(inventory_index);
        //        List<MyInventoryItem> items = new List<MyInventoryItem>();
        //        refinery_inventory.GetItems(items);
        //        MyInventoryItem[] refinery_items = items.ToArray();
        //        for (int idx = 0; idx < refinery_items.Length; idx++)
        //        {
        //            MyInventoryItem refinery_item = refinery_items[idx];
        //            foreach (IMyCargoContainer cargo in cargo_list)
        //            {
        //                IMyInventory cargo_inventory = cargo.GetInventory(0);
        //                if (!cargo_inventory.IsFull) refinery_inventory.TransferItemTo(cargo_inventory, idx, null, true, null);
        //                if (refinery_item.Amount.ToIntSafe() < 1) break;
        //            }
        //        }
        //    }
        //}
        public enum StateMachine
        {
            Stopped,
            Running,
            Waitting
        }
    }
}
