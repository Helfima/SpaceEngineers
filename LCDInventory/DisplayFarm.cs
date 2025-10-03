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
        public class DisplayFarm
        {
            protected DisplayLcd DisplayLcd;

            private int panel = 0;
            private bool enable = false;

            public bool search = true;

            private string filter = "*";
            private List<FarmPlot> farmPlots = new List<FarmPlot>();
            private BlockSystem<IMySolarFoodGenerator> farmSolars;
            public DisplayFarm(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }
            public bool farm_plot = true;
            public bool farm_solar = true;
            public void Load(MyIni MyIni)
            {
                panel = MyIni.Get("Farm", "panel").ToInt32(0);
                enable = MyIni.Get("Farm", "on").ToBoolean(false);
                filter = MyIni.Get("Farm", "filter").ToString("*");
                farm_plot = MyIni.Get("Farm", "farm_plot").ToBoolean(true);
                farm_solar = MyIni.Get("Farm", "farm_solar").ToBoolean(true);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Farm", "panel", panel);
                MyIni.Set("Farm", "on", enable);
                MyIni.Set("Farm", "filter", filter);
                MyIni.Set("Farm", "farm_plot", farm_plot);
                MyIni.Set("Farm", "farm_solar", farm_solar);
            }
            private void Search()
            {
                BlockFilter<IMyTerminalBlock> plot_filter = BlockFilter<IMyTerminalBlock>.Create(DisplayLcd.Block, filter);
                var blockPlots = BlockSystem<IMyTerminalBlock>.SearchByFilter(DisplayLcd.program, plot_filter);
                foreach(var blockPlot in blockPlots.List)
                {
                    FarmPlot farmPlot = null;
                    if (FarmPlot.TryCreateFarmBlock(blockPlot, out farmPlot)) {
                        farmPlots.Add(farmPlot);
                    };
                }
                
                DisplayLcd.program.Echo($"farmPlots: {farmPlots.Count}");
                

                BlockFilter<IMySolarFoodGenerator> solar_filter = BlockFilter<IMySolarFoodGenerator>.Create(DisplayLcd.Block, filter);
                farmSolars = BlockSystem<IMySolarFoodGenerator>.SearchByFilter(DisplayLcd.program, solar_filter);

                search = false;
            }
            public void Draw(Drawing drawing)
            {
                if (!enable) return;
                var surface = drawing.GetSurfaceDrawing(panel);
                surface.Initialize();
                Draw(surface);
            }
            public void Draw(SurfaceDrawing surface)
            {
                if (!enable) return;
                if (search) Search();

                Style style = new Style()
                {
                    Width = 250,
                    Height = 80,
                    Padding = new StylePadding(0),
                };
                int limit = 6;
                int count = 0;
                farmPlots.ForEach(delegate (FarmPlot block)
                {
                    Vector2 position2 = surface.Position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                    DrawFarmPlot(surface, position2, block, style);
                    count += 1;
                });
            }

            public void DrawFarmPlot(SurfaceDrawing surface, Vector2 position, FarmPlot block, Style style)
            {
                float size_icon = style.Height - 10;
                Color color_title = new Color(100, 100, 100, 128);
                Color color_text = new Color(100, 100, 100, 255);
                float RotationOrScale = 0.5f;
                float cell_spacing = 10f;

                float form_width = style.Width - 5;
                float form_height = style.Height - 5;

                string colorDefault = DisplayLcd.program.MyProperty.Get("color", "default");

                float x = 0f;

                surface.AddForm(position + new Vector2(0, 0), SpriteForm.SquareSimple, form_width, form_height, new Color(5, 5, 5, 125));
                DisplayLcd.program.Echo($"Block {block.FarmBlock.Name}");

                Vector2 positionQuantity = position + new Vector2(x, size_icon - 12);
                surface.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = block.FarmLogic.IsAlive.ToString(),
                    Color = color_text,
                    Position = positionQuantity,
                    RotationOrScale = RotationOrScale,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT
                });
            }
        }
    }
}
