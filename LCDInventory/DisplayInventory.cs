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
        public class DisplayInventory
        {
            protected DisplayLcd DisplayLcd;

            private bool enable = false;

            public bool search = true;

            private string filter = "*";

            private bool gauge = true;
            private bool gaugeFullscreen = true;
            private bool gaugeHorizontal = true;
            private float gaugeWidth = 80f;
            private float gaugeHeight = 40f;

            private bool item = true;
            private bool itemOre = true;
            private bool itemIngot = true;
            private bool itemComponent = true;
            private bool itemAmmo = true;

            private BlockSystem<IMyTerminalBlock> inventories = null;
            private Dictionary<string, Item> item_list = new Dictionary<string, Item>();
            public DisplayInventory(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }
            public void Load(MyIni MyIni)
            {
                filter = MyIni.Get("Inventory", "filter").ToString("*");
                enable = MyIni.Get("Inventory", "on").ToBoolean(true);

                gauge = MyIni.Get("Inventory", "gauge_on").ToBoolean(true);
                gaugeFullscreen = MyIni.Get("Inventory", "gauge_fullscreen").ToBoolean(true);
                gaugeHorizontal = MyIni.Get("Inventory", "gauge_horizontal").ToBoolean(true);
                gaugeWidth = MyIni.Get("Inventory", "gauge_width").ToSingle(80f);
                gaugeHeight = MyIni.Get("Inventory", "gauge_height").ToSingle(40f);

                item = MyIni.Get("Inventory", "item_on").ToBoolean(true);
                itemOre = MyIni.Get("Inventory", "item_ore").ToBoolean(true);
                itemIngot = MyIni.Get("Inventory", "item_ingot").ToBoolean(true);
                itemComponent = MyIni.Get("Inventory", "item_component").ToBoolean(true);
                itemAmmo = MyIni.Get("Inventory", "item_ammo").ToBoolean(true);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Inventory", "filter", filter);
                MyIni.Set("Inventory", "on", enable);

                MyIni.Set("Inventory", "gauge_on", gauge);
                MyIni.Set("Inventory", "gauge_fullscreen", gaugeFullscreen);
                MyIni.Set("Inventory", "gauge_horizontal", gaugeHorizontal);
                MyIni.Set("Inventory", "gauge_width", gaugeWidth);
                MyIni.Set("Inventory", "gauge_height", gaugeHeight);

                MyIni.Set("Inventory", "item_on", item);
                MyIni.Set("Inventory", "item_ore", itemOre);
                MyIni.Set("Inventory", "item_ingot", itemIngot);
                MyIni.Set("Inventory", "item_component", itemComponent);
                MyIni.Set("Inventory", "item_ammo", itemAmmo);
            }

            private void Search()
            {
                BlockFilter<IMyTerminalBlock> block_filter = BlockFilter<IMyTerminalBlock>.Create(DisplayLcd.lcd, filter);
                block_filter.HasInventory = true;
                inventories = BlockSystem<IMyTerminalBlock>.SearchByFilter(DisplayLcd.program, block_filter);

                search = false;
            }
            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                if (search) Search();

                if (gauge)
                {
                    position = DisplayGauge(drawing, position);
                }
                if (item)
                {
                    List<string> types = new List<string>();
                    if (itemOre) types.Add(Item.TYPE_ORE);
                    if (itemIngot) types.Add(Item.TYPE_INGOT);
                    if (itemComponent) types.Add(Item.TYPE_COMPONENT);
                    if (itemAmmo) types.Add(Item.TYPE_AMMO);
                    InventoryCount();
                    position = DisplayByType(drawing, position, types);
                }
                return position;
            }

            private Vector2 DisplayGauge(Drawing drawing, Vector2 position)
            {
                long volumes = 0;
                long maxVolumes = 1;
                inventories.ForEach(delegate (IMyTerminalBlock block)
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
                StyleGauge style = new StyleGauge()
                {
                    Orientation = gaugeHorizontal ? SpriteOrientation.Horizontal : SpriteOrientation.Vertical,
                    Fullscreen = gaugeFullscreen,
                    Width = gaugeWidth,
                    Height = gaugeHeight
                };
                drawing.DrawGauge(position, volumes, maxVolumes, style);
                if (gaugeHorizontal) position += new Vector2(0, gaugeHeight + 20);
                else position += new Vector2(gaugeWidth + 20, 0);
                return position;
            }
            private Vector2 DisplayByType(Drawing drawing, Vector2 position, List<string> types)
            {
                int count = 0;
                float height = 80f;
                float width = 3 * height;
                int limit = 5;
                string colorDefault = DisplayLcd.program.MyProperty.Get("color", "default");
                int limitDefault = DisplayLcd.program.MyProperty.GetInt("Limit", "default");

                foreach (string type in types)
                {
                    foreach (KeyValuePair<string, Item> entry in item_list.OrderByDescending(entry => entry.Value.Amount).Where(entry => entry.Value.Type == type))
                    {
                        Item item = entry.Value;
                        Vector2 position2 = position + new Vector2(width * (count / limit), (2 + height) * (count - (count / limit) * limit));
                        // Icon
                        Color color = DisplayLcd.program.MyProperty.GetColor("color", item.Name, colorDefault);
                        int limitBar = DisplayLcd.program.MyProperty.GetInt("Limit", item.Name, limitDefault);
                        //DisplayIcon(drawing, item, position2, width);
                        StyleIcon style = new StyleIcon()
                        {
                            path = item.Icon,
                            Width = width,
                            Height = height,
                            Color = color
                        };
                        drawing.DrawGaugeIcon(position2, item.Name, item.Amount, limitBar, style);
                        count++;
                    }
                }
                if(item_list.Count > limit) return position + new Vector2(0, (2 + height) * limit);
                return position + new Vector2(0, (2 + height) * item_list.Count);
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
    }
}
