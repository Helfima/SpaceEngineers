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
    public interface IReflectionProperty : ITerminalProperty
    {
        string Description { get; }
        object GetValue(object block);
        TValue GetValue<TValue>(object block);
    }
    public class ReflectionProperty<T> : IReflectionProperty where T : class
    {
        public ReflectionProperty(string id, string typeName, Func<T, object> lambdaGet, string description = "")
        {
            this.Id = id;
            this.TypeName = typeName;
            this.Description = description;
            this.lambdaGet = lambdaGet;
        }
        public string Id { get; }

        public string TypeName { get; }
        public string Description { get; }

        protected Func<T, object> lambdaGet;
        public object GetValue(object block)
        {
            return lambdaGet(block as T);
        }
        public TValue GetValue<TValue>(object block)
        {
            return (TValue)lambdaGet(block as T);
        }
    }
}
