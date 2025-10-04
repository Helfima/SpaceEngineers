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
using System.Security.Policy;
using static IngameScript.Program;

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

            public void DrawFarmPlot(SurfaceDrawing surface, Vector2 position, FarmPlot farmPlot, Style style)
            {
                float size_icon = style.Height - 10;
                Color color_title = new Color(100, 100, 100, 128);
                Color color_text = new Color(100, 100, 100, 255);
                float RotationOrScale = 0.5f;
                float cell_spacing = 10f;

                float form_width = style.Width - 5;
                float form_height = style.Height - 5;
                float font_size_title = Math.Max(0.3f, (float)Math.Round(style.Height / 4f / 32f, 1));
                float deltaTitle = font_size_title * 20f;

                float font_size_info = Math.Max(0.3f, (float)Math.Round(style.Height / 4f / 32f, 1));
                float deltaInfo = font_size_info * 32f;

                string colorDefault = DisplayLcd.program.MyProperty.Get("color", "default");

                float x = 0f;

                surface.AddForm(position + new Vector2(0, 0), SpriteForm.SquareSimple, form_width, form_height, new Color(5, 5, 5, 125));

                if (farmPlot.FarmLogic.IsPlantPlanted == false) return;

                string name = farmPlot.FarmLogic.OutputItem.SubtypeName;
                string sprite = "";
                if (farmPlot.FarmLogic.IsPlantFullyGrown)
                {
                    if (surface.Sprites_other.ContainsKey(name))
                    {
                        sprite = surface.Sprites_other[name];
                    }
                }
                else
                {
                    if (surface.Sprites_seed.ContainsKey(name))
                    {
                        sprite = surface.Sprites_seed[name];
                    }
                }
                
                Color iconColor = Color.Gray;
                if (farmPlot.FarmLogic.IsPlantFullyGrown) iconColor = Color.ForestGreen;
                else if (farmPlot.FarmLogic.IsAlive) iconColor = Color.YellowGreen;
                else iconColor = Color.OrangeRed;
                // icon
                surface.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = sprite,
                    Size = new Vector2(size_icon, size_icon),
                    Color = iconColor,
                    Position = position + new Vector2(x, size_icon / 2 + cell_spacing)

                });

                var infos = farmPlot.FarmLogic.GetDetailedInfoWithoutRequiredInput().Split(':','\n');
                var growText = infos[3];
                var growTime = String.Format("{0}:{1}:{2}", infos[5], infos[6], infos[7]);
                var waterText = infos[11];
                if (farmPlot.FarmLogic.IsAlive == false)
                {
                    growText = " Dead";
                    growTime = String.Format("{0}:{1}:{2}", infos[3], infos[4], infos[5]);
                    waterText = infos[9];
                }
                // grown
                Vector2 positionGrow = position + new Vector2(x + size_icon * 1.5f, deltaTitle + style.Padding.Y);
                surface.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = growText,
                    Color = iconColor,
                    Position = positionGrow,
                    RotationOrScale = font_size_info,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT
                });

                Vector2 positionTime = position + new Vector2(x + size_icon * 1.5f, deltaTitle + deltaInfo + 2 * style.Padding.Y);
                surface.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = growTime,
                    Color = color_text,
                    Position = positionTime,
                    RotationOrScale = font_size_info,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT
                });

                Vector2 positionWater = position + new Vector2(x + size_icon * 1.5f, deltaTitle +  2 * deltaInfo + 3 * style.Padding.Y);
                surface.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = waterText,
                    Color = Color.CadetBlue,
                    Position = positionWater,
                    RotationOrScale = font_size_info,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT
                });

                if (string.IsNullOrEmpty(name) == false)
                {
                    Vector2 positionName = position + new Vector2(style.Padding.X, style.Padding.Y);
                    surface.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = name,
                        Color = color_text,
                        Position = positionName,
                        RotationOrScale = RotationOrScale,
                        FontId = surface.Font,
                        Alignment = TextAlignment.LEFT
                    });
                }
            }
        }
    }
}
