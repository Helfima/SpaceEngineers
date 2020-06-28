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
                string tag = commandLine.Argument(1);
                switch (command)
                {
                    case "prefix":
                        RenamePrefix(tag);
                        break;
                    case "unprefix":
                        UnRenamePrefix(tag);
                        break;
                }
            }
        }

        private void RenamePrefix(string tag)
        {
            BlockSystem<IMyTerminalBlock> blocks = BlockSystem<IMyTerminalBlock>.SearchBlocks(this);
            blocks.ForEach(delegate (IMyTerminalBlock block)
            {
                if (block is IMyFunctionalBlock || block is IMyCargoContainer)
                {
                    if (!block.CustomName.StartsWith(tag))
                    {
                        block.CustomName = tag + " " + block.CustomName;
                    }
                }
            });

        }
        private void UnRenamePrefix(string tag)
        {
            BlockSystem<IMyTerminalBlock> blocks = BlockSystem<IMyTerminalBlock>.SearchBlocks(this);
            blocks.ForEach(delegate (IMyTerminalBlock block)
            {
                if (block is IMyFunctionalBlock || block is IMyCargoContainer)
                {
                    if (block.CustomName.StartsWith(tag))
                    {
                        block.CustomName = block.CustomName.Replace(tag + " ", "");
                    }
                }
            });

        }
        void RunContinuousLogic()
        {
            Display();
        }

        private void Display()
        {
            drawingSurface.WriteText($"Rename Script", false);
        }

        public void WriteText(string message, bool append)
        {
            message += "\n";
            drawingSurface.WriteText(message, append);
        }
    }
}
