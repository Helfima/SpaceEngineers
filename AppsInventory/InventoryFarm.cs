using System;
using System.Collections.Generic;
using AppsInventory.Common;
using AppsInventory.Extensions;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory
{
    [MyTextSurfaceScript("InventoryFarm", "Inventory Farm")]
    public class InventoryFarm : MyTSSCommon
    {
        public override ScriptUpdate NeedsUpdate { get; } = ScriptUpdate.Update10;

        readonly IMyTerminalBlock TerminalBlock;

        public InventoryFarm(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            TerminalBlock = (IMyTerminalBlock)block;
            TerminalBlock.OnMarkForClose += BlockDeleted;
        }

        public override void Dispose()
        {
            base.Dispose();
            TerminalBlock.OnMarkForClose -= BlockDeleted;
        }

        void BlockDeleted(IMyEntity _)
        {
            Dispose();
        }

        public override void Run()
        {
            base.Run();
            Search();
            Style style = new Style()
            {
                Width = 250,
                Height = 80,
                Padding = new StylePadding(0),
            };
            var SurfaceDrawing = new SurfaceDrawing(Surface);
            using (SurfaceDrawing)
            {
                int limit = 6;
                int count = 0;
                farmPlots.ForEach(delegate (FarmPlot block)
                {
                    Vector2 position2 = SurfaceDrawing.Position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                    DrawFarmPlot(SurfaceDrawing, position2, block, style);
                    count += 1;
                });
            }
        }
        private List<FarmPlot> farmPlots = new List<FarmPlot>();
        private void Search()
        {
            if (TerminalBlock != null)
            {
                BlockFilter<IMyTerminalBlock> plot_filter = BlockFilter<IMyTerminalBlock>.Create(TerminalBlock, "*");
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

                TerminalBlock.ClearDetailedInfo();
                var echo = TerminalBlock.GetDetailedInfo();
                echo.AppendLine();
                echo.AppendLine($"plots: {farmPlots.Count}");
            }
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

            Vector2 positionWater = position + new Vector2(x + size_icon * 1.5f, deltaTitle + 2 * deltaInfo + 3 * style.Padding.Y);
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
