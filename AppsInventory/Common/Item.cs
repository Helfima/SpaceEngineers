using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;
using VRage.Game;

namespace AppsInventory.Common
{
    public class Item : IComparable<Item>
    {
        public const string TYPE_ORE = "MyObjectBuilder_Ore";
        public const string TYPE_INGOT = "MyObjectBuilder_Ingot";
        public const string TYPE_COMPONENT = "MyObjectBuilder_Component";
        public const string TYPE_AMMO = "MyObjectBuilder_AmmoMagazine";

        public MyDefinitionId Definition { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Double Amount { get; set; }
        public int Variance { get; set; }
        public bool IsOre { get; set; }
        public bool IsIngot { get; set; }
        public bool IsComponent { get; set; }
        public bool IsTool { get; set; }
        public bool IsAmmo { get; set; }
        public bool IsOther { get; set; }
        public string Sprite { get; set; }
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
        public static Item Parse(MyInventoryItem inventoryItem)
        {
            var item = new Item();
            item.Type = inventoryItem.Type.TypeId;
            item.Name = inventoryItem.Type.SubtypeId;
            double amount = 0;
            Double.TryParse(inventoryItem.Amount.ToString(), out amount);
            item.Amount = amount;
            item.Sprite = String.Format("{0}/{1}", inventoryItem.Type.TypeId, inventoryItem.Type.SubtypeId);
            var itemInfo = inventoryItem.Type.GetItemInfo();
            item.IsOre = itemInfo.IsOre;
            item.IsIngot = itemInfo.IsIngot;
            item.IsComponent = itemInfo.IsComponent;
            item.IsTool = itemInfo.IsTool;
            item.IsAmmo = itemInfo.IsAmmo;
            item.IsOther = item.IsOre == false && item.IsIngot == false &&
                    item.IsComponent == false && item.IsTool == false && item.IsAmmo == false;
            return item;
        }
        public static Item Parse(MyProductionItem productionItem)
        {
            var item = new Item();
            string subtypeName = Util.CleanSubtypeName(productionItem.BlueprintId.SubtypeName);
            item.Name = subtypeName;
            if (subtypeName.EndsWith("Rifle") || subtypeName.StartsWith("Welder") || subtypeName.StartsWith("HandDrill") || subtypeName.StartsWith("AngleGrinder")) item.IsAmmo = true;
            item.IsOther = item.IsOre == false && item.IsIngot == false &&
                    item.IsComponent == false && item.IsTool == false && item.IsAmmo == false;
            return item;
        }
    }
}
