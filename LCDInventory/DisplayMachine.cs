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
        public class DisplayMachine
        {
            protected DisplayLcd DisplayLcd;

            private bool enable = false;

            public bool search = true;

            private string filter = "*";

            private bool machine_refinery = false;
            private bool machine_assembler = false;

            private int max_loop = 3;
            private int string_len = 20;

            private BlockSystem<IMyProductionBlock> producers;
            private Dictionary<long, Dictionary<string, double>> last_machine_amount = new Dictionary<long, Dictionary<string, double>>();

            public DisplayMachine(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }

            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Machine", "on").ToBoolean(false);
                filter = MyIni.Get("Machine", "filter").ToString("*");
                machine_refinery = MyIni.Get("Machine", "refinery").ToBoolean(true);
                machine_assembler = MyIni.Get("Machine", "assembler").ToBoolean(true);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Machine", "on", enable);
                MyIni.Set("Machine", "filter", filter);
                MyIni.Set("Machine", "refinery", machine_refinery);
                MyIni.Set("Machine", "assembler", machine_assembler);
            }

            private void Search()
            {
                BlockFilter<IMyProductionBlock> block_filter = BlockFilter<IMyProductionBlock>.Create(DisplayLcd.lcd, filter);
                producers = BlockSystem<IMyProductionBlock>.SearchByFilter(DisplayLcd.program, block_filter);

                search = false;
            }

            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                if (search) Search();
                List<string> types = new List<string>();
                int limit = 0;
                if (machine_refinery)
                {
                    types.Add("Refinery");
                    limit += 1;
                }
                if (machine_assembler)
                {
                    types.Add("Assembler");
                    limit += 1;
                }
                limit = 6 / limit;
                if (types.Count > 0)
                {
                    Style style = new Style()
                    {
                        Width = 250,
                        Height = 80,
                        Padding = new StylePadding(0),
                    };

                    foreach (string type in types)
                    {
                        int count = 0;
                        producers.List.Sort(new BlockComparer());
                        producers.ForEach(delegate (IMyProductionBlock block)
                        {
                            if (block.GetType().Name.Contains(type))
                            {
                                Vector2 position2 = position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                                List<Item> items = TraversalMachine(block);
                                DrawMachine(drawing, position2, block, items, style);
                                count += 1;
                            }
                        });
                        position += new Vector2(0, style.Height) * limit;
                    }
                }

                return position;
            }

            public List<Item> TraversalMachine(IMyProductionBlock block)
            {
                int loop = 0;
                List<Item> items = new List<Item>();

                Dictionary<string, double> last_amount;
                if (last_machine_amount.ContainsKey(block.EntityId))
                {
                    last_amount = last_machine_amount[block.EntityId];
                }
                else
                {
                    last_amount = new Dictionary<string, double>();
                    last_machine_amount.Add(block.EntityId, last_amount);
                }

                if (block is IMyAssembler)
                {
                    List<MyProductionItem> productionItems = new List<MyProductionItem>();
                    block.GetQueue(productionItems);
                    if (productionItems.Count > 0)
                    {
                        loop = 0;
                        foreach (MyProductionItem productionItem in productionItems)
                        {
                            if (loop >= max_loop) break;
                            string iName = Util.GetName(productionItem);
                            string iType = Util.GetType(productionItem);
                            string key = String.Format("{0}_{1}", iType, iName);
                            MyDefinitionId itemDefinitionId = productionItem.BlueprintId;
                            double amount = 0;
                            Double.TryParse(productionItem.Amount.ToString(), out amount);

                            int variance = 2;
                            if (last_amount.ContainsKey(key))
                            {
                                if (last_amount[key] < amount) variance = 1;
                                if (last_amount[key] > amount) variance = 3;
                                last_amount[key] = amount;
                            }
                            else
                            {
                                variance = 1;
                                last_amount.Add(key, amount);
                            }

                            items.Add(new Item()
                            {
                                Name = iName,
                                Type = iType,
                                Amount = amount,
                                Variance = variance
                            });
                            loop++;
                        }
                    }
                }
                else
                {
                    List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();
                    block.InputInventory.GetItems(inventoryItems);
                    if (inventoryItems.Count > 0)
                    {
                        loop = 0;
                        foreach (MyInventoryItem inventoryItem in inventoryItems)
                        {
                            if (loop >= max_loop) break;
                            string iName = Util.GetName(inventoryItem);
                            string iType = Util.GetType(inventoryItem);
                            string key = String.Format("{0}_{1}", iType, iName);
                            double amount = 0;
                            Double.TryParse(inventoryItem.Amount.ToString(), out amount);

                            int variance = 2;
                            if (last_amount.ContainsKey(key))
                            {
                                if (last_amount[key] < amount) variance = 1;
                                if (last_amount[key] > amount) variance = 3;
                                last_amount[key] = amount;
                            }
                            else
                            {
                                variance = 1;
                                last_amount.Add(key, amount);
                            }

                            items.Add(new Item()
                            {
                                Name = iName,
                                Type = iType,
                                Amount = amount,
                                Variance = variance
                            });
                            loop++;
                        }
                    }
                }
                last_machine_amount[block.EntityId] = last_amount;
                return items;
            }
            public void DrawMachine(Drawing drawing, Vector2 position, IMyProductionBlock block, List<Item> items, Style style)
            {
                float size_icon = style.Height - 10;
                Color color_title = new Color(100, 100, 100, 128);
                Color color_text = new Color(100, 100, 100, 255);
                float RotationOrScale = 0.5f;
                float cell_spacing = 10f;

                float form_width = style.Width - 5;
                float form_height = style.Height - 5;

                string colorDefault = DisplayLcd.program.MyProperty.Get("color", "default");

                float x = 0f;

                drawing.AddForm(position + new Vector2(0, 0), SpriteForm.SquareSimple, form_width, form_height, new Color(5, 5, 5, 125));

                foreach (Item item in items)
                {
                    
                    // icon
                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = item.Icon,
                        Size = new Vector2(size_icon, size_icon),
                        Color = DisplayLcd.program.MyProperty.GetColor("color", item.Name, colorDefault),
                        Position = position + new Vector2(x, size_icon / 2 + cell_spacing)

                    });

                    if (drawing.Symbol.Keys.Contains(item.Name))
                    {
                        // symbol
                        Vector2 positionSymbol = position + new Vector2(x, 20);
                        drawing.AddForm(positionSymbol, SpriteForm.SquareSimple, size_icon, 15f, new Color(10, 10, 10, 200));
                        drawing.AddSprite(new MySprite()
                        {
                            Type = SpriteType.TEXT,
                            Data = drawing.Symbol[item.Name],
                            Color = color_text,
                            Position = positionSymbol,
                            RotationOrScale = RotationOrScale,
                            FontId = drawing.Font,
                            Alignment = TextAlignment.LEFT
                        });
                    }

                    // Quantity
                    Vector2 positionQuantity = position + new Vector2(x, size_icon - 12);
                    Color mask_color = new Color(0, 0, 20, 200);
                    if (item.Variance == 2) mask_color = new Color(20, 0, 0, 200);
                    if (item.Variance == 3) mask_color = new Color(0, 20, 0, 200);
                    drawing.AddForm(positionQuantity, SpriteForm.SquareSimple, size_icon, 15f, mask_color);
                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = Util.GetKiloFormat(item.Amount),
                        Color = color_text,
                        Position = positionQuantity,
                        RotationOrScale = RotationOrScale,
                        FontId = drawing.Font,
                        Alignment = TextAlignment.LEFT
                    });
                    x += style.Height;
                }

                // Element Name
                MySprite icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = Util.CutString(block.CustomName, string_len),
                    Color = color_title,
                    Position = position + new Vector2(style.Margin.X, 0),
                    RotationOrScale = 0.6f,
                    FontId = drawing.Font,
                    Alignment = TextAlignment.LEFT

                };
                drawing.AddSprite(icon);
            }
        }
    }
}
