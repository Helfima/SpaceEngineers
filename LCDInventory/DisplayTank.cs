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

            private bool enable = false;

            private bool tank_h2 = false;
            private bool tank_o2 = false;
            public DisplayTank(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }

            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Tank", "on").ToBoolean(false);
                tank_h2 = MyIni.Get("Tank", "H2").ToBoolean(false);
                tank_o2 = MyIni.Get("Tank", "O2").ToBoolean(false);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Tank", "on", enable);
                MyIni.Set("Tank", "H2", tank_h2);
                MyIni.Set("Tank", "O2", tank_o2);
            }
            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                List<string> types = new List<string>();
                if (tank_h2) types.Add("Hydrogen");
                if (tank_o2) types.Add("Oxygen");
                if (types.Count > 0)
                {
                    foreach (string type in types)
                    {
                        BlockSystem<IMyGasTank> tanks = BlockSystem<IMyGasTank>.SearchBlocks(DisplayLcd.program, block => block.BlockDefinition.SubtypeName.Contains(type));
                        float volumes = 0f;
                        float capacity = 0f;
                        float width = 50f;
                        StyleGauge style = new StyleGauge()
                        {
                            Orientation = SpriteOrientation.Horizontal,
                            Fullscreen = true,
                            Width = width,
                            Height = width,
                            Padding = new StylePadding(0),
                            Round = false,
                            RotationOrScale = 0.5f
                        };

                        MySprite text = new MySprite()
                        {
                            Type = SpriteType.TEXT,
                            Color = Color.DimGray,
                            Position = position + new Vector2(0, 0),
                            RotationOrScale = 1f,
                            FontId = drawing.Font,
                            Alignment = TextAlignment.LEFT

                        };

                        tanks.ForEach(delegate (IMyGasTank block)
                        {
                            volumes += (float)block.FilledRatio * block.Capacity;
                            capacity += block.Capacity;
                        });

                        drawing.DrawGauge(position, volumes, capacity, style);
                        position += new Vector2(0, 60);
                        switch (type)
                        {
                            case "Hydrogen":
                                text.Data = $"H2: {Math.Round(volumes, 2)}L / {Math.Round(capacity, 2)}L";
                                break;
                            case "Oxygen":
                                text.Data = $"O2: {Math.Round(volumes, 2)}L / {Math.Round(capacity, 2)}L";
                                break;
                        }
                        text.Position = position;
                        drawing.AddSprite(text);
                        position += new Vector2(0, 60);
                    }
                }
                return position;
            }
        }
    }
}
