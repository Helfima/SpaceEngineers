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

    
    internal static class IMyTerminalBlockExtensions
    {
        public static void GetReflectionProperties(this IMyTerminalBlock block ,List<IReflectionProperty> resultList)
        {
            if (block is IMyMotorStator)
            {
                resultList.Add(new ReflectionProperty<IMyMotorStator>("Angle", "Single", x => (double)x.Angle * 180 / Math.PI, "Value in degres"));
            }
            if (block is IMyPistonBase)
            {
                resultList.Add(new ReflectionProperty<IMyPistonBase>("Position", "Single", x => (double)x.CurrentPosition));
            }
            if (block is IMyShipMergeBlock)
            {
                resultList.Add(new ReflectionProperty<IMyShipMergeBlock>("IsConnected", "Boolean", x => (bool)x.IsConnected));
            }
            if (block is IMySensorBlock)
            {
                resultList.Add(new ReflectionProperty<IMySensorBlock>("IsActive", "Boolean", x => (bool)x.IsActive));
            }
            if (block is IMyLandingGear)
            {
                resultList.Add(new ReflectionProperty<IMyLandingGear>("LockMode", "Single", x => (double)x.LockMode, "0:Unlocked 1:ReadyToLock 2:Locked"));
            }
        }
        public static IReflectionProperty GetReflectionProperty(this IMyTerminalBlock block, string id)
        {
            List<IReflectionProperty> reflectionProperties = new List<IReflectionProperty>();
            block.GetReflectionProperties(reflectionProperties);
            if (reflectionProperties.Count > 0)
            {
                return reflectionProperties.First(x => x.Id == id);
            }
            return null;
        }
        public static object GetReflectionValue(this IMyTerminalBlock block, string id)
        {
            var property = block.GetReflectionProperty(id);
            if(property == null) return null;
            return property.GetValue(block);
        }
        public static TValue GetReflectionValue<TValue>(this IMyTerminalBlock block, string id)
        {
            var property = block.GetReflectionProperty(id);
            if (property == null) return default(TValue);
            return property.GetValue<TValue>(block);
        }
    }
}
