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
        public class DisplayLcd
        {
            public Program program;
            public IMyTextPanel lcd;

            private DisplayInventory DisplayInventory;
            private DisplayDrill DisplayDrill;
            private DisplayMachine DisplayMachine;
            private DisplayPower DisplayPower;
            private DisplayShip DisplayShip;
            private DisplayTank DisplayTank;

            public DisplayLcd(Program program, IMyTextPanel lcd)
            {
                this.program = program;
                this.lcd = lcd;

                this.DisplayInventory = new DisplayInventory(this);
                this.DisplayDrill = new DisplayDrill(this);
                this.DisplayMachine = new DisplayMachine(this);
                this.DisplayPower = new DisplayPower(this);
                this.DisplayShip = new DisplayShip(this);
                this.DisplayTank = new DisplayTank(this);
            }

            public void Load(MyIni MyIni)
            {
                DisplayInventory.Load(MyIni);
                DisplayDrill.Load(MyIni);
                DisplayMachine.Load(MyIni);
                DisplayPower.Load(MyIni);
                DisplayShip.Load(MyIni);
                DisplayTank.Load(MyIni);
                if (lcd.CustomData.Trim().Equals("prepare"))
                {
                    program.drawingSurface.WriteText($"Prepare:{lcd.CustomName}\n", true);
                    DisplayInventory.Save(MyIni);
                    DisplayDrill.Save(MyIni);
                    DisplayMachine.Save(MyIni);
                    DisplayPower.Save(MyIni);
                    DisplayShip.Save(MyIni);
                    DisplayTank.Save(MyIni);
                    lcd.CustomData = MyIni.ToString();
                }
            }

            public void Draw()
            {
                Drawing drawing = new Drawing(lcd);
                lcd.ScriptBackgroundColor = Color.Black;
                Vector2 position = drawing.viewport.Position;

                position = DisplayInventory.Draw(drawing, position);
                position = DisplayDrill.Draw(drawing, position);
                position = DisplayMachine.Draw(drawing, position);
                position = DisplayPower.Draw(drawing, position);
                position = DisplayShip.Draw(drawing, position);
                DisplayTank.Draw(drawing, position);

                drawing.Dispose();
            }
        }
    }
}
