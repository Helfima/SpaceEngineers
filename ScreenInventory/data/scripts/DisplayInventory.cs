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

using IMyTextSurface = Sandbox.ModAPI.Ingame.IMyTextSurface;
using IMyCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;
using IMyEntity = VRage.ModAPI.IMyEntity;

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
            Search();
            //InventoryCount();
            Draw();
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
}
