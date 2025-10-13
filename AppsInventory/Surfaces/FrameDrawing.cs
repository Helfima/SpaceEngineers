using System;
using System.Collections.Generic;
using AppsInventory.Common;
using AppsInventory.Extensions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory.Surfaces
{
    public class FrameDrawing : IDisposable
    {
        private SurfaceDrawing parent;
        public SurfaceDrawing Parent => this.parent;
        private MySpriteDrawFrame frame;
        public FrameDrawing(SurfaceDrawing parent)
        {
            this.parent = parent;
            this.frame = parent.Surface.DrawFrame();
            this.position = parent.Viewport.Position;
        }
        public void Dispose()
        {
            frame.Dispose();
        }
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
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
                Position = position + new Vector2(0, height / 2)
            });
        }

        public MySprite AddSprite(SpriteType type = SpriteType.TEXTURE, string data = null, Vector2? position = null, Vector2? size = null, Color? color = null, string fontId = null, TextAlignment alignment = TextAlignment.LEFT, float rotation = 0)
        {
            MySprite sprite = new MySprite(type, data, position, size, color, fontId, alignment, rotation);
            // Add the sprite to the frame
            frame.Add(sprite);
            return sprite;
        }
        public Vector2 DrawGauge(Vector2 position, float amount, float limit, StyleGauge style, bool invert)
        {
            return DrawGauge(position, amount, limit, style, invert);
        }
        public Vector2 DrawGauge(Vector2 position, float amount, float limit, StyleGauge style)
        {
            float width = style.Width;
            float height = style.Height;

            if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Horizontal)) width = this.Parent.Viewport.Width;
            if (style.Fullscreen && style.Orientation.Equals(SpriteOrientation.Vertical)) height = this.Parent.Viewport.Height;

            width += -2 * style.Padding.X;
            height += -2 * style.Padding.X;
            Vector2 position2 = position + new Vector2(style.Padding.X, style.Padding.Y);
            // Gauge
            AddForm(position2, SpriteForm.SquareSimple, width, height, style.Color);
            // Gauge Interrior
            var color_interior = new Color(20, 20, 20, 255);
            AddForm(position2 + new Vector2(style.Margin.X, style.Margin.Y), SpriteForm.SquareSimple, width - 2 * style.Margin.X, height - 2 * style.Margin.Y, color_interior);

            // Gauge quantity
            float percent = Math.Min(1f, amount / limit);
            var threshold = style.Thresholds.GetGaugeThreshold(percent);
            Color color = threshold.Color * style.ColorSoftening;

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
                var icon = new MySprite()
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
                return position + new Vector2(0, height + 2 * style.Margin.Y);
            }
            else
            {
                return position + new Vector2(width + 2 * style.Margin.X, 0);
            }
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

            float globalSoftening = 0.7f;

            AddForm(position2, SpriteForm.SquareSimple, style_icon.Width, style_icon.Height, new Color(5, 5, 5, 125));
            // Add Icon 
            AddSprite(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = style_icon.path,
                Size = new Vector2(icon_size, icon_size),
                Color = style_icon.Color * globalSoftening,
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
                Thresholds = style_icon.Thresholds,
                ColorSoftening = style_icon.ColorSoftening
            };
            DrawGauge(position2 + new Vector2(width + style_icon.Margin.X, deltaTitle + deltaQuantity + style_icon.Margin.Y), (float)amount, limit, style);

            // Element Name
            var icon = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = name,
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
                Color = Color.LightGray * globalSoftening,
                Position = position2 + new Vector2(width + style_icon.Margin.X, deltaTitle + style_icon.Margin.Y),
                RotationOrScale = font_size_quantity,
                FontId = font_quantity

            };
            AddSprite(icon);

            float symbolSize = 20f * font_size_quantity;
            float offset = 25f * font_size_quantity;
            float delta = -2f;
            Color green = new Color(0, 100, 0, 255);
            Color red = new Color(100, 0, 0, 255);
            if (variance == 1)
            {
                AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = SpriteForm.Triangle.ToString(),
                    Size = new Vector2(symbolSize, symbolSize),
                    Color = green * style_icon.ColorSoftening,
                    Position = position2 + new Vector2(factor * width - offset, symbolSize - style_icon.Margin.Y),
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
                    Color = red * style_icon.ColorSoftening,
                    Position = position2 + new Vector2(factor * width - offset, symbolSize + style_icon.Margin.Y),
                    RotationOrScale = (float)Math.PI
                });
            }
        }
    }
}
