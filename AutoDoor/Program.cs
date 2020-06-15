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

        BlockSystem<IMyDoor> doors = null;
        BlockSystem<IMySensorBlock> sensors = null;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Search();
        }

        private void Init()
        {
        }

        public void Save()
        {

        }

        private void Search()
        {
            BlockFilter<IMyDoor> block_filter1 = BlockFilter<IMyDoor>.Create(Me, "*");
            doors = BlockSystem<IMyDoor>.SearchByFilter(this, block_filter1);

            BlockFilter<IMySensorBlock> block_filter2 = BlockFilter<IMySensorBlock>.Create(Me, "*");
            sensors = BlockSystem<IMySensorBlock>.SearchByFilter(this, block_filter2);

        }

        public void Main(string argument, UpdateType updateType)
        {
            drawingSurface.WriteText($"Argument:{argument}", false);
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
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "reset":
                        Init();
                        break;
                    default:
                        Search();
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            Display();
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{machine_state}", false);
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
