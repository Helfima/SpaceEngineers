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
using Sandbox.Game.Entities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        KProperty MyProperty;

        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;

        private BlockSystem<IMyTerminalBlock> blocks = null;

        private bool ForceUpdate = false;
        private bool search = true;
        private string version = "1.1";
        private Dictionary<long, DisplayLcd> displayLcds = new Dictionary<long, DisplayLcd>();

        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Search();
        }

        private void Init()
        {
        }

        private void Search()
        {
            Echo($"Version {version}");
            blocks = new BlockSystem<IMyTerminalBlock>();
            BlockFilter<IMyTextPanel> block_filter = BlockFilter<IMyTextPanel>.Create(Me, MyProperty.lcd_filter);
            BlockSystem<IMyTextPanel> lcds = BlockSystem<IMyTextPanel>.SearchByFilter(this, block_filter);

            if (lcds.IsEmpty == false)
            {
                blocks.List.AddRange(lcds.List.Cast<IMyTerminalBlock>().ToList());
            }

            BlockFilter<IMyCockpit> cockpit_filter = BlockFilter<IMyCockpit>.Create(Me, MyProperty.lcd_filter);
            BlockSystem<IMyCockpit> cockpits = BlockSystem<IMyCockpit>.SearchByFilter(this, cockpit_filter);

            if (cockpits.IsEmpty == false)
            {
                blocks.List.AddRange(cockpits.List.Cast<IMyTerminalBlock>().ToList());
            }

            search = false;
        }

        public void Save()
        {
            MyProperty.Save();
        }

        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & CommandUpdate) != 0)
            {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update100) != 0)
            {
                RunContinuousLogic();
            }
        }

        private void RunCommand(string argument)
        {
            MyProperty.Load();
            Init();
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);
                if (command != null) command = command.Trim().ToLower();
                switch (command)
                {
                    case "default":
                        Me.CustomData = "";
                        MyProperty.Load();
                        MyProperty.Save();
                        break;
                    case "forceupdate":
                        ForceUpdate = true;
                        break;
                    case "test":
                        IMyTextPanel lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(commandLine.Argument(1));
                        lcd.ScriptBackgroundColor = Color.Black;
                        Drawing drawing = new Drawing(lcd);
                        var surfaceDrawing = drawing.GetSurfaceDrawing();
                        surfaceDrawing.Test(this);
                        surfaceDrawing.Dispose();
                        break;
                    case "getname":
                        int index = 0;
                        int.TryParse(commandLine.Argument(1), out index);
                        var names = new List<string>();
                        drawingSurface.GetSprites(names);
                        Echo($"Sprite {index} name={names[index]}");
                        IMyTextPanel lcdResult = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("Result Name");
                        lcdResult.ContentType = ContentType.TEXT_AND_IMAGE;
                        lcdResult.WriteText($"Sprite {index}\n", false);
                        lcdResult.WriteText($"name={names[index]}", true);
                        break;
                    case "gettype":
                        int.TryParse(commandLine.Argument(1), out index);
                        string name = commandLine.Argument(1);
                        DiplayGetType(name);
                        break;
                    default:
                        search = true;
                        Search();
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            if (search) Search();
            Display();
            RunLcd();
        }
        private void DiplayGetType(string name)
        {
            IMyTerminalBlock block = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName(name);
            IMyTextPanel lcdResult2 = GridTerminalSystem.GetBlockWithName("Result Type") as IMyTextPanel;
            if(lcdResult2 != null)
            {
                lcdResult2.ContentType = ContentType.TEXT_AND_IMAGE;
                lcdResult2.WriteText($"Block {name}\n", false);
                lcdResult2.WriteText($"Type Name={block.GetType().Name}\n", true);
                lcdResult2.WriteText($"SubtypeName={block.BlockDefinition.SubtypeName}\n", true);
                lcdResult2.WriteText($"SubtypeId={block.BlockDefinition.SubtypeId}\n", true);
            }
            else
            {
                Echo($"Block {name}");
                Echo($"Type Name={block.GetType().Name}");
                Echo($"SubtypeName={block.BlockDefinition.SubtypeName}");
                Echo($"SubtypeId={block.BlockDefinition.SubtypeId}");
            }
        }
        private void Display()
        {
            drawingSurface.WriteText($"*** LCDInventory {version} ***\n", false);
            drawingSurface.WriteText($"------------------------------\n", true);
            drawingSurface.WriteText($"LCD list size:{blocks.List.Count}\n", true);
        }

        private void RunLcd()
        {
            blocks.List.ForEach(delegate (IMyTerminalBlock block) {
                if (block.CustomData != null && !block.CustomData.Equals(""))
                {
                    MyIniParseResult result;
                    MyIni MyIni = new MyIni();
                    MyIni.TryParse(block.CustomData, out result);
                    if (MyIni.ContainsSection("Inventory") || block.CustomData.Trim().Equals("prepare"))
                    {
                        DisplayLcd displayLcd;
                        if (displayLcds.ContainsKey(block.EntityId))
                        {
                            displayLcd = displayLcds[block.EntityId];
                        }
                        else
                        {
                            displayLcd = new DisplayLcd(this, block);
                            displayLcds.Add(block.EntityId, displayLcd);
                        }
                        displayLcd.Load(MyIni);
                        displayLcd.Draw();
                    }
                }
            });
            ForceUpdate = false;
        }

    }
}
