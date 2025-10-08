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

namespace AppsInventory.Common
{
    public class SurfaceDrawing : IDisposable
    {
        public string Font { get; } = "Monospace";
        public Sandbox.ModAPI.Ingame.IMyTextSurface surface;
        private MySpriteDrawFrame frame;
        public RectangleF viewport;
        public SurfaceDrawing(Sandbox.ModAPI.Ingame.IMyTextSurface surface)
        {
            this.surface = surface;
            Initialize();
        }
        public void Initialize()
        {
            // background color
            surface.ScriptBackgroundColor = Color.Black;
            // Calculate the viewport by centering the surface size onto the texture size
            this.viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
            this.position = this.viewport.Position;
            this.frame = this.surface.DrawFrame();
            PrepareSprite();
        }
        private Vector2 position;
        public Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }
        public void Dispose()
        {
            this.frame.Dispose();
        }

        public MySprite AddSprite(MySprite sprite)
        {
            this.frame.Add(sprite);
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
        public Dictionary<string, string> Sprites_component = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ingot = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ore = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_tool = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_seed = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ammo = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_other = new Dictionary<string, string>();

        private bool isPrepared = false;
        private void PrepareSprite()
        {
            if (this.isPrepared) return;
            this.isPrepared = true;
            var names = new List<string>();
            this.surface.GetSprites(names);
            foreach (var name in names)
            {
                if (name.Contains("/"))
                {
                    var words = name.Split('/');
                    switch (words[0])
                    {
                        case "MyObjectBuilder_AmmoMagazine":
                            Sprites_ammo.Add(words[1], name);
                            break;
                        case "MyObjectBuilder_Component":
                            Sprites_component.Add(words[1], name);
                            break;
                        case "MyObjectBuilder_Ingot":
                            Sprites_ingot.Add(words[1], name);
                            break;
                        case "MyObjectBuilder_Ore":
                            Sprites_ore.Add(words[1], name);
                            break;
                        case "MyObjectBuilder_PhysicalGunObject":
                            Sprites_tool.Add(words[1], name);
                            break;
                        case "MyObjectBuilder_SeedItem":
                            Sprites_seed.Add(words[1], name);
                            break;
                        default:
                            Sprites_other.Add(words[1], name);
                            break;
                    }
                }
            }
        }
    }
}
