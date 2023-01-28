using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenInventory
{
    public class Item : IComparable<Item>
    {
        public const string TYPE_ORE = "MyObjectBuilder_Ore";
        public const string TYPE_INGOT = "MyObjectBuilder_Ingot";
        public const string TYPE_COMPONENT = "MyObjectBuilder_Component";
        public const string TYPE_AMMO = "MyObjectBuilder_AmmoMagazine";

        public string Name;
        public string Type;
        public Double Amount;
        public int Variance;

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
