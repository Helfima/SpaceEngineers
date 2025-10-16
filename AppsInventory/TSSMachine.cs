using System;
using System.Collections.Generic;
using AppsInventory.Common;
using AppsInventory.Extensions;
using AppsInventory.Surfaces;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.WorldEnvironment;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory
{
    [MyTextSurfaceScript("Helfima_TSSMachine", "Inventory Machine")]
    public class TSSMachine : TSSBase
    {
        public TSSMachine(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.PropertiesSection = "Machine";
        }

        private List<IMyProductionBlock> blocks;
        protected override void OnSearch()
        {
            if (TerminalBlock != null)
            {
                BlockFilter<IMyProductionBlock> plot_filter = BlockFilter<IMyProductionBlock>.Create(TerminalBlock, filter);
                this.blocks = TerminalBlock.SearchBlocks(plot_filter);
                this.TerminalBlock.GetDetailedInfo().AppendLine($"blocks: {this.blocks.Count}");
            }
        }
        protected override void OnDraw(FrameDrawing frame)
        {
            if (this.blocks == null || this.blocks.Count == 0) return;
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
                    blocks.Sort(new BlockComparer());
                    foreach (var block in blocks)
                    {
                        if (block.GetType().Name.Contains(type))
                        {
                            Vector2 position2 = frame.Position + new Vector2(style.Width * (count / limit), style.Height * (count - (count / limit) * limit));
                            List<Item> items = TraversalMachine(block);
                            DrawMachine(frame, position2, block, items, style);
                            count += 1;
                        }
                    }
                    frame.Position += new Vector2(0, style.Height) * limit;
                }
            }
        }
        private Dictionary<long, Dictionary<string, double>> last_machine_amount = new Dictionary<long, Dictionary<string, double>>();
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
                List<Sandbox.ModAPI.Ingame.MyProductionItem> productionItems = new List<Sandbox.ModAPI.Ingame.MyProductionItem>();
                block.GetQueue(productionItems);
                if (productionItems.Count > 0)
                {
                    loop = 0;
                    foreach (Sandbox.ModAPI.Ingame.MyProductionItem productionItem in productionItems)
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
                            Variance = variance,
                            Definition = productionItem.BlueprintId
                        });
                        loop++;
                    }
                }
            }
            else
            {
                List<VRage.Game.ModAPI.Ingame.MyInventoryItem> inventoryItems = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
                block.InputInventory.GetItems(inventoryItems);
                if (inventoryItems.Count > 0)
                {
                    loop = 0;
                    foreach (VRage.Game.ModAPI.Ingame.MyInventoryItem inventoryItem in inventoryItems)
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
                        Item item = Item.Parse(inventoryItem);
                        item.Variance = variance;
                        items.Add(item);
                        loop++;
                    }
                }
            }
            last_machine_amount[block.EntityId] = last_amount;
            return items;
        }
        public void DrawMachine(FrameDrawing frame, Vector2 position, IMyProductionBlock block, List<Item> items, Style style)
        {
            float size_icon = style.Height - 10;
            Color color_title = new Color(100, 100, 100, 128);
            Color color_text = new Color(100, 100, 100, 255);
            float RotationOrScale = 0.5f;
            float cell_spacing = 10f;

            float form_width = style.Width - 5;
            float form_height = style.Height - 5;

            string colorDefault = Properties.Get("color", "default");

            float x = 0f;

            frame.AddForm(position + new Vector2(0, 0), SpriteForm.SquareSimple, form_width, form_height, new Color(5, 5, 5, 125));

            foreach (Item item in items)
            {
                string sprite = item.Icon;
                if (item.Name.StartsWith("Seeds"))
                {
                    var sprite_name = item.Name.Replace("Seeds_", "");
                    if (frame.Parent.Sprites_seed.ContainsKey(sprite_name))
                    {
                        sprite = frame.Parent.Sprites_seed[sprite_name];
                    }
                }
                else if (item.Name.StartsWith("Spores"))
                {
                    var sprite_name = item.Name.Replace("Spores_", "");
                    if (frame.Parent.Sprites_seed.ContainsKey(sprite_name))
                    {
                        sprite = frame.Parent.Sprites_seed[sprite_name];
                    }
                }
                else
                {
                    if (frame.Parent.Sprites_ammo.ContainsKey(item.Name))
                    {
                        sprite = frame.Parent.Sprites_ammo[item.Name];
                    }
                    else if (frame.Parent.Sprites_component.ContainsKey(item.Name))
                    {
                        sprite = frame.Parent.Sprites_component[item.Name];
                    }
                    else if (frame.Parent.Sprites_tool.ContainsKey(item.Name))
                    {
                        sprite = frame.Parent.Sprites_tool[item.Name];
                    }
                    else if (frame.Parent.Sprites_other.ContainsKey(item.Name))
                    {
                        sprite = frame.Parent.Sprites_other[item.Name];
                    }
                }
                // icon
                frame.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = sprite,
                    Size = new Vector2(size_icon, size_icon),
                    Color = Properties.GetColor("color", item.Name, colorDefault),
                    Position = position + new Vector2(x, size_icon / 2 + cell_spacing)

                });

                if (frame.Parent.Symbol.ContainsKey(item.Name))
                {
                    // symbol
                    Vector2 positionSymbol = position + new Vector2(x, 20);
                    frame.AddForm(positionSymbol, SpriteForm.SquareSimple, size_icon, 15f, new Color(10, 10, 10, 200));
                    frame.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = frame.Parent.Symbol[item.Name],
                        Color = color_text,
                        Position = positionSymbol,
                        RotationOrScale = RotationOrScale,
                        FontId = frame.Parent.Font,
                        Alignment = TextAlignment.LEFT
                    });
                }

                // Quantity
                Vector2 positionQuantity = position + new Vector2(x, size_icon - 12);
                Color mask_color = new Color(0, 0, 20, 200);
                if (item.Variance == 2) mask_color = new Color(20, 0, 0, 200);
                if (item.Variance == 3) mask_color = new Color(0, 20, 0, 200);
                frame.AddForm(positionQuantity, SpriteForm.SquareSimple, size_icon, 15f, mask_color);
                frame.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = Util.GetKiloFormat(item.Amount),
                    Color = color_text,
                    Position = positionQuantity,
                    RotationOrScale = RotationOrScale,
                    FontId = frame.Parent.Font,
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
                FontId = frame.Parent.Font,
                Alignment = TextAlignment.LEFT

            };
            frame.AddSprite(icon);
        }
        string colorDefault;
        int limitDefault;
        private string filter = "*";
        private bool machine_refinery = false;
        private bool machine_assembler = false;

        private int max_loop = 3;
        private int string_len = 20;

        protected override void DataLoad()
        {
            Properties.Load();

            filter = Properties.Get("Machine", "filter", "*");
            machine_refinery = Properties.GetBoolean("Machine", "refinery", true);
            machine_assembler = Properties.GetBoolean("Machine", "assembler", true);

            limitDefault = Properties.GetInt("Limit", "default", 1000);
            colorDefault = Properties.Get("Color", "default", "128,128,128,255");
        }

        protected override void DataSave()
        {
            Properties.Set("Machine", "filter", filter);
            Properties.Set("Machine", "refinery", machine_refinery);
            Properties.Set("Machine", "assembler", machine_assembler);
            Properties.Save();
        }
    }
}
