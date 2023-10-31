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
            public IMyTerminalBlock Block { get; private set; }

            private DisplayInventory DisplayInventory;
            private DisplayDrill DisplayDrill;
            private DisplayMachine DisplayMachine;
            private DisplayPower DisplayPower;
            private DisplayShip DisplayShip;
            private DisplayTank DisplayTank;
            private int cleanup;

            public DisplayLcd(Program program, IMyTerminalBlock block)
            {
                this.program = program;
                this.Block = block;

                this.DisplayInventory = new DisplayInventory(this);
                this.DisplayDrill = new DisplayDrill(this);
                this.DisplayMachine = new DisplayMachine(this);
                this.DisplayPower = new DisplayPower(this);
                this.DisplayShip = new DisplayShip(this);
                this.DisplayTank = new DisplayTank(this);
                this.cleanup = 0;
            }

            public void Load(MyIni MyIni)
            {
                DisplayInventory.Load(MyIni);
                DisplayDrill.Load(MyIni);
                DisplayMachine.Load(MyIni);
                DisplayPower.Load(MyIni);
                DisplayShip.Load(MyIni);
                DisplayTank.Load(MyIni);
                if (Block.CustomData.Trim().Equals("prepare") || program.ForceUpdate)
                {
                    program.drawingSurface.WriteText($"Prepare:{Block.CustomName}\n", true);
                    DisplayInventory.Save(MyIni);
                    DisplayDrill.Save(MyIni);
                    DisplayMachine.Save(MyIni);
                    DisplayPower.Save(MyIni);
                    DisplayShip.Save(MyIni);
                    DisplayTank.Save(MyIni);
                    Block.CustomData = MyIni.ToString();
                }
            }
            public void Draw()
            {
                cleanup++;
                Drawing drawing = new Drawing(Block);
                TestViewport(drawing);

                if (cleanup < 100)
                {
                    DisplayInventory.Draw(drawing);
                    DisplayDrill.Draw(drawing);
                    DisplayMachine.Draw(drawing);
                    DisplayPower.Draw(drawing);
                    DisplayShip.Draw(drawing);
                    DisplayTank.Draw(drawing);
                }
                else
                {
                    drawing.Clean();
                    cleanup = 0;
                }

                drawing.Dispose();
            }
            private void TestViewport(Drawing drawing)
            {
                var surface = drawing.GetSurfaceDrawing(3);
                if (surface == null) return;
                surface.Initialize();
                program.Echo($"Position {surface.Position}");
                program.Echo($"TextureSize {surface.Surface.TextureSize}");
                program.Echo($"SurfaceSize {surface.Surface.SurfaceSize}");
                program.Echo($"Viewport {surface.Viewport}");
                program.Echo($"Viewport.Position {surface.Viewport.Position}");
            }
        }
    }
}
