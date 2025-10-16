using System;
using System.Collections.Generic;
using System.Linq;
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
    [MyTextSurfaceScript("Helfima_TSSDrills", "Inventory Drills")]
    public class TSSDrills : TSSBase
    {
        public TSSDrills(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.PropertiesSection = "Drills";
        }
        private List<IMyShipDrill> drills = null;
        protected override void OnSearch()
        {
            if (TerminalBlock != null)
            {
                this.drills = TerminalBlock.SearchBlocks<IMyShipDrill>();
            }
        }
        protected override void OnDraw(FrameDrawing frame)
        {
            float width = drills_size;
            float padding = drills_padding;
            float x_min = 0f;
            float x_max = 0f;
            float y_min = 0f;
            float y_max = 0f;
            bool first = true;
            Vector2 margin_screen = new Vector2(drills_margin_x, drills_margin_y);
            frame.Position += margin_screen;
            StyleGauge style = new StyleGauge()
            {
                Orientation = SpriteOrientation.Horizontal,
                Fullscreen = false,
                Width = width,
                Height = width,
                Padding = new StylePadding(0),
                Round = false,
                RotationOrScale = 0.5f,
                Percent = drills_size > 49 ? true : false,
                Thresholds = this.ChestThresholds
            };

            if (drills_info)
            {
                frame.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Drill Number:{drills.Count} ({filter})",
                    Size = new Vector2(width, width),
                    Color = Color.DimGray,
                    Position = frame.Position + new Vector2(0, 0),
                    RotationOrScale = 0.5f,
                    FontId = frame.Parent.Font,
                    Alignment = TextAlignment.LEFT

                });
                frame.Position += new Vector2(0, 20);
            }
            drills.ForEach(delegate (IMyShipDrill drill)
            {
                switch (drills_orientation)
                {
                    case "x":
                        if (first || drill.Position.Y < x_min) x_min = drill.Position.Y;
                        if (first || drill.Position.Y > x_max) x_max = drill.Position.Y;
                        if (first || drill.Position.Z < y_min) y_min = drill.Position.Z;
                        if (first || drill.Position.Z > y_max) y_max = drill.Position.Z;
                        break;
                    case "y":
                        if (first || drill.Position.X < x_min) x_min = drill.Position.X;
                        if (first || drill.Position.X > x_max) x_max = drill.Position.X;
                        if (first || drill.Position.Z < y_min) y_min = drill.Position.Z;
                        if (first || drill.Position.Z > y_max) y_max = drill.Position.Z;
                        break;
                    default:
                        if (first || drill.Position.X < x_min) x_min = drill.Position.X;
                        if (first || drill.Position.X > x_max) x_max = drill.Position.X;
                        if (first || drill.Position.Y < y_min) y_min = drill.Position.Y;
                        if (first || drill.Position.Y > y_max) y_max = drill.Position.Y;
                        break;
                }
                first = false;
            });
            //drawingSurface.WriteText($"X min:{x_min} Y min:{y_min}\n", false);
            drills.ForEach(delegate (IMyShipDrill drill)
            {
                IMyInventory block_inventory = drill.GetInventory(0);
                long volume = block_inventory.CurrentVolume.RawValue;
                long maxVolume = block_inventory.MaxVolume.RawValue;
                float x = 0;
                float y = 0;
                switch (drills_orientation)
                {
                    case "x":
                        x = Math.Abs(drill.Position.Y - x_min);
                        y = Math.Abs(drill.Position.Z - y_min);
                        break;
                    case "y":
                        x = Math.Abs(drill.Position.X - x_min);
                        y = Math.Abs(drill.Position.Z - y_min);
                        break;
                    default:
                        x = Math.Abs(drill.Position.X - x_min);
                        y = Math.Abs(drill.Position.Y - y_min);
                        break;
                }
                //drawingSurface.WriteText($"X:{x} Y:{y}\n", true);
                if (drills_flip_x) x = Math.Abs(x_max - x_min) - x;
                if (drills_flip_y) y = Math.Abs(y_max - y_min) - y;
                //drawingSurface.WriteText($"Volume [{x},{y}]:{volume}/{maxVolume}\n", true);
                Vector2 position_relative = drills_rotate ? new Vector2(y * (width + padding), x * (width + padding)) : new Vector2(x * (width + padding), y * (width + padding));

                frame.DrawGauge(frame.Position + position_relative, volume, maxVolume, style);
            });
        }
       
        public GaugeThresholds ChestThresholds { get; set; }
        private void LoadThresholds()
        {
            ChestThresholds = Properties.LoadThresholds("ChestThresholds", false);
            if (ChestThresholds == null)
            {
                ChestThresholds = new GaugeThresholds();
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0f, Color.Green));
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0.50f, new Color(180, 130, 0, 128)));
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0.75f, new Color(180, 0, 0, 128)));
            }
        }
        private string filter = "GM:Drills";
        private string drills_orientation = "z";
        private bool drills_rotate = false;
        private bool drills_flip_x = false;
        private bool drills_flip_y = false;
        private bool drills_info = false;
        private float drills_size = 25f;
        private float drills_margin_x = 0f;
        private float drills_margin_y = 0f;
        private float drills_padding = 2f;
        protected override void DataLoad()
        {
            Properties.Load();
            this.filter = Properties.Get("Drills", "filter", "GM:Drills");
            this.drills_orientation = Properties.Get("Drills", "orientation", "z");
            this.drills_rotate = Properties.GetBoolean("Drills", "rotate", false);
            this.drills_flip_x = Properties.GetBoolean("Drills", "flip_x", false);
            this.drills_flip_y = Properties.GetBoolean("Drills", "flip_y", false);
            this.drills_size = Properties.GetSingle("Drills", "size", 25f);
            this.drills_info = Properties.GetBoolean("Drills", "info", false);
            this.drills_margin_x = Properties.GetSingle("Drills", "margin_x", 0f);
            this.drills_margin_y = Properties.GetSingle("Drills", "margin_y", 0f);
            this.drills_padding = Properties.GetSingle("Drills", "padding", 2f);
            LoadThresholds();
        }
        protected override void DataSave()
        {
            Properties.Set("Drills", "filter", this.filter);
            Properties.Set("Drills", "orientation", this.drills_orientation);
            Properties.Set("Drills", "rotate", this.drills_rotate);
            Properties.Set("Drills", "flip_x", this.drills_flip_x);
            Properties.Set("Drills", "flip_y", this.drills_flip_y);
            Properties.Set("Drills", "size", this.drills_size);
            Properties.Set("Drills", "info", this.drills_info);
            Properties.Set("Drills", "margin_x", this.drills_margin_x);
            Properties.Set("Drills", "margin_y", this.drills_margin_y);
            Properties.Set("Drills", "padding", this.drills_padding);
            Properties.Save();
        }
    }
}
