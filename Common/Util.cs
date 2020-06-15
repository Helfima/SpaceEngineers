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
        public class Util
        {
            static public string GetKiloFormat(double value)
            {
                double pow = 1.0;
                string suffix = "";
                if (value > 1000.0)
                {
                    int y = int.Parse(Math.Floor(Math.Log10(value) / 3).ToString());
                    suffix = "KMGTPEZY".Substring(y - 1, 1);
                    pow = Math.Pow(10, y * 3);
                }
                return String.Format("{0:0.0}{1}", (value / pow), suffix);

            }

            static public double RadToDeg(float angle)
            {
                return angle * 180 / Math.PI;
            }
            static public double DegToRad(float angle)
            {
                return angle * Math.PI / 180;
            }
            static public string GetType(MyInventoryItem inventory_item)
            {
                return inventory_item.Type.TypeId;
            }

            static public string GetName(MyInventoryItem inventory_item)
            {
                return inventory_item.Type.SubtypeId;
            }
            static public string GetType(MyProductionItem production_item)
            {
                MyDefinitionId itemDefinitionId;
                string subtypeName = production_item.BlueprintId.SubtypeName;
                string typeName = Util.GetName(production_item);
                if ((subtypeName.EndsWith("Rifle") || subtypeName.StartsWith("Welder") || subtypeName.StartsWith("HandDrill") || subtypeName.StartsWith("AngleGrinder"))
                    && MyDefinitionId.TryParse("MyObjectBuilder_PhysicalGunObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
                if (subtypeName.StartsWith("Hydrogen") && MyDefinitionId.TryParse("MyObjectBuilder_GasContainerObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
                if (subtypeName.StartsWith("Oxygen") && MyDefinitionId.TryParse("MyObjectBuilder_OxygenContainerObject", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
                if (subtypeName.EndsWith("Magazine") && MyDefinitionId.TryParse("MyObjectBuilder_AmmoMagazine", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
                if (MyDefinitionId.TryParse("MyObjectBuilder_Component", typeName, out itemDefinitionId)) return itemDefinitionId.TypeId.ToString();
                return production_item.BlueprintId.TypeId.ToString();
            }

            static public string GetName(MyProductionItem production_item)
            {
                string subtypeName = production_item.BlueprintId.SubtypeName;
                if (subtypeName.EndsWith("Component")) subtypeName = subtypeName.Replace("Component", "");
                if (subtypeName.EndsWith("Rifle") || subtypeName.StartsWith("Welder") || subtypeName.StartsWith("HandDrill") || subtypeName.StartsWith("AngleGrinder")) subtypeName = subtypeName + "Item";
                if (subtypeName.EndsWith("Magazine")) subtypeName = subtypeName.Replace("Magazine", "");
                return subtypeName;
            }
        }
    }
}
