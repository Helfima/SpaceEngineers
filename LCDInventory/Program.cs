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
        
        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
        }

        private void Init()
        {
            if (MyProperty.multigrid_lcd)
            {
                lcds = BlockSystem<IMyTextPanel>.SearchBlocks(this);
            }
            else
            {
                lcds = BlockSystem<IMyTextPanel>.SearchBlocks(this, block => (block.CubeGrid == Me.CubeGrid));
            }
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

                switch (command)
                {
                    case "default":
                        Me.CustomData = "";
                        MyProperty.Load();
                        MyProperty.Save();
                        break;
                    case "test":
                        IMyTextPanel lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(commandLine.Argument(1));
                        lcd.ScriptBackgroundColor = Color.Black;
                        Drawing drawing = new Drawing(lcd);
                        drawing.Test(drawingSurface);
                        drawing.Dispose();
                        break;
                    case "getname":
                        int index = 0;
                        int.TryParse(commandLine.Argument(1), out index);
                        var names = new List<string>();
                        drawingSurface.GetSprites(names);
                        Echo($"Sprite {index} name={names[index]}");
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
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

                        DisplayLcd displayLcd = new DisplayLcd(this, lcd);
                        displayLcd.Load(MyIni);
                        displayLcd.Draw();
                    }
                }
            });
        }

    }
}
