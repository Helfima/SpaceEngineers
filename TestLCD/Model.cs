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
        public class Model
        {
            public const string TYPE_ORE = "Ore";
            public const string TYPE_INGOT = "Ingot";
            public const string TYPE_COMPONENT = "Component";

            public Dictionary<string, Resource> resource_list = new Dictionary<string, Resource>();
            public Dictionary<string, Component> component_list = new Dictionary<string, Component>();

            public void AddResourceItem(string name, string symbol, double ore_threshold, double ingot_threshold)
            {
                if (!resource_list.ContainsKey(name)) resource_list.Add(name, new Resource(name, symbol, ore_threshold, ingot_threshold));
            }

            public void AddComponentItem(string name, string symbol, double threshold)
            {
                if (!component_list.ContainsKey(name)) component_list.Add(name, new Component(name, symbol, threshold));
            }

            public void AddAmount(string type, string name, double amount)
            {
                switch (type)
                {
                    case TYPE_ORE:
                        if (resource_list.ContainsKey(name)) ((Resource)resource_list[name]).ore.amount += amount;
                        break;
                    case TYPE_INGOT:
                        if (resource_list.ContainsKey(name)) ((Resource)resource_list[name]).ingot.amount += amount;
                        break;
                    case TYPE_COMPONENT:
                        if (resource_list.ContainsKey(name)) ((Component)component_list[name]).component.amount += amount;
                        break;
                }
            }

            public void ResetAmount()
            {
                foreach (KeyValuePair<string, Resource> pair in resource_list)
                {
                    pair.Value.Reset();
                }
                foreach (KeyValuePair<string, Component> pair in component_list)
                {
                    pair.Value.Reset();
                }
            }

        }

        interface IResource
        {
            void Reset();
        }

        public class Resource : IResource
        {
            public string name;
            public string symbol;
            public ItemStat ore;
            public ItemStat ingot;

            public Resource(string name, string symbol, double ore_threshold, double ingot_threshold)
            {
                this.name = name;
                this.symbol = symbol;
                this.ore = new ItemStat(ore_threshold);
                this.ingot = new ItemStat(ingot_threshold);
            }

            public void Reset()
            {
                ore.amount = 0.0;
                ingot.amount = 0.0;
            }
        }

        public class Component : IResource
        {
            public string name;
            public string symbol;
            public ItemStat component;

            public Component(string name, string symbol, double threshold)
            {
                this.name = name;
                this.symbol = symbol;
                this.component = new ItemStat(threshold);
            }

            public void Reset()
            {
                component.amount = 0.0;
            }
        }

        public class ItemStat
        {
            public double amount = 0.0;
            public double threshold = 0.0;

            public ItemStat(double threshold)
            {
                this.threshold = threshold;
            }
        }
    }
}
