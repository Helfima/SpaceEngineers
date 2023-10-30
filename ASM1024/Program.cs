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
using static System.Collections.Specialized.BitVector32;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;

        private Instructions instructions;

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
                    default:
                        drawingSurface.WriteText("Program started", false);
                        instructions.Init();
                        instructions.Start();
                        break;
                }
            }
        }
        private void GetInformation(string filter)
        {
            BlockFilter<IMyTerminalBlock> block_filter = BlockFilter<IMyTerminalBlock>.Create(Me, filter);
            var items = BlockSystem<IMyTerminalBlock>.SearchByFilter(this, block_filter);
            if (items.IsEmpty == false)
            {
                var infos = new StringBuilder();
                var item = items.First;
                var type = item.GetType();
                infos.AppendLine($"Type:{type.Name}");

                List<ITerminalAction> actions = new List<ITerminalAction>();
                item.GetActions(actions);
                actions.Sort(new TerminalActionComparer());
                infos.AppendLine();
                infos.AppendLine("Actions:");
                foreach (var action in actions)
                {
                    infos.AppendLine($"{action.Id}: {action.Name}");
                }

                List<ITerminalProperty> properties = new List<ITerminalProperty>();
                item.GetProperties(properties);
                properties.Sort(new TerminalPropertyComparer());
                infos.AppendLine();
                infos.AppendLine("Properties:");
                foreach (var property in properties)
                {
                    infos.AppendLine($"{property.Id}: {property.TypeName}");
                }

                item.CustomData = infos.ToString();
            }
        }
        void RunContinuousLogic()
        {
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
