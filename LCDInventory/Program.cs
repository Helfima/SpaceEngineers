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
    partial class Program : MyGridProgram
    {
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        KProperty MyProperty;

        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;

        private BlockSystem<IMyTerminalBlock> inventories = null;
        private BlockSystem<IMyTextPanel> lcds = null;
        
        private Dictionary<string, Item> item_list = new Dictionary<string, Item>();
        
        private Dictionary<string, Model> model_list = new Dictionary<string, Model>();

        private MySprite icon;
        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
        }

        private void Init()
        {
            inventories = BlockSystem<IMyTerminalBlock>.SearchBlocks(this, block => block.HasInventory);
            lcds = BlockSystem<IMyTextPanel>.SearchBlocks(this);

            model_list = new Dictionary<string, Model>();
            addModel(new Model { Name = "Ice", Color = Color.LightBlue });
            addModel(new Model { Name = "Stone", Color = Color.Brown });
            addModel(new Model { Name = "Iron", Tag = "Fe", Color = Color.Orange });
            addModel(new Model { Name = "Nickel", Tag = "Ni", Color = Color.Gray });
            addModel(new Model { Name = "Cobalt", Tag = "Co", Color = Color.Blue });
            addModel(new Model { Name = "Magnesium", Tag = "Ma", Color = Color.Gray });
            addModel(new Model { Name = "Silicon", Tag = "Si", Color = Color.Gray });
            addModel(new Model { Name = "Silver", Tag = "Ag" , Color = Color.LightGray});
            addModel(new Model { Name = "Gold", Tag = "Au", Color = Color.Gold });
            addModel(new Model { Name = "Platinum", Tag = "Pl", Color = Color.Gray });
            addModel(new Model { Name = "Uranium", Tag = "Ur", Color = Color.GreenYellow});
        }

        private void addModel(Model model)
        {
            model_list.Add(model.Name, model);
        }

        public void Save()
        {
            MyProperty.Save();
        }

        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & CommandUpdate) != 0)
            {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update100) != 0)
            {
                RunContinuousLogic();
            }
        }

        private void RunCommand(string argument)
        {
            MyProperty.Load();
            Init();
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "reset":
                        Init();
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            Display();
            InventoryCount();
            DisplayLcd();
        }

        private void Display()
        {
            drawingSurface.WriteText($"Inventory list size:{inventories.List.Count}\n", false);
            drawingSurface.WriteText($"Ore list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_ORE).ToList().Count}\n", true);
            drawingSurface.WriteText($"Ingot list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_INGOT).ToList().Count}\n", true);
            drawingSurface.WriteText($"Component list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_COMPONENT).ToList().Count}\n", true);
            drawingSurface.WriteText($"Ammo list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_AMMO).ToList().Count}\n", true);
        }

        /*
        private void displaytext(lcd lcd, string type)
        {
            if (lcd.isempty) return;
            stringbuilder message = new stringbuilder();
            foreach (keyvaluepair<string, item> entry in item_list.where(entry => entry.value.type == type))
            {
                item item = entry.value;
                message.append(string.format("{0,6}:{1}\n", util.getkiloformat(item.amount), item.name));
            }
            lcd.print(message, false);
        }*/

        private void DisplayLcd()
        {
            lcds.List.ForEach(delegate (IMyTextPanel lcd) {
                if (lcd.CustomData != null && !lcd.CustomData.Equals(""))
                {
                    MyIniParseResult result;
                    MyIni MyIni = new MyIni();
                    MyIni.TryParse(lcd.CustomData, out result);
                    bool itemOre = MyIni.Get("Item", "ore").ToBoolean(false);
                    bool itemIngot = MyIni.Get("Item", "ingot").ToBoolean(false);
                    bool itemComponent = MyIni.Get("Item", "component").ToBoolean(false);
                    bool itemAmmo = MyIni.Get("Item", "ammo").ToBoolean(false);

                    bool inventory = MyIni.Get("Inventory", "on").ToBoolean(false);
                    string inventoryFilter = MyIni.Get("Inventory", "filter").ToString("none");
                    int inventoryHeight = MyIni.Get("Inventory", "height").ToInt32(300);

                    if (lcd.CustomData.Equals("prepare"))
                    {
                        MyIni.Set("Item", "ore", itemOre);
                        MyIni.Set("Item", "ingot", itemIngot);
                        MyIni.Set("Item", "component", itemComponent);
                        MyIni.Set("Item", "ammo", itemAmmo);

                        MyIni.Set("Inventory", "on", inventory);
                        MyIni.Set("Inventory", "filter", inventoryFilter);
                        MyIni.Set("Inventory", "height", inventoryHeight);
                        lcd.CustomData = MyIni.ToString();
                    }

                    Drawing drawing = new Drawing(lcd);
                    lcd.ScriptBackgroundColor = Color.Black;
                    Vector2 position = drawing.viewport.Position;

                    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                    if (inventory)
                    {
                        string mode = "none";
                        string search = "";
                        if (!inventoryFilter.Equals("none"))
                        {
                            if (inventoryFilter.Contains(":"))
                            {
                                string[] param = inventoryFilter.Trim().Split(':');
                                mode = param[0];
                                search = param[1];
                            }
                            else
                            {
                                mode = "name";
                                search = inventoryFilter.Trim();
                            }
                        }
                        //drawingSurface.WriteText($"\nMode:{mode}", true);
                        //drawingSurface.WriteText($"\nsearch:{search}", true);
                        switch (mode)
                        {
                            case "group":
                                blocks = inventories.List.Where(inv => inv.CustomName.Equals(search)).ToList();
                                break;
                            case "name":
                                blocks = inventories.List.Where(inv => inv.CustomName.Equals(search)).ToList();
                                break;
                            default:
                                blocks = inventories.List;
                                break;
                        }
                        long volumes = 0;
                        long maxVolumes = 1;
                        blocks.ForEach(delegate (IMyTerminalBlock block)
                        {
                            for (int i = 0; i < block.InventoryCount; i++)
                            {
                                IMyInventory block_inventory = block.GetInventory(i);
                                long volume = block_inventory.CurrentVolume.RawValue;
                                volumes += volume;
                                long maxVolume = block_inventory.MaxVolume.RawValue;
                                maxVolumes += maxVolume;
                                //drawingSurface.WriteText($"\nVolume:{volume}/{maxVolume}", true);
                            }
                        });
                        //drawingSurface.WriteText($"\nVolume:{volumes}/{maxVolumes}", true);
                        float width = 80f;
                        DisplayVolume(drawing, volumes, maxVolumes, position, width, inventoryHeight);
                        position += new Vector2(width + 20, 0);
                    }
                    

                    List<string> types = new List<string>();
                    if (itemOre) types.Add(Item.TYPE_ORE);
                    if (itemIngot) types.Add(Item.TYPE_INGOT);
                    if (itemComponent) types.Add(Item.TYPE_COMPONENT);
                    if (itemAmmo) types.Add(Item.TYPE_AMMO);

                    DisplayByType(drawing, position, types);
                    drawing.Dispose();
                }
            });
        }

        private void DisplayVolume(Drawing drawing, long volumes, long maxVolumes, Vector2 position, float width = 80f, int height = 300)
        {
            // Jauge
            icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2(width, height),
                Color = Color.Gray,
                Position = position + new Vector2(10 , height/2 + 30)

            };
            // Jauge quantity
            drawing.AddSprite(icon);
            // attention un long est un entier et la division l'ai aussi
            float ratio = (volumes*1000 / maxVolumes);
            float percent = ratio / 1000;
            drawingSurface.WriteText($"\nVolumes:{volumes}/{maxVolumes}", true);
            drawingSurface.WriteText($"\nPercent:{percent:P1}", true);
            Color color = Color.Green;
            if (percent > 0.5) color = Color.Yellow;
            if (percent > 0.75) color = Color.Red;

            drawing.AddSprite(icon); icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2(width-4 , height * percent),
                Color = color,
                Position = position + new Vector2(10+2, height / 2 + 30 + ((height-4) * (1-percent))/2 -2)
            };
            drawing.AddSprite(icon);
            // Tag
            icon = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = $"{percent:P1}",
                Size = new Vector2(width, width),
                Color = Color.DimGray,
                Position = position + new Vector2(10, 0),
                RotationOrScale = 0.7f,
                FontId = "Monospace",
                Alignment = TextAlignment.LEFT

            };
            drawing.AddSprite(icon);
            
        }
        private void DisplayByType(Drawing drawing, Vector2 position, List<string> types)
        {
            int count = -1;
            float width = 80f;
            int limit = 6;
            foreach (string type in types)
            {
                foreach (KeyValuePair<string, Item> entry in item_list.OrderByDescending(entry => entry.Value.Amount).Where(entry => entry.Value.Type == type))
                {
                    Item item = entry.Value;
                    count++;
                    Vector2 position2 = position + new Vector2(width * 3 * (count / limit), width * (0.5f + count - (count / limit) * limit));
                    // Icon
                    DisplayIcon(drawing, item, position2, width);
                }
            }
        }

        private void DisplayIcon(Drawing drawing, Item item, Vector2 position, float width = 80f)
        {
            // Icon
            string colorDefault = MyProperty.Get("color", "default");
            Color color = MyProperty.GetColor("color", item.Name, colorDefault);
            string tag = item.Name;
            icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = item.Icon,
                Size = new Vector2(width, width),
                Color = color,
                Position = position

            };
            // Jauge
            drawing.AddSprite(icon); icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2(width, 15),
                Color = Color.Gray,
                Position = position + new Vector2(width + 10, 20)

            };
            // Jauge quantity
            drawing.AddSprite(icon);
            int limitDefault = MyProperty.GetInt("Limit", "default");
            int limitBar = MyProperty.GetInt("Limit", item.Name, limitDefault);
            double percent = Math.Min(item.Amount / limitBar, 1);
            drawing.AddSprite(icon); icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2((width - 4) * float.Parse(percent.ToString()), 15 - 2),
                Color = Color.Red,
                Position = position + new Vector2(width + 10 + 2, 20 + 2)

            };
            drawing.AddSprite(icon);
            // Tag
            icon = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = tag,
                Size = new Vector2(width, width),
                Color = Color.DimGray,
                Position = position + new Vector2(5, -width / 2),
                RotationOrScale = 0.5f,
                FontId = "Monospace",
                Alignment = TextAlignment.LEFT

            };
            drawing.AddSprite(icon);
            // Quantity
            icon = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = Util.GetKiloFormat(item.Amount),
                Size = new Vector2(width, width),
                Color = Color.LightGray,
                Position = position + new Vector2(width + 10, 10 - width / 2),
                RotationOrScale = 1f,
                FontId = "Monospace"

            };
            drawing.AddSprite(icon);
        }

        private void InventoryCount()
        {
            item_list.Clear();
            foreach (IMyTerminalBlock block in inventories.List)
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

        private class Model
        {
            private string name;
            private string tag;
            private Color color;

            public string Name
            {
                get{return name;}
                set{name = value;}
            }

            public string Tag
            {
                get{
                    if (tag != null) return tag;
                    return name;
                }
                set{
                    tag = value;
                }
            }
            public Color Color
            {
                get{
                    if (color != null) return color;
                    return Color.Gray;
                }
                set{
                    color = value;
                }
            }
        }

        private class Item : IComparable<Item>
        {
            public const string TYPE_ORE = "MyObjectBuilder_Ore";
            public const string TYPE_INGOT = "MyObjectBuilder_Ingot";
            public const string TYPE_COMPONENT = "MyObjectBuilder_Component";
            public const string TYPE_AMMO = "MyObjectBuilder_AmmoMagazine";

            public string Name;
            public string Type;
            public Double Amount;

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
    }
}
