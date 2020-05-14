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
            public const string CIRCLE = "Circle";
            public const string GRID = "Grid";
            public const string Rectangle = "Rectangle";

            private IMyTextPanel surfaceProvider;
            private MySpriteDrawFrame frame;
            public RectangleF viewport;

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
            }

            public void Dispose()
            {
                // We are done with the frame, send all the sprites to the text panel
                this.frame.Dispose();
            }

            public MySprite AddSprite(MySprite sprite)
            {
                frame.Add(sprite);
                return sprite;
            }

            public MySprite AddSprite(SpriteType type = SpriteType.TEXTURE, string data = null, Vector2? position = null, Vector2? size = null, Color? color = null, string fontId = null, TextAlignment alignment = TextAlignment.LEFT, float rotation = 0)
            {
                MySprite sprite = new MySprite(type, data, position, size, color, fontId, alignment, rotation);
                // Add the sprite to the frame
                frame.Add(sprite);
                return sprite;
            }

            public void Test()
            {
                MySprite icon;
                //Sandbox.ModAPI.Ingame.IMyTextSurface#GetSprites
                //Gets a list of available sprites
                var names = new List<string>();
                this.surfaceProvider.GetSprites(names);
                int count = 0;
                float width = 40f;
                int limit = 10;
                foreach (string name in names)
                {
                    //logger.Debug(String.Format("Sprite {0}", name));
                    count++;
                    Vector2 vPosition = this.viewport.Position + new Vector2(width * (1 + count / limit), width * (2 + count - (count / limit) * limit));
                    icon = new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = name,
                        Size = new Vector2(width, width),
                        Color = Color.White,
                        Position = vPosition

                    };
                    this.frame.Add(icon);
                    icon = new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = count.ToString(),
                        Size = new Vector2(width, width),
                        RotationOrScale = 0.5f,
                        Color = Color.Red,
                        Position = vPosition,
                        FontId = "InfoMessageBoxText"

                    };
                    this.frame.Add(icon);
                }
            }
        }


        
    }
}
