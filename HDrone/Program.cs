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
    partial class Program : MyGridProgram
    {
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        MyCommandLine commandLine = new MyCommandLine();
        private IMyTextSurface drawingSurface;
        private StateMachine machine_state = StateMachine.Stopped;

        private BlockSystem<IMyTextPanel> lcds = null;

        private bool search=true;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
            
        }

        private void Init()
        {
        }

        public void Save()
        {

        }

        private void Search()
        {
            BlockFilter<IMyTextPanel> block_filter = BlockFilter<IMyTextPanel>.Create(Me, "C:TEST");
            lcds = BlockSystem<IMyTextPanel>.SearchByFilter(this, block_filter);
            search = false;
        }
        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & CommandUpdate) != 0)
            {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update100) != 0)
            {
                RunContinuousLogic();
            }
        }

        private void RunCommand(string argument)
        {
            Init();
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);
                if (command != null) command = command.Trim().ToLower();
                switch (command)
                {
                    default:
                        search = true;
                        Search();
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            if(search) Search();
            WriteText($"LCD found: {(lcds != null ? lcds.List.Count : 0)}", false);
            BlockFilter<IMyThrust> block_filter = BlockFilter<IMyThrust>.Create(Me, "C:Up");
            BlockSystem<IMyThrust> thrust = BlockSystem<IMyThrust>.SearchByFilter(this, block_filter);
            if (!thrust.IsEmpty) {
                ParserInfo parser = new ParserInfo(this);
                IMyThrust block = thrust.First;
                parser.ParserTitle(block);
                parser.ParserTerminalBlock(block);
                parser.ParserThrust(block);
                parser.ParserCubeBlock(block);
            }
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{machine_state}", false);
        }


        public void WriteText(string message, bool append)
        {
            message += "\n";
            drawingSurface.WriteText(message, append);
            if (lcds != null) lcds.ForEach(delegate (IMyTextPanel block)
            {
                block.WriteText(message, append);
            });
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
