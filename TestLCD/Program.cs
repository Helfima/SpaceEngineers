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
    partial class Program : MyGridProgram
    {
        int card_state = 0;

        string TAG_REFINERY = "ROT";
        string TAG_CARGO_ORE = "ORE";
        string TAG_CARGO_INGOT = "INGOT";

        MyCommandLine commandLine = new MyCommandLine();

        List<MyProductionItem> product_list = new List<MyProductionItem>();

        Lcd lcd_ore;
        public Logger logger;
        ControllerRefinery controller_refinery;
        ControllerCargo controller_cargo_ore;
        ControllerCargo controller_cargo_ingot;
        Model model;

        ControllerInventory inventory_stat;
        Lcd lcd_stat;

        // Script constructor
        public Program()
        {
            logger = new Logger(this, "DEBUG");
            logger.level = 5;

            model = new Model();
            model.AddResourceItem("Cobalt", "Co", 1000000, 10000);
            model.AddResourceItem("Gold", "Au", 1000000, 10000);
            model.AddResourceItem("Iron", "Fe", 1000000, 10000);
            model.AddResourceItem("Magnesium", "Ma", 1000000, 10000);
            model.AddResourceItem("Nickel", "Ni", 1000000, 10000);
            model.AddResourceItem("Platinum", "Pt", 1000000, 10000);
            model.AddResourceItem("Scrap", "Scrap", 1000000, 10000);
            model.AddResourceItem("Silicon", "Si", 1000000, 10000);
            model.AddResourceItem("Silver", "Ag", 1000000, 10000);
            model.AddResourceItem("Stone", "Stone", 1000000, 10000);
            model.AddResourceItem("Uranium", "Ur", 1000000, 10000);

            lcd_ore = new Lcd(this);
            lcd_stat = new Lcd(this);
            controller_refinery = new ControllerRefinery(this, TAG_REFINERY);
            controller_cargo_ore = new ControllerCargo(this, TAG_CARGO_ORE);
            controller_cargo_ingot = new ControllerCargo(this, TAG_CARGO_INGOT);

            // Set the continuous update frequency of this script
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

        }

        // Main Entry Point
        public void Main(string argument, UpdateType updateType)
        {
            CommandParser(argument);
        }

        void CommandParser(string argument)
        {
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "reset":
                        lcd_ore.Search("ORE");
                        controller_refinery.Search();
                        controller_cargo_ore.Search();
                        controller_cargo_ingot.Search();
                        controller_refinery.RefineryInit();
                        RefineryOreCleanup();
                        RefineryIngotCleanup();
                        DisplayOre();
                        break;
                    case "test":
                        Echo("Test");
                        Lcd lcd_test = new Lcd(this);
                        lcd_test.Search("Test");
                        if (lcd_test.IsEmpty)
                        {
                            Echo("Not found lcd Test");
                        } else {
                            Echo("Found lcd Test");
                            lcd_test.GetDrawing().Test();
                            lcd_test.GetDrawing().Dispose();
                        }
                        break;
                    default:
                        //Echo("Command:");
                        //Echo(" reset");
                        break;
                }
            }
        }


        void DisplayOre()
        {
            StringBuilder message = new StringBuilder();
            foreach (KeyValuePair<string, double> item in controller_cargo_ore.OreCount())
            {
                message.Append(String.Format("{0:d}:{1}\n", Property.GetKiloFormat(item.Value), item.Key));
            }
            lcd_ore.Print(message, false);
        }

        void RefineryOreCleanup()
        {
            controller_refinery.RefineryCleanup(0, controller_cargo_ore.cargo_list);
        }

        void RefineryIngotCleanup()
        {
            controller_refinery.RefineryCleanup(1, controller_cargo_ingot.cargo_list);
        }
    }
}
