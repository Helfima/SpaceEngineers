using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    // This template is intended for extension classes. For most purposes you're going to want a normal
    // utility class.
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
    internal static class IMyInventoryExtensions
    {
        public static void GetReflectionProperties(this IMyInventory inventory, List<IReflectionProperty> resultList)
        {
            resultList.Add(new ReflectionProperty<IMyInventory>("IsFull", "Boolean", x => (bool)x.IsFull, "Use inventory command"));
            resultList.Add(new ReflectionProperty<IMyInventory>("CurrentMass", "Single", x => { double amount = 0; Double.TryParse(x.CurrentMass.ToString(), out amount); return amount; }, "Use inventory command"));
            resultList.Add(new ReflectionProperty<IMyInventory>("MaxVolume", "Single", x => { double amount = 0; Double.TryParse(x.MaxVolume.ToString(), out amount); return amount; }, "Use inventory command"));
            resultList.Add(new ReflectionProperty<IMyInventory>("CurrentVolume", "Single", x => { double amount = 0; Double.TryParse(x.CurrentVolume.ToString(), out amount); return amount; }, "Use inventory command"));
            resultList.Add(new ReflectionProperty<IMyInventory>("ItemCount", "Single", x => (double)x.ItemCount, "Use inventory command"));
            resultList.Add(new ReflectionProperty<IMyInventory>("VolumeFillFactor", "Single", x => (double)x.VolumeFillFactor, "Use inventory command"));
        }
        public static IReflectionProperty GetReflectionProperty(this IMyInventory inventory, string id)
        {
            List<IReflectionProperty> reflectionProperties = new List<IReflectionProperty>();
            inventory.GetReflectionProperties(reflectionProperties);
            if (reflectionProperties.Count > 0)
            {
                return reflectionProperties.First(x => x.Id == id);
            }
            return null;
        }
        public static object GetReflectionValue(this IMyInventory inventory, string id)
        {
            var property = inventory.GetReflectionProperty(id);
            if (property == null) return null;
            return property.GetValue(inventory);
        }
        public static TValue GetReflectionValue<TValue>(this IMyInventory inventory, string id)
        {
            var property = inventory.GetReflectionProperty(id);
            if (property == null) return default(TValue);
            return property.GetValue<TValue>(inventory);
        }
    }
}
