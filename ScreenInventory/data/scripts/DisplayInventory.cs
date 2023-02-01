using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRageMath;
using VRage.Game.GUI.TextPanel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using VRage.Game;

using IMyTextSurface = Sandbox.ModAPI.Ingame.IMyTextSurface;
using IMyCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;
using IMyEntity = VRage.ModAPI.IMyEntity;
using IMyInventory = VRage.Game.ModAPI.Ingame.IMyInventory;

namespace ScreenInventory
{
    [MyTextSurfaceScript("LCDInventory", "LCD Inventory")]
    public class DisplayInventory : MyTextSurfaceScriptBase
    {
        private IMyTextSurface surface;
        public RectangleF viewport;
        private MySpriteDrawFrame frame;
        public string Font = "Monospace";
        private bool IsWrite = false;
        private Drawing drawing;
        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update100;
        public DisplayInventory(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.surface = surface;
            // Set the sprite display mode
            surface.ContentType = ContentType.SCRIPT;

            this.viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
            surface.ScriptBackgroundColor = Color.Black;

            drawing = new Drawing(surface);
        }

        public override void Run()
        {
            try
            {
                Search();
                //InventoryCount();
                Draw();
            }
            catch { }
        }
        public override void Dispose()
        {
        }

        public void Draw()
        {
            frame = surface.DrawFrame();
            MySprite icon;
            //Sandbox.ModAPI.Ingame.IMyTextSurface#GetSprites
            //Gets a list of available sprites
            var names = new List<string>();
            this.surface.GetSprites(names);
            int count = -1;
            float width = 30;
            bool auto = false;
            if (auto)
            {
                float delta = 100 - 4 * (this.viewport.Width - 100) * this.viewport.Height / names.Count;
                width = (-10 + (float)Math.Sqrt(Math.Abs(delta))) / 2f;
            }
            float height = width + 10f;
            int limit = (int)Math.Floor(this.viewport.Height / height);
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
                    Position = position2 + new Vector2(0, height / 2 + 10 / 2),

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
            frame.Dispose();
        }

        private List<IMyTerminalBlock> inventories = null;
        private Dictionary<string, Item> item_list = new Dictionary<string, Item>();
        private Dictionary<string, double> last_amount = new Dictionary<string, double>();

        private bool isSearched = false;
        private void Search()
        {
            if (isSearched) return;
            var grid = this.Block.CubeGrid;
            var components = grid.Components;
            var entities = new HashSet<IMyEntity>();
            Sandbox.ModAPI.MyAPIGateway.Entities.GetEntities(entities, delegate (IMyEntity entity)
            {
                if (entity is IMyTerminalBlock)
                {
                    var block = entity as IMyTerminalBlock;
                    return block.CubeGrid == grid && block.HasInventory;
                }

                return false;
            });
            inventories = entities.Cast<IMyTerminalBlock>().ToList();
            isSearched = true;
        }
        private void InventoryCount()
        {
            item_list.Clear();
            foreach (IMyTerminalBlock block in inventories)
            {

                for (int i = 0; i < block.InventoryCount; i++)
                {
                    IMyInventory block_inventory = block.GetInventory(i);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    block_inventory.GetItems(items);
                    foreach (MyInventoryItem block_item in items)
                    {

                        string name = Util.GetName(block_item);
                        string type = Util.GetType(block_item);
                        double amount = 0;
                        string key = String.Format("{0}_{1}", type, name);
                        //string icon = block_item.Type.
                        //DisplayLcd.program.drawingSurface.WriteText($"Type:{type} Name:{name}\n",true);
                        Double.TryParse(block_item.Amount.ToString(), out amount);
                        Item item = new Item()
                        {
                            Type = type,
                            Name = name,
                            Amount = amount
                        };

                        if (item_list.ContainsKey(key))
                        {
                            item_list[key].Amount += amount;
                        }
                        else
                        {
                            item_list.Add(key, item);
                        }
                    }
                }
            }
        }
    }

    public class Util
    {
        static public string GetKiloFormat(double value)
        {
            double pow = 1.0;
            string suffix = "";
            if (value > 1000.0)
            {
                int y = int.Parse(Math.Floor(Math.Log10(value) / 3).ToString());
                suffix = "KMGTPEZY".Substring(y - 1, 1);
                pow = Math.Pow(10, y * 3);
            }
            return String.Format("{0:0.0}{1}", (value / pow), suffix);

        }

        static public double RadToDeg(float angle)
        {
            return angle * 180 / Math.PI;
        }
        static public double DegToRad(float angle)
        {
            return angle * Math.PI / 180;
        }
        static public string GetType(MyInventoryItem inventory_item)
        {
            return inventory_item.Type.TypeId;
        }

        static public string GetName(MyInventoryItem inventory_item)
        {
            return inventory_item.Type.SubtypeId;
        }
        static public string GetType(MyProductionItem production_item)
        {
            MyDefinitionId itemDefinitionId;
            string subtypeName = production_item.BlueprintId.SubtypeName;
            string typeName = Util.GetName(production_item);
            if ((subtypeName.EndsWith("Rifle") || subtypeName.StartsWith("Welder") || subtypeName.StartsWith("HandDrill") || subtypeName.StartsWith("AngleGrinder"))
                && MyDefinitionId.TryParse("MyObjectBuilder_PhysicalGunObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
            if (subtypeName.StartsWith("Hydrogen") && MyDefinitionId.TryParse("MyObjectBuilder_GasContainerObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
            if (subtypeName.StartsWith("Oxygen") && MyDefinitionId.TryParse("MyObjectBuilder_OxygenContainerObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
            if ((subtypeName.Contains("Missile") || subtypeName.EndsWith("Magazine")) && MyDefinitionId.TryParse("MyObjectBuilder_AmmoMagazine", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
            if (MyDefinitionId.TryParse("MyObjectBuilder_Component", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
            return production_item.BlueprintId.TypeId.ToString();
        }

        static public string GetName(MyProductionItem production_item)
        {
            string subtypeName = production_item.BlueprintId.SubtypeName;
            if (subtypeName.EndsWith("Component")) subtypeName = subtypeName.Replace("Component", "");
            if (subtypeName.EndsWith("Rifle") || subtypeName.StartsWith("Welder") || subtypeName.StartsWith("HandDrill") || subtypeName.StartsWith("AngleGrinder")) subtypeName = subtypeName + "Item";
            if (subtypeName.EndsWith("Magazine")) subtypeName = subtypeName.Replace("Magazine", "");
            return subtypeName;
        }

        static public string CutString(string value, int limit)
        {
            if (value.Length > limit)
            {
                int len = (limit - 3) / 2;
                return value.Substring(0, len) + "..." + value.Substring(value.Length - len, len);
            }
            return value;
        }
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

    public class Drawing
    {
        public float Padding_x = 10f;
        public float Padding_y = 10f;
        public string Font = "Monospace";

        private IMyTextSurface surfaceProvider;
        private MySpriteDrawFrame frame;
        public RectangleF viewport;

        private MySprite icon;

        public Dictionary<string, string> Symbol = new Dictionary<string, string>();

        public Drawing(IMyTextSurface lcd)
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
                    Color = new Color(0, 100, 0, 255),
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

            width += -2 * style.Padding.X;
            height += -2 * style.Padding.X;
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
            int limit = (int)Math.Floor(viewport.Height / height);
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
                    Position = position2 + new Vector2(0, height / 2 + 10 / 2),

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
}
