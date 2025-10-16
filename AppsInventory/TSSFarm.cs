using System;
using System.Collections.Generic;
using AppsInventory.Common;
using AppsInventory.Extensions;
using AppsInventory.Surfaces;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory
{
    [MyTextSurfaceScript("Helfima_TSSFarm", "Inventory Farm")]
    public class TSSFarm : TSSBase
    {
        private string filter = "*";
        public bool farm_plot = true;
        public bool farm_solar = true;
        public TSSFarm(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.PropertiesSection = "Farm";
        }

        private List<FarmPlot> farmPlots = new List<FarmPlot>();
        protected override void OnSearch()
        {
            if (TerminalBlock != null)
            {
                BlockFilter<IMyTerminalBlock> plot_filter = BlockFilter<IMyTerminalBlock>.Create(TerminalBlock, this.filter);
                List<IMyTerminalBlock> blockPlots = TerminalBlock.SearchBlocks(plot_filter);

                this.farmPlots = new List<FarmPlot>();
                foreach (var blockPlot in blockPlots)
                {
                    FarmPlot farmPlot = null;
                    if (FarmPlot.TryCreateFarmBlock(blockPlot, out farmPlot))
                    {
                        farmPlots.Add(farmPlot);
                    };
                }

                TerminalBlock.GetDetailedInfo().AppendLine($"plots: {farmPlots.Count}");
            }
        }
        protected override void OnDraw(FrameDrawing frame)
        {
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
                Vector2 position2 = frame.Position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                DrawFarmPlot(frame, position2, block, style);
                count += 1;
            });
        }
        public void DrawFarmPlot(FrameDrawing frame, Vector2 position, FarmPlot farmPlot, Style style)
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

            float x = 0f;

            frame.AddForm(position + new Vector2(0, 0), SpriteForm.SquareSimple, form_width, form_height, new Color(5, 5, 5, 125));

            if (farmPlot.FarmLogic.IsPlantPlanted == false) return;

            string name = farmPlot.FarmLogic.OutputItem.SubtypeName;
            string sprite = "";
            if (farmPlot.FarmLogic.IsPlantFullyGrown)
            {
                if (frame.Parent.Sprites_other.ContainsKey(name))
                {
                    sprite = frame.Parent.Sprites_other[name];
                }
            }
            else
            {
                if (frame.Parent.Sprites_seed.ContainsKey(name))
                {
                    sprite = frame.Parent.Sprites_seed[name];
                }
            }

            Color iconColor = Color.Gray;
            if (farmPlot.FarmLogic.IsPlantFullyGrown) iconColor = Color.ForestGreen;
            else if (farmPlot.FarmLogic.IsAlive) iconColor = Color.YellowGreen;
            else iconColor = Color.OrangeRed;
            // icon
            frame.AddSprite(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = sprite,
                Size = new Vector2(size_icon, size_icon),
                Color = iconColor,
                Position = position + new Vector2(x, size_icon / 2 + cell_spacing)

            });

            var infos = farmPlot.FarmLogic.GetDetailedInfoWithoutRequiredInput().Split(':', '\n');
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
            frame.AddSprite(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = growText,
                Color = iconColor,
                Position = positionGrow,
                RotationOrScale = font_size_info,
                FontId = frame.Parent.Font,
                Alignment = TextAlignment.LEFT
            });

            Vector2 positionTime = position + new Vector2(x + size_icon * 1.5f, deltaTitle + deltaInfo + 2 * style.Padding.Y);
            frame.AddSprite(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = growTime,
                Color = color_text,
                Position = positionTime,
                RotationOrScale = font_size_info,
                FontId = frame.Parent.Font,
                Alignment = TextAlignment.LEFT
            });

            Vector2 positionWater = position + new Vector2(x + size_icon * 1.5f, deltaTitle + 2 * deltaInfo + 3 * style.Padding.Y);
            frame.AddSprite(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = waterText,
                Color = Color.CadetBlue,
                Position = positionWater,
                RotationOrScale = font_size_info,
                FontId = frame.Parent.Font,
                Alignment = TextAlignment.LEFT
            });

            if (string.IsNullOrEmpty(name) == false)
            {
                Vector2 positionName = position + new Vector2(style.Padding.X, style.Padding.Y);
                frame.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = name,
                    Color = color_text,
                    Position = positionName,
                    RotationOrScale = RotationOrScale,
                    FontId = frame.Parent.Font,
                    Alignment = TextAlignment.LEFT
                });
            }
        }
        protected override void DataLoad()
        {
            Properties.Load();
            filter = Properties.Get("Farm", "filter", "*");
            farm_plot = Properties.GetBoolean("Farm", "farm_plot", true);
            farm_solar = Properties.GetBoolean("Farm", "farm_solar", true);
        }
        protected override void DataSave()
        {
            Properties.Set("Farm", "filter", filter);
            Properties.Set("Farm", "farm_plot", farm_plot);
            Properties.Set("Farm", "farm_solar", farm_solar);
            Properties.Save();
        }
    }
}
