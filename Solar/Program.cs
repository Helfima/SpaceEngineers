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
        KProperty MyProperty;

        BlockSystem<IMyMotorStator> lat_stators = null;
        BlockSystem<IMyMotorStator> lon_stators = null;
        BlockSystem<IMySolarPanel> solar_panels = null;

        private IMyTextSurface drawingSurface;
        private StateMachine machine_state = StateMachine.Stopped;
        private StateLat lat_state = StateLat.Stopped;

        private List<float> last_outputs = new List<float>();

        private float lat_output = 0f;

        float delta1 = 0f;
        float delta2 = 0f;


        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            lat_stators = BlockSystem<IMyMotorStator>.SearchByName(this, MyProperty.Name);
            if (!lat_stators.IsEmpty)
            {
                lon_stators = BlockSystem<IMyMotorStator>.SearchByGrid(this, lat_stators.First.Top.CubeGrid);
                solar_panels = new BlockSystem<IMySolarPanel>();
                foreach (IMyMotorStator lon_stator in lon_stators.List)
                {
                    solar_panels.Merge(BlockSystem<IMySolarPanel>.SearchByGrid(this, lon_stator.Top.CubeGrid));
                }
            }
        }

        public void Save()
        {
            MyProperty.Save();
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
            MyProperty.Load();
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                }
            }
        }
        void RunContinuousLogic()
        {
            SolarPower();
            Display();
            Running();
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{machine_state}", false);
            drawingSurface.WriteText($"\nlat_state:{lat_state}", true);
            if (lat_stators != null && !lat_stators.IsEmpty)
            {
                drawingSurface.WriteText($"\nAngle Z:{Math.Round(Util.RadToDeg(lat_stators.First.Angle), 2)}", true);
            }
            drawingSurface.WriteText($"\nLast_outputs:{last_outputs.Count}", true);
            if (last_outputs != null && last_outputs.Count > 0)
            {
                drawingSurface.WriteText($"\nSolar Power:{Util.GetKiloFormat(last_outputs.Last()* 1e6)}W", true);
                drawingSurface.WriteText($"\nDelta1:{delta1}", true);
                drawingSurface.WriteText($"\nDelta2:{delta2}", true);
            }

            foreach(float last_output in last_outputs)
            {
                drawingSurface.WriteText($"\nlast_output:{last_output}", true);
            }

        }

        public void SolarPower()
        {
            if (solar_panels != null && !solar_panels.IsEmpty)
            {
                float power = 0f;
                foreach (IMySolarPanel solar_panel in solar_panels.List)
                {
                    power += solar_panel.MaxOutput;
                }
                last_outputs.Add(power);
                if (last_outputs.Count > 4)
                {
                    last_outputs.RemoveAt(0);
                }
                if (last_outputs.Count > 3)
                {
                    delta1 = last_outputs[1] - last_outputs[0];
                    delta2 = last_outputs[3] - last_outputs[2];
                }
            }
        }

        void ClearLat()
        {
            last_outputs.Clear();
            delta1 = 0f;
            delta2 = 0f;
        }
        void Running()
        {
            IMyMotorStator lat_stator = lat_stators.First;
            switch (machine_state)
            {
                case StateMachine.Stopped:
                    lat_stator.ApplyAction("OnOff_Off");
                    lat_stator.RotorLock = true;

                    if (Math.Abs(lat_output - last_outputs.Last()) > MyProperty.Lat_Delta)
                    {
                        machine_state = StateMachine.Traking;
                    }
                    break;
                case StateMachine.Traking:

                    if (delta1 < 0 && delta2 < 0)
                    {
                        lat_state = lat_state == StateLat.Forward ? lat_state = StateLat.Backward : lat_state = StateLat.Forward;
                        ClearLat();
                    } else if (delta1 > 0 && delta2 < 0)
                    {
                        machine_state = StateMachine.Stopped;
                        lat_output = last_outputs.Last();
                        ClearLat();
                    }
                    lat_stator.ApplyAction("OnOff_On");
                    lat_stator.RotorLock = false;
                    if (lat_state == StateLat.Forward)
                    {
                        lat_stator.TargetVelocityRPM = MyProperty.Lat_Speed;
                    }
                    else
                    {
                        lat_stator.TargetVelocityRPM = -MyProperty.Lat_Speed;
                    }
                    break;
                default:
                    if(last_outputs.Last() == 0f)
                    {
                        machine_state = StateMachine.Stopped;
                    } else
                    {
                        machine_state = StateMachine.Traking;
                    }
                    break;
            }

        }

        public enum StateMachine
        {
            Stopped,
            Traking,
            Running,
            Waitting
        }
        public enum StateLat
        {
            Stopped,
            Forward,
            Backward
        }
    }
}
