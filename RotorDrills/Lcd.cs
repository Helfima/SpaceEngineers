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
    partial class Program
    {
        public class Lcd
        {
            public Program myProgram;
            public List<IMyTextPanel> panel_list = new List<IMyTextPanel>();

            public Lcd(Program program)
            {
                myProgram = program;
            }

            public void Search(string tag)
            {
                panel_list.Clear();
                myProgram.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panel_list, block => block.CustomName.Contains(tag));
                myProgram.Echo(String.Format("Lcd <{0}> found:{1}", tag, panel_list.Count));
                if (panel_list.Count > 0)
                {
                    foreach (IMyTextPanel panel in panel_list)
                    {
                        panel.BackgroundColor = Color.Black;
                        panel.ScriptBackgroundColor = Color.Black;
                    }
                }
            }

            public void Print(StringBuilder message, bool append)
            {
                foreach (IMyTextPanel panel in panel_list)
                {
                    panel.ContentType = ContentType.TEXT_AND_IMAGE;
                    panel.WriteText(message, append);
                }

            }

            public bool IsEmpty
            {
                get
                {
                    return panel_list.Count == 0;
                }
            }


        }
    }
}
