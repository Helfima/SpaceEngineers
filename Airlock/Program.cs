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

        KProperty MyProperty;

        List<IMyBlockGroup> Groups;
        List<Airlock> Airlocks;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
            Search();
        }
        private void Init()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
        }
        public void Save()
        {

        }

        private void Search()
        {
            BlockFilter<IMyBlockGroup> group_filter = BlockFilter<IMyBlockGroup>.Create(Me, MyProperty.filter);
            Groups = BlockSystem<IMyBlockGroup>.SearchGroupFilter(this, group_filter);
            Airlocks = new List<Airlock>();
            if (Groups != null) Groups.ForEach(delegate (IMyBlockGroup group)
             {
                 Airlocks.Add(new Airlock(this, group.Name));
             });
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
                if (command != null) { 
                    
                }
                else
                {
                    Init();
                    Search();
                }

                string current_group = "";
                if (commandLine.ArgumentCount > 1)
                {
                    current_group = commandLine.Argument(1);
                }
                if (Airlocks != null) Airlocks.ForEach(delegate (Airlock airlock)
                {
                    if(current_group.Equals("") || airlock.GroupFilter.Contains(current_group))
                    airlock.RunCommand(argument);
                });
            }
        }

        
        void RunContinuousLogic()
        {
            Display();
            if (Airlocks != null) Airlocks.ForEach(delegate (Airlock airlock)
            {
                airlock.RunContinuousLogic();
            });
        }

        public void WriteText(string message, bool append)
        {
            message += "\n";
            drawingSurface.WriteText(message, append);
        }

        private void Display()
        {
            if (Groups != null)
            {
                WriteText($"Group Size:{Groups.Count}", false);
                Groups.ForEach(delegate (IMyBlockGroup group) {
                    WriteText($"Group Name:{group.Name}", true);
                });
            }
            else
            {
                WriteText($"No Group Selected", false);
            }

        }
        
    }
}
