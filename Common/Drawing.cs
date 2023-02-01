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
        
        public class Drawing
        {
            public float Padding_x = 10f;
            public float Padding_y = 10f;
            public string Font = "Monospace";

            private IMyTextPanel surfaceProvider;
            private MySpriteDrawFrame frame;
            public RectangleF viewport;

            private MySprite icon;

            public Dictionary<string, string> Symbol = new Dictionary<string, string>();

            public Drawing(IMyTextPanel lcd)
            {
                surfaceProvider = lcd;
                Initialize();
            }

            private void Initialize()
            {
                // Set the sprite display mode
                surfaceProvider.ContentType = ContentType.SCRIPT;
                // Make sure no built-in script has been selected
                surfaceProvider.Script = "";
                // Calculate the viewport by centering the surface size onto the texture size
                this.viewport = new RectangleF((surfaceProvider.TextureSize - surfaceProvider.SurfaceSize) / 2f, surfaceProvider.SurfaceSize);
                // Retrieve the Large Display, which is the first surface
                this.frame = surfaceProvider.DrawFrame();
                Symbol.Add("Cobalt", "Co");
                Symbol.Add("Nickel", "Ni");
                Symbol.Add("Magnesium", "Mg");
                Symbol.Add("Platinum", "Pt");
                Symbol.Add("Iron", "Fe");
                Symbol.Add("Gold", "Au");
                Symbol.Add("Silicon", "Si");
                Symbol.Add("Silver", "Ag");
                Symbol.Add("Stone", "Stone");
                Symbol.Add("Uranium", "U");
                Symbol.Add("Ice", "Ice");
            }

            public void Dispose()
            {
                // We are done with the frame, send all the sprites to the text panel
                this.frame.Dispose();
            }
            public void Clean()
            {
                AddForm(new Vector2(), SpriteForm.SquareSimple, viewport.Width, viewport.Height, Color.Black);
            }

            public MySprite AddSprite(MySprite sprite)
            {
                frame.Add(sprite);
                return sprite;
            }

            public MySprite AddForm(Vector2 position, SpriteForm form, float width, float height, Color color)
            {
                return AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = form.ToString(),
                    Size = new Vector2(width, height),
                    Color = color,
                    Position = position + new Vector2(0, height/2)

                });
            }

            public MySprite AddSprite(SpriteType type = SpriteType.TEXTURE, string data = null, Vector2? position = null, Vector2? size = null, Color? color = null, string fontId = null, TextAlignment alignment = TextAlignment.LEFT, float rotation = 0)
            {
                MySprite sprite = new MySprite(type, data, position, size, color, fontId, alignment, rotation);
                // Add the sprite to the frame
                frame.Add(sprite);
                return sprite;
            }

            public void DrawGaugeIcon(Vector2 position, string name, double amount, int limit, StyleIcon style_icon, int variance = 0)
            {
                Vector2 position2 = position + new Vector2(style_icon.Padding.X, style_icon.Padding.Y);
                // cadre info
                //AddForm(position2, SpriteForm.SquareSimple, style_icon.Width, style_icon.Height, new Color(40, 40, 40, 128));

                float width = (style_icon.Width - 3 * style_icon.Margin.X) / 3;
                float fontTitle = Math.Max(0.3f, (float)Math.Round(0.5f * (style_icon.Height / 80f), 1));
                float fontQuantity = Math.Max(0.5f, (float)Math.Round(1f * (style_icon.Height / 80f), 1));
                // Icon 
                AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = style_icon.path,
                    Size = new Vector2(width, width),
                    Color = style_icon.Color,
                    Position = position2 + new Vector2(0, width / 2)

                });

                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Horizontal,
                    Fullscreen = false,
                    Width = width * 2,
                    Height = width / 3,
                    Padding = new StylePadding(0),
                    RotationOrScale = Math.Max(0.3f, (float)Math.Round(0.6f * (style_icon.Height / 80f), 1))
                };
                DrawGauge(position2 + new Vector2(width + style_icon.Margin.X, style_icon.Height / 2), (float)amount, limit, style);

                // Element Name
                icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = name,
                    Size = new Vector2(width, width),
                    Color = Color.DimGray,
                    Position = position2 + new Vector2(style_icon.Margin.X, -8),
                    RotationOrScale = fontTitle,
                    FontId = Font,
                    Alignment = TextAlignment.LEFT

                };
                AddSprite(icon);
                // Quantity
                icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = Util.GetKiloFormat(amount),
                    Size = new Vector2(width, width),
                    Color = Color.LightGray,
                    Position = position2 + new Vector2(width + style_icon.Margin.X, style_icon.Margin.Y),
                    RotationOrScale = fontQuantity,
                    FontId = Font

                };
                AddSprite(icon);

                float symbolSize = 20f;
                if (variance == 1)
                {
                    AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = SpriteForm.Triangle.ToString(),
                        Size = new Vector2(symbolSize, symbolSize),
                        Color = new Color(0,100,0,255),
                        Position = position2 + new Vector2(3 * width - 25, symbolSize - style_icon.Margin.Y),
                        RotationOrScale = 0
                    });
                }
                if (variance == 3)
                {
                    AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = SpriteForm.Triangle.ToString(),
                        Size = new Vector2(symbolSize, symbolSize),
                        Color = new Color(100, 0, 0, 255),
                        Position = position2 + new Vector2(3 * width - 25, symbolSize + style_icon.Margin.Y),
                        RotationOrScale = (float)Math.PI
                    });
                }
            }

            public void DrawGauge(Vector2 position, float amount, float limit, StyleGauge style, bool invert = false)
            {
                float width = style.Width;
                float height = style.Height;
                
                if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Horizontal)) width = viewport.Width;
                if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Vertical)) height = viewport.Height;

                width += - 2 * style.Padding.X;
                height += - 2 * style.Padding.X;
                Vector2 position2 = position + new Vector2(style.Padding.X, style.Padding.Y);
                // Gauge
                AddForm(position2, SpriteForm.SquareSimple, width, height, style.Color);
                // Gauge Interrior
                AddForm(position2 + new Vector2(style.Margin.X, style.Margin.Y), SpriteForm.SquareSimple, width - 2 * style.Margin.X, height - 2 * style.Margin.Y, new Color(20, 20, 20, 255));

                // Gauge quantity
                float percent = Math.Min(1f, amount / limit);
                Color color = Color.Green;
                if (percent > 0.5 && !invert) color = new Color(180, 130, 0, 128);
                if (percent > 0.75 && !invert) color = new Color(180, 0, 0, 128);

                if (percent < 0.5 && invert) color = new Color(180, 130, 0, 128);
                if (percent < 0.25 && invert) color = new Color(180, 0, 0, 128);

                if (style.Orientation.Equals(SpriteOrientation.Horizontal))
                {
                    float width2 = width - 2 * style.Margin.X;
                    float height2 = height - 2 * style.Margin.Y;
                    float length = width2 * percent;
                    AddForm(position2 + new Vector2(style.Margin.X, style.Margin.Y), SpriteForm.SquareSimple, length, height2, color);
                }
                else
                {
                    float width2 = width - 2 * style.Margin.X;
                    float height2 = height - 2 * style.Margin.Y;
                    float length = height2 * percent;
                    AddForm(position2 + new Vector2(style.Margin.X, height2 - length + style.Margin.Y), SpriteForm.SquareSimple, width2, length, color);
                }
                if (style.Percent)
                {
                    string data = $"{percent:P0}";
                    if (percent < 0.999 && style.Round) data = $"{percent:P1}";
                    // Tag
                    icon = new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = data,
                        Size = new Vector2(width, width),
                        Color = Color.Black,
                        Position = position2 + new Vector2(2 * style.Margin.X, style.Margin.Y),
                        RotationOrScale = style.RotationOrScale,
                        FontId = Font,
                        Alignment = TextAlignment.LEFT

                    };
                    AddSprite(icon);
                }
            }
            public void Test()
            {
                MySprite icon;
                //Sandbox.ModAPI.Ingame.IMyTextSurface#GetSprites
                //Gets a list of available sprites
                var names = new List<string>();
                this.surfaceProvider.GetSprites(names);
                int count = -1;
                float width = 40;
                bool auto = false;
                if (auto)
                {
                    float delta = 100 - 4 * (viewport.Width - 100) * viewport.Height / names.Count;
                    width = (-10 + (float)Math.Sqrt(Math.Abs(delta))) / 2f;
                }
                float height = width + 10f;
                int limit = (int)Math.Floor(viewport.Height/height);
                Vector2 position = new Vector2(0, 0);

                foreach (string name in names)
                {
                    //logger.Debug(String.Format("Sprite {0}", name));
                    count++;
                    Vector2 position2 = position + new Vector2(width * (count / limit), height * (count - (count / limit) * limit));
                    icon = new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = name,
                        Size = new Vector2(width, width),
                        Color = Color.White,
                        Position = position2 + new Vector2(0, height/2+10/2),

                    };
                    this.frame.Add(icon);
                    icon = new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = count.ToString(),
                        Size = new Vector2(width, width),
                        RotationOrScale = 0.3f,
                        Color = Color.Gray,
                        Position = position2 + new Vector2(0, 0),
                        FontId = Font
                    };
                    this.frame.Add(icon);
                }
            }
        }

        public enum SpriteForm
        {
            SquareSimple,
            SquareHollow,
            Circle,
            Triangle
        }

        public enum SpriteOrientation
        {
            Horizontal,
            Vertical
        }

        public class StylePadding
        {
            public StylePadding(int x = 2, int y = 2)
            {
                X = x;
                Y = y;
            }
            public StylePadding(int value)
            {
                X = value;
                Y = value;
            }

            public int X = 2;
            public int Y = 2;
        }

        public class StyleMargin : StylePadding
        {
            public StyleMargin(int x = 2, int y = 2)
            {
                X = x;
                Y = y;
            }
            public StyleMargin(int value)
            {
                X = value;
                Y = value;
            }
        }
        public class Style
        {
            public StylePadding Padding = new StylePadding(2);
            public StyleMargin Margin = new StyleMargin(2);
            public float Width = 50f;
            public float Height = 50f;
            public Color Color = new Color(100, 100, 100, 128);
        }
        public class StyleIcon : Style
        {
            public string path;
        }
        public class StyleGauge : Style
        {
            public SpriteOrientation Orientation = SpriteOrientation.Horizontal;
            public bool Fullscreen = false;
            public bool Percent = true;
            public bool Round = true;
            public float RotationOrScale = 0.6f;
        }

        public class Item : IComparable<Item>
        {
            public const string TYPE_ORE = "MyObjectBuilder_Ore";
            public const string TYPE_INGOT = "MyObjectBuilder_Ingot";
            public const string TYPE_COMPONENT = "MyObjectBuilder_Component";
            public const string TYPE_AMMO = "MyObjectBuilder_AmmoMagazine";

            public string Name;
            public string Type;
            public Double Amount;
            public int Variance;

            public string Icon
            {
                get
                {
                    return String.Format("{0}/{1}", Type, Name);
                }
            }

            public int CompareTo(Item other)
            {
                return Amount.CompareTo(other.Amount);
            }
        }

        class BlockComparer : IComparer<IMyTerminalBlock>
        {
            public int Compare(IMyTerminalBlock block1, IMyTerminalBlock block2)
            {
                return block1.CustomName.CompareTo(block2.CustomName);
            }
        }
    }
}
