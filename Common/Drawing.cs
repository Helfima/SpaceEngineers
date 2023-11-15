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
            private IMyTextSurface surface;
            private IMyTextSurfaceProvider provider;
            private List<SurfaceDrawing> surfaces = new List<SurfaceDrawing>();
            public Dictionary<string, string> Symbol = new Dictionary<string, string>();
            public Drawing(IMyTerminalBlock block)
            {
                if(block is IMyTextSurfaceProvider)
                {
                    Initialize(block as IMyTextSurfaceProvider);
                }
                else
                {
                    Initialize(block as IMyTextSurface);
                }
                Initialize();
            }
            public IMyTerminalBlock TerminalBlock
            {
                get {
                    if(this.provider != null)
                    {
                        return this.provider as IMyTerminalBlock;
                    }    
                    return this.surface as IMyTerminalBlock;
                }
            }
            private void Initialize(IMyTextSurface surface)
            {
                this.surface = surface;
                this.surfaces.Add(new SurfaceDrawing(this, surface));
            }
            private void Initialize(IMyTextSurfaceProvider provider)
            {
                this.provider = provider;
                for (int i = 0; i < provider.SurfaceCount; i++)
                {
                    var surface = provider.GetSurface(i);
                    this.surfaces.Add(new SurfaceDrawing(this, surface));
                }
            }
            private void Initialize()
            {
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
            public SurfaceDrawing GetSurfaceDrawing(int index = 0)
            {
                if(index < surfaces.Count)
                {
                    return surfaces[index];
                }
                return null;
            }
            public void Dispose()
            {
                foreach (var surface in this.surfaces)
                {
                    surface.Dispose();
                }
            }
            public void Clean()
            {
                foreach (var surface in this.surfaces)
                {
                    surface.Clean();
                }
            }
        }


        public class SurfaceDrawing
        {
            public float Padding_x = 10f;
            public float Padding_y = 10f;
            public string Font { get; } = "Monospace";

            public IMyTextSurface Surface;
            private MySpriteDrawFrame frame;
            public RectangleF Viewport;

            private MySprite icon;
            private bool initialized = false;


            public SurfaceDrawing(Drawing parent, IMyTextSurface surface)
            {
                this.parent = parent;
                this.Surface = surface;
            }
            private Drawing parent;
            public Drawing Parent {
                get { return this.parent; }
            }
            private Vector2 position;
            public Vector2 Position
            {
                get { return this.position; }   
                set { this.position = value; }
            }
            /// <summary>
            /// Initialize only if not
            /// </summary>
            public void Initialize()
            {
                if (this.initialized) return;
                initialized = true;
                // Set the sprite display mode
                Surface.ContentType = ContentType.SCRIPT;
                // Make sure no built-in script has been selected
                Surface.Script = "";
                // background color
                Surface.ScriptBackgroundColor = Color.Black;
                // Calculate the viewport by centering the surface size onto the texture size
                this.Viewport = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2f, Surface.SurfaceSize);
                this.position = this.Viewport.Position;
                // Retrieve the Large Display, which is the first surface
                this.frame = Surface.DrawFrame();
                // add clip token
                this.frame.Clip(0, 0, (int)this.Viewport.Width, (int)this.Viewport.Height);
            }
            /// <summary>
            /// Dipose if initialized
            /// </summary>
            public void Dispose()
            {
                if (this.initialized)
                {
                    // We are done with the frame, send all the sprites to the text panel
                    this.frame.Dispose();
                }
            }
            public void Clean()
            {
                if (this.initialized)
                {
                    AddForm(new Vector2(), SpriteForm.SquareSimple, Viewport.Width, Viewport.Height, Color.Black);
                }
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

                float factor = 2.5f;

                float width = (style_icon.Width - 3 * style_icon.Margin.X) / factor;
                float height = (style_icon.Height - 3 * style_icon.Margin.Y);
                string font_title = EnumFont.BuildInfo;
                float font_size_title = Math.Max(0.3f, (float)Math.Round(height / 4f / 32f, 1));
                float deltaTitle = font_size_title * 20f;

                string font_quantity = EnumFont.BuildInfo;
                float font_size_quantity = Math.Max(0.3f, (float)Math.Round(height / 2.25f / 32f, 1));
                float deltaQuantity = font_size_quantity * 32f;

                float icon_size = style_icon.Height - style_icon.Margin.Y - deltaTitle;

                AddForm(position2, SpriteForm.SquareSimple, style_icon.Width, style_icon.Height, new Color(5, 5, 5, 125));
                // Add Icon 
                AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = style_icon.path,
                    Size = new Vector2(icon_size, icon_size),
                    Color = style_icon.Color,
                    Position = position2 + new Vector2(0, deltaTitle + icon_size / 2)
                });

                // Add Gauge
                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Horizontal,
                    Fullscreen = false,
                    Width = width * (factor - 1f),
                    Height = height / 3,
                    Padding = new StylePadding(0),
                    Thresholds = style_icon.Thresholds
                };
                DrawGauge(position2 + new Vector2(width + style_icon.Margin.X, deltaTitle + deltaQuantity + style_icon.Margin.Y), (float)amount, limit, style);

                // Element Name
                icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = name,
                    //Size = new Vector2(width, width),
                    Color = Color.DimGray,
                    Position = position2,
                    RotationOrScale = font_size_title,
                    FontId = font_title,
                    Alignment = TextAlignment.LEFT

                };
                AddSprite(icon);
                // Quantity
                icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = Util.GetKiloFormat(amount),
                    //Size = new Vector2(width, width),
                    Color = Color.LightGray,
                    Position = position2 + new Vector2(width + style_icon.Margin.X, deltaTitle + style_icon.Margin.Y),
                    RotationOrScale = font_size_quantity,
                    FontId = font_quantity

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
                        Position = position2 + new Vector2(factor * width - 25, symbolSize - style_icon.Margin.Y),
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
                        Position = position2 + new Vector2(factor * width - 25, symbolSize + style_icon.Margin.Y),
                        RotationOrScale = (float)Math.PI
                    });
                }
            }
            public Vector2 DrawGauge(Vector2 position, float amount, float limit, StyleGauge style, bool invert)
            {
                return DrawGauge(position, amount, limit, style, invert);
            }
            public Vector2 DrawGauge(Vector2 position, float amount, float limit, StyleGauge style)
            {
                float width = style.Width;
                float height = style.Height;
                
                if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Horizontal)) width = Viewport.Width;
                if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Vertical)) height = Viewport.Height;

                width += - 2 * style.Padding.X;
                height += - 2 * style.Padding.X;
                Vector2 position2 = position + new Vector2(style.Padding.X, style.Padding.Y);
                // Gauge
                AddForm(position2, SpriteForm.SquareSimple, width, height, style.Color);
                // Gauge Interrior
                AddForm(position2 + new Vector2(style.Margin.X, style.Margin.Y), SpriteForm.SquareSimple, width - 2 * style.Margin.X, height - 2 * style.Margin.Y, new Color(20, 20, 20, 255));

                // Gauge quantity
                float percent = Math.Min(1f, amount / limit);
                var threshold = style.Thresholds.GetGaugeThreshold(percent);
                Color color = threshold.Color;

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
                        
                        FontId = EnumFont.Monospace,
                        Alignment = TextAlignment.LEFT

                    };
                    if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Horizontal))
                    {
                        icon.RotationOrScale = Math.Max(0.3f, (float)Math.Round((height - 2 * style.Margin.Y) / 32f, 1));
                    }
                    else
                    {
                        icon.RotationOrScale = Math.Max(0.3f, (float)Math.Round((height - 2 * style.Margin.Y) / 32f, 1));
                    }
                    AddSprite(icon);
                }
                if (style.Orientation.Equals(SpriteOrientation.Horizontal))
                {
                    return position + new Vector2 (0, height + 2 * style.Margin.Y);
                }
                else
                {
                    return position + new Vector2(width + 2 * style.Margin.X, 0);
                }
            }
            public void Test(MyGridProgram program)
            {
                this.Initialize();
                MySprite icon;
                //Sandbox.ModAPI.Ingame.IMyTextSurface#GetSprites
                //Gets a list of available sprites
                var names = new List<string>();
                this.Surface.GetSprites(names);
                int count = -1;
                float width = 35;
                bool auto = false;
                if (auto)
                {
                    float delta = 100 - 4 * (Viewport.Width - 100) * Viewport.Height / names.Count;
                    width = (-10 + (float)Math.Sqrt(Math.Abs(delta))) / 2f;
                }
                float height = width + 10f;
                int limit = (int)Math.Floor(Viewport.Height/height);
                Vector2 position = new Vector2(0, 0);
                program.Echo($"Count names: {names.Count}");
                program.Echo($"limit: {limit}");
                var customData = new StringBuilder();
                foreach (string name in names)
                {
                    count++;
                    customData.AppendLine($"{count}:{name}");
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
                        RotationOrScale = 0.4f,
                        Color = Color.Gray,
                        Position = position2 + new Vector2(0, 0),
                        FontId = EnumFont.BuildInfo
                    };
                    this.frame.Add(icon);
                }
                this.parent.TerminalBlock.CustomData = customData.ToString();
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
            public StylePadding(float x = 2, float y = 2)
            {
                X = x;
                Y = y;
            }
            public StylePadding(float value)
            {
                X = value;
                Y = value;
            }

            public float X = 2;
            public float Y = 2;

            public virtual void Scale(float scale)
            {
                this.X *= scale;
                this.Y *= scale;
            }
        }
        
        public class StyleMargin : StylePadding
        {
            public StyleMargin(float x = 2, float y = 2)
            {
                X = x;
                Y = y;
            }
            public StyleMargin(float value)
            {
                X = value;
                Y = value;
            }
        }
        public class Style
        {
            public StylePadding Padding { get; set; } = new StylePadding(2);
            public StyleMargin Margin { get; set; } = new StyleMargin(2);
            public float Width { get; set; } = 50f;
            public float Height { get; set; } = 50f;
            public float RotationOrScale { get; set; } = 1f;
            public Color Color { get; set; } = new Color(100, 100, 100, 128);
            public virtual void Scale(float scale)
            {
                this.Width *= scale;
                this.Height *= scale;
                this.Padding.Scale(scale);
                this.Margin.Scale(scale);
            }
        }
        public class StyleIcon : Style
        {
            public string path { get; set; }
            public GaugeThresholds Thresholds { get; set; } = new GaugeThresholds();
        }
        public class StyleGauge : Style
        {
            public SpriteOrientation Orientation { get; set; } = SpriteOrientation.Horizontal;
            public bool Fullscreen { get; set; } = false;
            public bool Percent { get; set; } = true;
            public bool Round { get; set; } = true;
            public GaugeThresholds Thresholds { get; set; } = new GaugeThresholds();
        }
        public class GaugeThresholds
        {
            public List<GaugeThreshold> Thresholds { get; set; } = new List<GaugeThreshold>();
            public GaugeThreshold GetGaugeThreshold(float value)
            {
                GaugeThreshold gaugeThreshold = Thresholds.First();
                foreach (var threshold in Thresholds)
                {
                    if (value >= threshold.Value)
                    {
                        gaugeThreshold = threshold;
                    }
                }
                return gaugeThreshold;
            }
        }
        public class GaugeThreshold
        {
            public GaugeThreshold()
            {

            }
            public GaugeThreshold(float value, Color color)
            {
                Value = value;
                Color = color;
            }
            public float Value { get; set; }
            public Color Color { get; set; }

            public override string ToString()
            {
                return $"{Value}:{Color.R},{Color.G},{Color.B},{Color.A}";
            }
        }

        public class Item : IComparable<Item>
        {
            public const string TYPE_ORE = "MyObjectBuilder_Ore";
            public const string TYPE_INGOT = "MyObjectBuilder_Ingot";
            public const string TYPE_COMPONENT = "MyObjectBuilder_Component";
            public const string TYPE_AMMO = "MyObjectBuilder_AmmoMagazine";

            public string Name { get; set; }
            public string Type { get; set; }
            public Double Amount { get; set; }
            public int Variance { get; set; }

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
