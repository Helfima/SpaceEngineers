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
        
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        KProperty MyProperty;

        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;

        private BlockSystem<IMyTextPanel> lcds = null;
        
        private bool ForceUpdate = false;
        private bool search = true;

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
            BlockFilter<IMyTextPanel> block_filter = BlockFilter<IMyTextPanel>.Create(Me, MyProperty.lcd_filter);
            lcds = BlockSystem<IMyTextPanel>.SearchByFilter(this, block_filter);
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
                        drawing.Test();
                        drawing.Dispose();
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
                        IMyTerminalBlock block = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName(name);
                        IMyTextPanel lcdResult2 = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("Result Type");
                        lcdResult2.ContentType = ContentType.TEXT_AND_IMAGE;
                        lcdResult2.WriteText($"Block {name}\n", false);
                        lcdResult2.WriteText($"Type Name={block.GetType().Name}\n", true);
                        lcdResult2.WriteText($"SubtypeName={block.BlockDefinition.SubtypeName}\n", true);
                        lcdResult2.WriteText($"SubtypeId={block.BlockDefinition.SubtypeId}\n", true);
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

        private void Display()
        {
            drawingSurface.WriteText($"LCD list size:{lcds.List.Count}\n", false);
        }

        private void RunLcd()
        {
            lcds.List.ForEach(delegate (IMyTextPanel lcd) {
                if (lcd.CustomData != null && !lcd.CustomData.Equals(""))
                {
                    MyIniParseResult result;
                    MyIni MyIni = new MyIni();
                    MyIni.TryParse(lcd.CustomData, out result);
                    if (MyIni.ContainsSection("Inventory") || lcd.CustomData.Trim().Equals("prepare"))
                    {
                        DisplayLcd displayLcd;
                        if (displayLcds.ContainsKey(lcd.EntityId))
                        {
                            displayLcd = displayLcds[lcd.EntityId];
                        }
                        else
                        {
                            displayLcd = new DisplayLcd(this, lcd);
                            displayLcds.Add(lcd.EntityId, displayLcd);
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
