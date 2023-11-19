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
        public class DisplayTank
        {
            protected DisplayLcd DisplayLcd;

            private int panel = 0;
            private bool enable = false;

            private float scale = 1f;
            private bool tank_h2 = false;
            private bool tank_o2 = false;
            public DisplayTank(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }

            public void Load(MyIni MyIni)
            {
                panel = MyIni.Get("Tank", "panel").ToInt32(0);
                enable = MyIni.Get("Tank", "on").ToBoolean(false);
                scale = MyIni.Get("Tank", "scale").ToSingle(1f);
                tank_h2 = MyIni.Get("Tank", "H2").ToBoolean(true);
                tank_o2 = MyIni.Get("Tank", "O2").ToBoolean(true);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Tank", "panel", panel);
                MyIni.Set("Tank", "on", enable);
                MyIni.Set("Tank", "scale", scale);
                MyIni.Set("Tank", "H2", tank_h2);
                MyIni.Set("Tank", "O2", tank_o2);
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
                List<string> types = new List<string>();
                if (tank_h2) types.Add("Hydrogen");
                if (tank_o2) types.Add("Oxygen");
                if (types.Count > 0)
                {
                    foreach (string type in types)
                    {
                        BlockSystem<IMyGasTank> tanks = BlockSystem<IMyGasTank>.SearchBlocks(DisplayLcd.program, block => String.IsNullOrEmpty(block.BlockDefinition.SubtypeName) ? block.BlockDefinition.TypeIdString.Contains(type) : block.BlockDefinition.SubtypeName.Contains(type));
                        float volumes = 0f;
                        float capacity = 0f;
                        float width = 50f * scale;
                        StyleGauge style = new StyleGauge()
                        {
                            Orientation = SpriteOrientation.Horizontal,
                            Fullscreen = true,
                            Width = width,
                            Height = width,
                            Padding = new StylePadding(0),
                            Round = false,
                            RotationOrScale = 1f * scale,
                            Thresholds = this.DisplayLcd.program.MyProperty.TankThresholds
                        };

                        MySprite text = new MySprite()
                        {
                            Type = SpriteType.TEXT,
                            Color = Color.DimGray,
                            Position = surface.Position + new Vector2(0, 0),
                            RotationOrScale = 1f * scale,
                            FontId = surface.Font,
                            Alignment = TextAlignment.LEFT

                        };

                        tanks.ForEach(delegate (IMyGasTank block)
                        {
                            volumes += (float)block.FilledRatio * block.Capacity / 1000;
                            capacity += block.Capacity / 1000;
                        });

                        surface.DrawGauge(surface.Position, volumes, capacity, style);
                        surface.Position += new Vector2(0, 60 * scale);
                        switch (type)
                        {
                            case "Hydrogen":
                                text.Data = $"H2: {Math.Round(volumes, 2)}M³/{Math.Round(capacity, 2)}M³";
                                break;
                            case "Oxygen":
                                text.Data = $"O2: {Math.Round(volumes, 2)}M³/{Math.Round(capacity, 2)}M³";
                                break;
                        }
                        text.Position = surface.Position;
                        surface.AddSprite(text);
                        surface.Position += new Vector2(0, 60 * scale);
                    }
                }
            }
        }
    }
}
