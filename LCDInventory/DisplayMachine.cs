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
            public DisplayMachine(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }

            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Machine", "on").ToBoolean(false);
                //filter = MyIni.Get("Machine", "filter").ToString("*");
                machine_refinery = MyIni.Get("Machine", "refinery").ToBoolean(true);
                machine_assembler = MyIni.Get("Machine", "assembler").ToBoolean(true);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Machine", "on", enable);
                //MyIni.Set("Machine", "filter", filter);
                MyIni.Set("Machine", "refinery", machine_refinery);
                MyIni.Set("Machine", "assembler", machine_assembler);
            }

            private void Search()
            {
                //BlockFilter<IMyShipDrill> block_filter = BlockFilter<IMyShipDrill>.Create(parent, filter);
                //drill_inventories = BlockSystem<IMyShipDrill>.SearchByFilter(program, block_filter);

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
                        Width = 300,
                        Height = 80,
                        Padding = new StylePadding(0),
                    };

                    foreach (string type in types)
                    {
                        BlockSystem<IMyProductionBlock> producers = BlockSystem<IMyProductionBlock>.SearchBlocks(DisplayLcd.program, block => block.GetType().Name.Contains(type));
                        int count = 0;
                        producers.ForEach(delegate (IMyProductionBlock block)
                        {
                            Vector2 position2 = position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                            DrawMachine(drawing, position2, block, style);
                            count += 1;
                        });
                        position += new Vector2(0, style.Height) * limit;
                    }
                }

                return position;
            }
            public void DrawMachine(Drawing drawing, Vector2 position, IMyProductionBlock block, Style style)
            {
                float size_icon = style.Height - 10;
                Color color_title = new Color(100, 100, 100, 128);
                Color color_text = new Color(100, 100, 100, 255);
                float RotationOrScale = 0.5f;

                string colorDefault = DisplayLcd.program.MyProperty.Get("color", "default");


                List<Item> items = new List<Item>();
                if (block is IMyAssembler)
                {
                    List<MyProductionItem> productionItems = new List<MyProductionItem>();
                    block.GetQueue(productionItems);
                    if (productionItems.Count > 0)
                    {
                        foreach (MyProductionItem productionItem in productionItems)
                        {
                            string iName = Util.GetName(productionItem);
                            string iType = Util.GetType(productionItem);
                            MyDefinitionId itemDefinitionId = productionItem.BlueprintId;
                            DisplayLcd.program.drawingSurface.WriteText($"SubtypeName:{itemDefinitionId.SubtypeName}\n", true);
                            DisplayLcd.program.drawingSurface.WriteText($"Icon:{iType}/{iName}\n", true);
                            double amount = 0;
                            Double.TryParse(productionItem.Amount.ToString(), out amount);
                            items.Add(new Item()
                            {
                                Name = iName,
                                Type = iType,
                                Amount = amount

                            });
                        }
                    }
                }
                else
                {
                    List<MyInventoryItem> inventoryItems = new List<MyInventoryItem>();
                    block.InputInventory.GetItems(inventoryItems);
                    if (inventoryItems.Count > 0)
                    {
                        foreach (MyInventoryItem inventoryItem in inventoryItems)
                        {
                            string iName = Util.GetName(inventoryItem);
                            string iType = Util.GetType(inventoryItem);
                            double amount = 0;
                            Double.TryParse(inventoryItem.Amount.ToString(), out amount);
                            items.Add(new Item()
                            {
                                Name = iName,
                                Type = iType,
                                Amount = amount
                            });
                        }
                    }
                }

                int loop = 0;
                float x = 0f;
                foreach (Item item in items)
                {
                    if (loop >= 3) break;
                    // icon
                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXTURE,
                        Data = item.Icon,
                        Size = new Vector2(size_icon, size_icon),
                        Color = DisplayLcd.program.MyProperty.GetColor("color", item.Name, colorDefault),
                        Position = position + new Vector2(x, size_icon / 2 + 10)

                    });
                    drawing.AddForm(position + new Vector2(x, 10), SpriteForm.SquareSimple, size_icon, size_icon, new Color(0, 0, 0, 150));

                    if (drawing.Symbol.Keys.Contains(item.Name))
                    {
                        // symbol
                        drawing.AddSprite(new MySprite()
                        {
                            Type = SpriteType.TEXT,
                            Data = drawing.Symbol[item.Name],
                            Color = color_text,
                            Position = position + new Vector2(x, 20),
                            RotationOrScale = RotationOrScale,
                            FontId = drawing.Font,
                            Alignment = TextAlignment.LEFT
                        });
                    }

                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = Util.GetKiloFormat(item.Amount),
                        Color = color_text,
                        Position = position + new Vector2(x, size_icon - 20),
                        RotationOrScale = RotationOrScale,
                        FontId = drawing.Font,
                        Alignment = TextAlignment.LEFT
                    });
                    x += style.Height;
                    loop += 1;
                }

                // Element Name
                MySprite icon = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = block.CustomName,
                    Color = color_title,
                    Position = position + new Vector2(style.Margin.X, 0),
                    RotationOrScale = 0.8f,
                    FontId = drawing.Font,
                    Alignment = TextAlignment.LEFT

                };
                drawing.AddSprite(icon);
            }
        }
    }
}
