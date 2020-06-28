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
        public class DisplayDrill
        {
            protected DisplayLcd DisplayLcd;

            private bool enable = false;

            public bool search = true;

            private string filter = "*";

            private string drills_orientation = "y";
            private bool drills_rotate = false;
            private bool drills_flip_x = false;
            private bool drills_flip_y = false;
            private float drills_size = 50f;

            private BlockSystem<IMyShipDrill> drill_inventories;
            public DisplayDrill(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }
            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Drills", "on").ToBoolean(false);
                filter = MyIni.Get("Drills", "filter").ToString("GM:Drills");
                drills_orientation = MyIni.Get("Drills", "orientation").ToString("y");
                drills_rotate = MyIni.Get("Drills", "rotate").ToBoolean(false);
                drills_flip_x = MyIni.Get("Drills", "flip_x").ToBoolean(false);
                drills_flip_y = MyIni.Get("Drills", "flip_y").ToBoolean(false);
                drills_size = MyIni.Get("Drills", "size").ToSingle(50f);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Drills", "on", enable);
                MyIni.Set("Drills", "filter", filter);
                MyIni.Set("Drills", "orientation", drills_orientation);
                MyIni.Set("Drills", "rotate", drills_rotate);
                MyIni.Set("Drills", "flip_x", drills_flip_x);
                MyIni.Set("Drills", "flip_y", drills_flip_y);
                MyIni.Set("Drills", "size", drills_size);
            }

            private void Search()
            {
                BlockFilter<IMyShipDrill> block_filter = BlockFilter<IMyShipDrill>.Create(DisplayLcd.lcd, filter);
                drill_inventories = BlockSystem<IMyShipDrill>.SearchByFilter(DisplayLcd.program, block_filter);

                search = false;
            }
            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                if (search) Search();

                float width = drills_size;
                float padding = 4f;
                float x_min = 0f;
                float x_max = 0f;
                float y_min = 0f;
                float y_max = 0f;
                bool first = true;

                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Horizontal,
                    Fullscreen = false,
                    Width = width,
                    Height = width,
                    Padding = new StylePadding(0),
                    Round = false,
                    RotationOrScale = 0.5f
                };

                drawing.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Drill Number:{drill_inventories.List.Count} ({filter})",
                    Size = new Vector2(width, width),
                    Color = Color.DimGray,
                    Position = position + new Vector2(0, 0),
                    RotationOrScale = 0.5f,
                    FontId = drawing.Font,
                    Alignment = TextAlignment.LEFT

                });
                position += new Vector2(0, 20);
                drill_inventories.ForEach(delegate (IMyShipDrill drill)
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
                drill_inventories.ForEach(delegate (IMyShipDrill drill)
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

                    drawing.DrawGauge(position + position_relative, volume, maxVolume, style);
                });

                return position;
            }
        }
    }
}
