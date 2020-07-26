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
        public class ThrusterSystem
        {
            protected Program program;

            private BlockSystem<IMyThrust> thrusters;

            private string thrusters_filter;
            public ThrusterSystem(Program program)
            {
                this.program = program;
                Search();
            }

            private void Search()
            {
                BlockFilter<IMyThrust> block_thrusters_filter = BlockFilter<IMyThrust>.Create(program.Me, thrusters_filter);
                thrusters = BlockSystem<IMyThrust>.SearchByFilter(program, block_thrusters_filter);
            }
        }
    }
}
