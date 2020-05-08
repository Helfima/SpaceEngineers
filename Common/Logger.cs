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
        public class Logger
        {
            Program myProgram;

            private IMyTextPanel panel;
            private List<string> messages = new List<string>();
            public int level = 0;
            public bool console = false;

            public Logger(Program program, string lcd_name)
            {
                myProgram = program;
                Search(lcd_name);
            }

            private void Search(string name)
            {
                if (myProgram.GridTerminalSystem == null) return;
                panel = (IMyTextPanel)myProgram.GridTerminalSystem.GetBlockWithName(name);
                if (panel != null)
                {
                    panel.FontSize = 0.75f;
                    panel.Font = "Monospace";
                    panel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                }
            }

            public void Console(string message)
            {
                if (console) myProgram.Echo(message);
            }

            public void Info(string message)
            {
                if (level >= 1) Print("INFO", message);
            }

            public void Debug(string message)
            {
                if (level >= 2) Print("DEBUG", message);
            }

            public void Trace(string message)
            {
                if (level >= 3) Print("TRACE", message);
            }

            private void Print(string mode, string message)
            {
                if (panel != null)
                {
                    messages.Add(String.Join("", String.Format("{0:0.00}", myProgram.Runtime.LastRunTimeMs), " [", mode, "] ", message, "\n"));
                    if (messages.Count > 20) messages.RemoveAt(0);
                    panel.WriteText("", false);
                    foreach (string content in messages)
                    {
                        panel.WriteText(content, true);
                    }
                }


            }
        }
    }
}
