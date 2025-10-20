using System;
using System.Collections.Generic;
using System.Linq;
using AppsInventory.Common;
using AppsInventory.Extensions;
using AppsInventory.Surfaces;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
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
    [MyTextSurfaceScript("Helfima_TSSCargo", "Inventory Cargo")]
    public class TSSCargo : TSSBase
    {
        public TSSCargo(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.PropertiesSection = "Drills";
        }

        private Dictionary<string, Item> item_list = new Dictionary<string, Item>();
        private Dictionary<string, double> last_amount = new Dictionary<string, double>();

        protected override void OnDraw(FrameDrawing frame)
        {
            if (this.blocks == null || this.blocks.Count == 0) return;
            if (gauge)
            {
                DisplayGauge(frame);
            }
            last_amount.Clear();
            foreach (KeyValuePair<string, Item> entry in item_list)
            {
                last_amount.Add(entry.Key, entry.Value.Amount);
            }

            InventoryCount();
            DisplayByType(frame);
        }
        private List<IMyTerminalBlock> blocks;
        protected override void OnSearch()
        {
            if (TerminalBlock != null)
            {
                BlockFilter<IMyTerminalBlock> plot_filter = BlockFilter<IMyTerminalBlock>.Create(TerminalBlock, filter);
                this.blocks = TerminalBlock.SearchBlocks(plot_filter);
                this.TerminalBlock.GetDetailedInfo().AppendLine($"blocks: {this.blocks.Count}");
            }
        }
        private bool IsValidItem(Item item)
        {
            if (item.IsAmmo) return item.IsAmmo == this.itemAmmo;
            if (item.IsComponent) return item.IsComponent == this.itemComponent;
            if (item.IsIngot) return item.IsIngot == this.itemIngot;
            if (item.IsOre) return item.IsOre == this.itemOre;
            if (item.IsTool) return item.IsTool == this.itemTool;
            if (item.IsOther) return item.IsOther == this.itemOther;
            return true;
        }
        private void InventoryCount()
        {
            item_list.Clear();
            foreach (IMyTerminalBlock block in blocks)
            {

                for (int i = 0; i < block.InventoryCount; i++)
                {
                    IMyInventory block_inventory = block.GetInventory(i);
                    List<VRage.Game.ModAPI.Ingame.MyInventoryItem> items = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
                    block_inventory.GetItems(items);
                    foreach (VRage.Game.ModAPI.Ingame.MyInventoryItem block_item in items)
                    {
                        string name = Util.GetName(block_item);
                        string type = Util.GetType(block_item);
                        double amount = 0;
                        string key = String.Format("{0}_{1}", type, name);
                        Double.TryParse(block_item.Amount.ToString(), out amount);
                        Item item = Item.Parse(block_item);

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
        private int GetLimit(FrameDrawing frame, float itemSize, float cellSpacing)
        {
            int limit = 5;
            if (gauge && gaugeHorizontal) { limit = (int)Math.Floor((frame.Parent.Viewport.Height - (gaugeHeight + topPadding) * scale) / (itemSize + cellSpacing)); }
            else { limit = (int)Math.Floor((frame.Parent.Viewport.Height - topPadding * scale) / (itemSize + cellSpacing)); }
            return Math.Max(limit, 1);
        }
        private void DisplayByType(FrameDrawing frame)
        {
            int count = 0;
            float height = itemSize;
            float width = 2.5f * itemSize;
            float delta_width = width * scale;
            float delta_height = height * scale;
            int limit = GetLimit(frame, delta_height, cellSpacing);

            foreach (KeyValuePair<string, Item> entry in item_list.OrderByDescending(entry => entry.Value.Amount).Where(entry => IsValidItem(entry.Value)))
            {
                Item item = entry.Value;
                Vector2 position2 = frame.Position + new Vector2((cellSpacing + delta_width) * (count / limit), (cellSpacing + delta_height) * (count - (count / limit) * limit));
                // Icon
                Color color = Properties.GetColor("color", item.Name, this.colorDefault);
                int limitBar = Properties.GetInt("Limit", item.Name, this.limitDefault);
                //DisplayIcon(drawing, item, position2, width);
                StyleIcon style = new StyleIcon()
                {
                    path = item.Icon,
                    Width = width,
                    Height = height,
                    Color = color,
                    Thresholds = this.ItemThresholds,
                    ColorSoftening = .6f
                };
                style.Scale(scale);
                int variance = 2;
                //DisplayLcd.program.drawingSurface.WriteText($"variance:{entry.Key}?{last_amount.ContainsKey(entry.Key)}\n", true);
                if (last_amount.ContainsKey(entry.Key))
                {
                    if (last_amount[entry.Key] < item.Amount) variance = 1;
                    if (last_amount[entry.Key] > item.Amount) variance = 3;
                }
                else
                {
                    variance = 1;
                }
                frame.DrawGaugeIcon(position2, item.Name, item.Amount, limitBar, style, variance);
                count++;
            }
            if (item_list.Count > limit) frame.Position += new Vector2(0, (cellSpacing * scale + height) * limit);
            frame.Position += new Vector2(0, (cellSpacing * scale + height) * item_list.Count);
        }

        private void DisplayGauge(FrameDrawing frame)
        {
            long volumes = 0;
            long maxVolumes = 1;
            this.blocks.ForEach(delegate (IMyTerminalBlock block)
            {
                for (int i = 0; i < block.InventoryCount; i++)
                {
                    IMyInventory block_inventory = block.GetInventory(i);
                    long volume = block_inventory.CurrentVolume.RawValue;
                    volumes += volume;
                    long maxVolume = block_inventory.MaxVolume.RawValue;
                    maxVolumes += maxVolume;
                }
            });
            StyleGauge style = new StyleGauge()
            {
                Orientation = gaugeHorizontal ? SpriteOrientation.Horizontal : SpriteOrientation.Vertical,
                Fullscreen = gaugeFullscreen,
                Width = gaugeWidth,
                Height = gaugeHeight,
                Thresholds = this.ChestThresholds,
                ColorSoftening = .6f
            };
            style.Scale(scale);
            frame.Position = frame.DrawGauge(frame.Position, volumes, maxVolumes, style);
            if (gaugeHorizontal)
            {
                frame.Position += new Vector2(0, 2 * cellSpacing * scale);
            }
        }
        private float scale = 1f;

        private string filter = "*";

        private bool gauge = true;
        private bool gaugeFullscreen = true;
        private bool gaugeHorizontal = true;
        private float gaugeWidth = 80f;
        private float gaugeHeight = 40f;

        private bool item = true;
        private float itemSize = 80f;
        private bool itemOre = true;
        private bool itemIngot = true;
        private bool itemComponent = true;
        private bool itemAmmo = true;
        private bool itemTool = true;
        private bool itemOther = true;

        private float topPadding = 10f;
        private float leftPadding = 10f;
        private float cellSpacing = 2f;
        public GaugeThresholds ItemThresholds { get; set; }
        public GaugeThresholds ChestThresholds { get; set; }
        private void LoadThresholds()
        {
            ItemThresholds = Properties.LoadThresholds("ItemThresholds", false);
            ChestThresholds = Properties.LoadThresholds("ChestThresholds", false);
            if (ItemThresholds == null)
            {
                ItemThresholds = new GaugeThresholds();
                ItemThresholds.Thresholds.Add(new GaugeThreshold(0f, new Color(180, 0, 0, 128)));
                ItemThresholds.Thresholds.Add(new GaugeThreshold(0.25f, new Color(180, 130, 0, 128)));
                ItemThresholds.Thresholds.Add(new GaugeThreshold(0.50f, Color.Green));
                ItemThresholds.Thresholds.Add(new GaugeThreshold(1f, new Color(0, 0, 180, 128)));
            }
            if (ChestThresholds == null)
            {
                ChestThresholds = new GaugeThresholds();
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0f, Color.Green));
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0.50f, new Color(180, 130, 0, 128)));
                ChestThresholds.Thresholds.Add(new GaugeThreshold(0.75f, new Color(180, 0, 0, 128)));
            }
        }
        string colorDefault;
        int limitDefault;
        protected override void DataLoad()
        {
            filter = Properties.Get("Inventory", "filter", "*");
            scale = Properties.GetSingle("Inventory", "scale", 1f);

            gauge = Properties.GetBoolean("Inventory", "gauge_on", true);
            gaugeFullscreen = Properties.GetBoolean("Inventory", "gauge_fullscreen", true);
            gaugeHorizontal = Properties.GetBoolean("Inventory", "gauge_horizontal", true);
            gaugeWidth = Properties.GetSingle("Inventory", "gauge_width", 80f);
            gaugeHeight = Properties.GetSingle("Inventory", "gauge_height", 40f);

            item = Properties.GetBoolean("Inventory", "item_on", true);
            itemSize = Properties.GetSingle("Inventory", "item_size", 80f);

            itemOre = Properties.GetBoolean("Inventory", "item_ore", true);
            itemIngot = Properties.GetBoolean("Inventory", "item_ingot", true);
            itemComponent = Properties.GetBoolean("Inventory", "item_component", true);
            itemAmmo = Properties.GetBoolean("Inventory", "item_ammo", true);
            itemTool = Properties.GetBoolean("Inventory", "item_tool", true);
            itemOther = Properties.GetBoolean("Inventory", "item_other", true);

            limitDefault = Properties.GetInt("Limit", "default", 1000);
            colorDefault = Properties.Get("Color", "default", "128,128,128,255");
            LoadThresholds();
        }

        protected override void DataSave()
        {
            Properties.Set("Inventory", "filter", filter);
            Properties.Set("Inventory", "scale", scale);

            Properties.Set("Inventory", "gauge_on", gauge);
            Properties.Set("Inventory", "gauge_fullscreen", gaugeFullscreen);
            Properties.Set("Inventory", "gauge_horizontal", gaugeHorizontal);
            Properties.Set("Inventory", "gauge_width", gaugeWidth);
            Properties.Set("Inventory", "gauge_height", gaugeHeight);

            Properties.Set("Inventory", "item_on", item);
            Properties.Set("Inventory", "item_size", itemSize);

            Properties.Set("Inventory", "item_ore", itemOre);
            Properties.Set("Inventory", "item_ingot", itemIngot);
            Properties.Set("Inventory", "item_component", itemComponent);
            Properties.Set("Inventory", "item_ammo", itemAmmo);
            Properties.Set("Inventory", "item_tool", itemTool);
            Properties.Set("Inventory", "item_other", itemOther);

            Properties.Set("Limit", "Cobalt", "1000");
            Properties.Set("Limit", "Iron", "100000");
            Properties.Set("Limit", "Gold", "1000");
            Properties.Set("Limit", "Platinum", "1000");
            Properties.Set("Limit", "Silver", "1000");

            Properties.Set("Color", "Cobalt", "000,080,080,255");
            Properties.Set("Color", "Gold", "255,153,000,255");
            Properties.Set("Color", "Ice", "040,130,130,255");
            Properties.Set("Color", "Iron", "040,040,040,255");
            Properties.Set("Color", "Nickel", "110,080,080,255");
            Properties.Set("Color", "Platinum", "120,150,120,255");
            Properties.Set("Color", "Silicon", "150,150,150,255");
            Properties.Set("Color", "Silver", "120,120,150,255");
            Properties.Set("Color", "Stone", "120,040,000,200");
            Properties.Set("Color", "Uranium", "040,130,000,200");

            Properties.Save();
        }
    }
}
