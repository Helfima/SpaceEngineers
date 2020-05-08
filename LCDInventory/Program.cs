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
        private StateMachine machine_state = StateMachine.Stopped;

        private BlockSystem<IMyTerminalBlock> inventories = null;
        private Lcd ore_lcd = null;
        private Lcd ingot_lcd = null;
        private Lcd component_lcd = null;
        private Lcd ammo_lcd = null;

        private Dictionary<string, Item> item_list = new Dictionary<string, Item>();
        
        private Dictionary<string, Model> model_list = new Dictionary<string, Model>();

        private bool display_text = false;

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
            ore_lcd = new Lcd(this);
            ore_lcd.Search(MyProperty.Search_Lcd_Ore);
            ingot_lcd = new Lcd(this);
            ingot_lcd.Search(MyProperty.Search_Lcd_Ingot);
            component_lcd = new Lcd(this);
            component_lcd.Search(MyProperty.Search_Lcd_Component);
            ammo_lcd = new Lcd(this);
            ammo_lcd.Search(MyProperty.Search_Lcd_Ammo);

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
                    case "text":
                        display_text = true;
                        break;
                    case "icon":
                        display_text = false;
                        break;
                    case "test":
                        test2();
                        break;
                }
            }
        }
        private void test()
        {
            StringBuilder message = new StringBuilder();
            List<string> sprites = new List<string>();
            drawingSurface.GetSprites(sprites);
            foreach (string sprite in sprites)
            {
                message.Append(String.Format("{0}\n", sprite));
            }
            Me.CustomData = message.ToString();
        }
        private void test2()
        {
            StringBuilder message = new StringBuilder();
            foreach (IMyTerminalBlock block in inventories.List)
            {
                for (int i = 0; i < block.InventoryCount; i++)
                {
                    IMyInventory block_inventory = block.GetInventory(i);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    block_inventory.GetItems(items);
                    foreach (MyInventoryItem block_item in items)
                    {

                        message.Append(String.Format("{0} {1}\n", block_item.GetType().Name, block_item.GetType().FullName));
                    }
                }
            }
            Me.CustomData = message.ToString();
        }

        void RunContinuousLogic()
        {
            Display();
            InventoryCount();
            if (display_text)
            {
                DisplayText();
            } else
            {
                DisplayIcon();
            }
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{machine_state}\n", false);
            drawingSurface.WriteText($"Item list size:{item_list.Count}\n", true);
        }

        private void DisplayText()
        {
            DisplayText(ore_lcd, Item.TYPE_ORE);
            DisplayText(ingot_lcd, Item.TYPE_INGOT);
            DisplayText(component_lcd, Item.TYPE_COMPONENT);
            DisplayText(ammo_lcd, Item.TYPE_AMMO);
        }
        private void DisplayText(Lcd lcd, string type)
        {
            if (lcd.IsEmpty) return;
            StringBuilder message = new StringBuilder();
            foreach (KeyValuePair<string, Item> entry in item_list.Where(entry => entry.Value.Type == type))
            {
                Item item = entry.Value;
                message.Append(String.Format("{0,6}:{1}\n", Util.GetKiloFormat(item.Amount), item.Name));
            }
            lcd.Print(message, false);
        }

        private void DisplayIcon()
        {
            DisplayIcon(ore_lcd, Item.TYPE_ORE);
            DisplayIcon(ingot_lcd, Item.TYPE_INGOT);
            DisplayIcon(component_lcd, Item.TYPE_COMPONENT);
            DisplayIcon(ammo_lcd, Item.TYPE_AMMO);
        }
        private void DisplayIcon(Lcd lcd, string type)
        {
            if (lcd.IsEmpty) return;
            MySprite icon;
            int count = 0;
            float width = 80f;
            int limit = 7;
            Drawing drawing = lcd.GetDrawing();
            foreach (KeyValuePair<string, Item> entry in item_list.Where(entry => entry.Value.Type == type))
            {
                Item item = entry.Value;
                count++;
                Vector2 position = drawing.viewport.Position + new Vector2(width * 3 * (count / limit), width * (-0.5f + count - (count / limit) * limit));
                // Icon
                Color color = MyProperty.GetColor("color", item.Name);
                string tag = item.Name;
                if (model_list.ContainsKey(item.Name))
                {
                    Model model = model_list[item.Name];
                    color = MyProperty.GetColor("color", item.Name);
                    tag = model.Tag;
                }
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
                drawing.AddSprite(icon);
                double percent = Math.Min(item.Amount / 10000,1);
                drawing.AddSprite(icon); icon = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Size = new Vector2((width-4) * float.Parse(percent.ToString()), 15-2),
                    Color = Color.Red,
                    Position = position + new Vector2(width + 10+2, 20 + 2)

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
            drawing.Dispose();
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
                get
                {
                    return name;
                }
                set
                {
                    name = value;
                }
            }

            public string Tag
            {
                get
                {
                    if (tag != null) return tag;
                    return name;
                }
                set
                {
                    tag = value;
                }
            }
            public Color Color
            {
                get
                {
                    if (color != null) return color;
                    return Color.Gray;
                }
                set
                {
                    color = value;
                }
            }
        }

        private class Item
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
        }

        
        public enum StateMachine
        {
            Stopped,
            Traking,
            Running,
            Waitting
        }
    }
}
