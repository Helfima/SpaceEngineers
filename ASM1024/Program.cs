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
    partial class Program : MyGridProgram
    {
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;

        private Instructions instructions;
        private string version = "0.1";

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;

            instructions = new Instructions(this);

            Init();
        }

        private void Init()
        {
            
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & CommandUpdate) != 0)
            {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update10) != 0)
            {
                RunContinuousLogic();
            }
        }

        private void RunCommand(string argument)
        {
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "infos":
                        GetInformation(commandLine.Argument(1));
                        break;
                    case "execute":
                        instructions.Init();
                        instructions.ExecuteLabel(commandLine.Argument(1));
                        break;
                    case "reset":
                        Init();
                        break;
                    case "prefix":
                        RenamePrefix(commandLine.Argument(1));
                        break;
                    case "unprefix":
                        UnRenamePrefix(commandLine.Argument(1));
                        break;
                    default:
                        drawingSurface.WriteText("Program started", false);
                        instructions.Init();
                        instructions.Start();
                        break;
                }
            }
        }
        private void RenamePrefix(string tag)
        {
            BlockSystem<IMyTerminalBlock> blocks = BlockSystem<IMyTerminalBlock>.SearchBlocks(this);
            blocks.ForEach(delegate (IMyTerminalBlock block)
            {
                if (!block.CustomName.StartsWith(tag))
                {
                    block.CustomName = tag + " " + block.CustomName;
                }
            });

        }
        private void UnRenamePrefix(string tag)
        {
            BlockSystem<IMyTerminalBlock> blocks = BlockSystem<IMyTerminalBlock>.SearchBlocks(this);
            blocks.ForEach(delegate (IMyTerminalBlock block)
            {
                if (block.CustomName.StartsWith(tag))
                {
                    block.CustomName = block.CustomName.Replace(tag + " ", "");
                }
            });

        }
        private void GetInformation(string filter)
        {
            BlockFilter<IMyTerminalBlock> block_filter = BlockFilter<IMyTerminalBlock>.Create(Me, filter);
            var items = BlockSystem<IMyTerminalBlock>.SearchByFilter(this, block_filter);
            if (items.IsEmpty == false)
            {
                foreach (var item in items.List)
                {
                    var infos = new StringBuilder();
                    var type = item.GetType();
                    infos.AppendLine($"# {type.Name}");

                    List<ITerminalProperty> properties = new List<ITerminalProperty>();
                    item.GetProperties(properties);
                    properties.Sort(new TerminalPropertyComparer());
                    infos.AppendLine();
                    infos.AppendLine("## Properties:");
                    foreach (var property in properties)
                    {
                        infos.AppendLine($"* [RW] {property.Id}: {property.TypeName}");
                    }

                    List<IReflectionProperty> reflectionProperties = new List<IReflectionProperty>();
                    item.GetReflectionProperties(reflectionProperties);
                    if (reflectionProperties.Count > 0)
                    {
                        properties.Sort(new TerminalPropertyComparer());
                        infos.AppendLine();
                        infos.AppendLine("## Reflection Properties:");
                        foreach (var property in reflectionProperties)
                        {
                            var bindingFlags = "[R ]";
                            if (property.BindingFlags == ReflectionBindingFlags.ReadWrite)
                            {
                                bindingFlags = "[RW]";
                            }
                            infos.AppendLine($"* {bindingFlags} {property.Id}: {property.TypeName} {property.Description}");
                        }
                    }

                    List<ITerminalAction> actions = new List<ITerminalAction>();
                    item.GetActions(actions);
                    actions.Sort(new TerminalActionComparer());
                    infos.AppendLine();
                    infos.AppendLine("## Actions:");
                    foreach (var action in actions)
                    {
                        infos.AppendLine($"* [W] {action.Id}: {action.Name}");
                    }
                    item.CustomData = infos.ToString();
                }
            }
        }
        void RunContinuousLogic()
        {
            if(instructions.FirstStarted == false)
            {
                Echo($"Version {version}");
                instructions.Init();
                instructions.Start();
            }
            instructions.Execute();
        }

        public enum StateMachine
        {
            Stopped,
            Traking,
            Running,
            Waitting
        }
    }
}
