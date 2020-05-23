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
            inventories = BlockSystem<IMyTerminalBlock>.SearchBlocks(this, block => (block.HasInventory && block.CubeGrid == Me.CubeGrid));
            lcds = BlockSystem<IMyTextPanel>.SearchBlocks(this, block => (block.CubeGrid == Me.CubeGrid));
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
                    case "test":
                        IMyTextPanel lcd = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(commandLine.Argument(1));
                        lcd.ScriptBackgroundColor = Color.Black;
                        Drawing drawing = new Drawing(lcd);
                        drawing.Test(drawingSurface);
                        drawing.Dispose();
                        break;
                    case "getname":
                        int index = 0;
                        int.TryParse(commandLine.Argument(1), out index);
                        var names = new List<string>();
                        drawingSurface.GetSprites(names);
                        Echo($"Sprite {index} name={names[index]}");
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
            drawingSurface.WriteText($"LCD list size:{lcds.List.Count}\n", false);
            drawingSurface.WriteText($"Inventory list size:{inventories.List.Count}\n", true);
            drawingSurface.WriteText($"Ore list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_ORE).ToList().Count}\n", true);
            drawingSurface.WriteText($"Ingot list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_INGOT).ToList().Count}\n", true);
            drawingSurface.WriteText($"Component list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_COMPONENT).ToList().Count}\n", true);
            drawingSurface.WriteText($"Ammo list size:{item_list.Where(entry => entry.Value.Type == Item.TYPE_AMMO).ToList().Count}\n", true);
        }

        private void DisplayLcd()
        {
            lcds.List.ForEach(delegate (IMyTextPanel lcd) {
                if (lcd.CustomData != null && !lcd.CustomData.Equals(""))
                {
                    MyIniParseResult result;
                    MyIni MyIni = new MyIni();
                    MyIni.TryParse(lcd.CustomData, out result);
                    bool inventoryGauge = MyIni.Get("Inventory", "gauge").ToBoolean(true);
                    string inventoryFilter = MyIni.Get("Inventory", "filter").ToString("none");

                    bool gauge = MyIni.Get("Gauge", "on").ToBoolean(true);
                    bool gaugeFullscreen = MyIni.Get("Gauge", "fullscreen").ToBoolean(true);
                    bool gaugeHorizontal = MyIni.Get("Gauge", "horizontal").ToBoolean(true);
                    float gaugeWidth = MyIni.Get("Gauge", "width").ToSingle(80f);
                    float gaugeHeight = MyIni.Get("Gauge", "height").ToSingle(40f);

                    bool itemOre = MyIni.Get("Item", "ore").ToBoolean(true);
                    bool itemIngot = MyIni.Get("Item", "ingot").ToBoolean(true);
                    bool itemComponent = MyIni.Get("Item", "component").ToBoolean(true);
                    bool itemAmmo = MyIni.Get("Item", "ammo").ToBoolean(true);

                    bool drills = MyIni.Get("Drills", "on").ToBoolean(false);
                    bool drills_flip_x = MyIni.Get("Drills", "flip_x").ToBoolean(false);
                    bool drills_flip_y = MyIni.Get("Drills", "flip_y").ToBoolean(false);

                    if (lcd.CustomData.Equals("prepare"))
                    {
                        drawingSurface.WriteText($"Prepare:{lcd.CustomName}\n", true);
                        MyIni.Set("Inventory", "gauge", inventoryGauge);
                        MyIni.Set("Inventory", "filter", inventoryFilter);

                        MyIni.Set("Gauge", "on", gauge);
                        MyIni.Set("Gauge", "fullscreen", gaugeFullscreen);
                        MyIni.Set("Gauge", "horizontal", gaugeHorizontal);
                        MyIni.Set("Gauge", "width", gaugeWidth);
                        MyIni.Set("Gauge", "height", gaugeHeight);

                        MyIni.Set("Item", "ore", itemOre);
                        MyIni.Set("Item", "ingot", itemIngot);
                        MyIni.Set("Item", "component", itemComponent);
                        MyIni.Set("Item", "ammo", itemAmmo);

                        MyIni.Set("Drills", "on", drills);
                        MyIni.Set("Drills", "flip_x", drills_flip_x);
                        MyIni.Set("Drills", "flip_y", drills_flip_y);
                        lcd.CustomData = MyIni.ToString();
                    }

                    Drawing drawing = new Drawing(lcd);
                    lcd.ScriptBackgroundColor = Color.Black;
                    Vector2 position = drawing.viewport.Position;

                    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                    if (inventoryGauge)
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
                            case "contains":
                                blocks = inventories.List.Where(inv => inv.CustomName.ToLower().Contains(search.ToLower())).ToList();
                                break;
                            case "equals":
                                blocks = inventories.List.Where(inv => inv.CustomName.ToLower().Equals(search.ToLower())).ToList();
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
                        StyleGauge style = new StyleGauge() {
                            Orientation = gaugeHorizontal ? SpriteOrientation.Horizontal : SpriteOrientation.Vertical,
                            Fullscreen = gaugeFullscreen,
                            Width = gaugeWidth,
                            Height = gaugeHeight
                        };
                        drawing.DrawGauge(position, volumes, maxVolumes, style);
                        if(gaugeHorizontal) position += new Vector2(0, gaugeHeight + 20);
                        else position += new Vector2(gaugeWidth + 20, 0);

                    }

                    if (drills)
                    {
                        float width = 50f;
                        float padding = 4f;
                        float x_min = 0f;
                        float x_max = 0f;
                        float y_min = 0f;
                        float y_max = 0f;
                        bool first = true;
                        blocks = inventories.List.Where(inv => inv is IMyShipDrill).ToList();
                        blocks.ForEach(delegate (IMyTerminalBlock block)
                        {
                            IMyShipDrill drill = (IMyShipDrill)block;
                            if (first || drill.Position.X < x_min) x_min = drill.Position.X;
                            if (first || drill.Position.X > x_max) x_max = drill.Position.X;
                            if (first || drill.Position.Z < y_min) y_min = drill.Position.Z;
                            if (first || drill.Position.Z < y_max) y_max = drill.Position.Z;
                            first = false;
                        });
                        //drawingSurface.WriteText($"X min:{x_min} Y min:{y_min}\n", false);
                        blocks.ForEach(delegate (IMyTerminalBlock block)
                        {
                            IMyShipDrill drill = (IMyShipDrill)block;
                            IMyInventory block_inventory = block.GetInventory(0);
                            long volume = block_inventory.CurrentVolume.RawValue;
                            long maxVolume = block_inventory.MaxVolume.RawValue;
                            float x = drill.Position.X - x_min;
                            float y = drill.Position.Z - y_min;
                            //drawingSurface.WriteText($"X:{x} Y:{y}\n", true);
                            if (drills_flip_x) x = x_max - x_min - x;
                            if (drills_flip_y) y = y_max - y_min - y;
                            //drawingSurface.WriteText($"Volume [{x},{y}]:{volume}/{maxVolume}\n", true);
                            Vector2 position_relative = new Vector2(x * (width + padding), y * (width + padding));
                            DrawDrill(drawing, volume, maxVolume, position + position_relative, width, width);
                        });
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

        private void DrawDrill(Drawing drawing, long volume, long maxVolume, Vector2 position, float width = 80f, float height = 80f)
        {
            // Jauge
            Color color = Color.Gray;
            icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2(width, height),
                Color = color,
                Position = position + new Vector2(10, height / 2 + 30)
            };
            // Jauge quantity
            drawing.AddSprite(icon);
            // attention un long est un entier et la division l'ai aussi
            float ratio = (volume * 1000 / maxVolume);
            float percent = ratio / 1000;
            color = Color.Green;
            if (percent > 0.5) color = Color.Yellow;
            if (percent > 0.75) color = Color.Red;
            float max = 0;
            if(volume > 0) max = 2;
            drawing.AddSprite(icon); icon = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "SquareSimple",
                Size = new Vector2(width - 4, Math.Max(max, height * percent)),
                Color = color,
                Position = position + new Vector2(10 + 2, height / 2 + 30 + ((height - 4) * (1 - percent)) / 2 - 2)
            };
            drawing.AddSprite(icon);
        }
        
        private void DisplayByType(Drawing drawing, Vector2 position, List<string> types)
        {
            int count = -1;
            float height = 80f;
            float width = 3 * height;
            int limit = 5;
            string colorDefault = MyProperty.Get("color", "default");
            int limitDefault = MyProperty.GetInt("Limit", "default");

            foreach (string type in types)
            {
                foreach (KeyValuePair<string, Item> entry in item_list.OrderByDescending(entry => entry.Value.Amount).Where(entry => entry.Value.Type == type))
                {
                    Item item = entry.Value;
                    count++;
                    Vector2 position2 = position + new Vector2(width * (count / limit), height * (count - (count / limit) * limit));
                    // Icon
                    Color color = MyProperty.GetColor("color", item.Name, colorDefault);
                    int limitBar = MyProperty.GetInt("Limit", item.Name, limitDefault);
                    //DisplayIcon(drawing, item, position2, width);
                    StyleIcon style = new StyleIcon()
                    {
                        path = item.Icon,
                        Width = width,
                        Height = height
                    };
                    drawing.DrawGaugeIcon(position2, item.Name, item.Amount, limitBar, style);
                }
            }
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
