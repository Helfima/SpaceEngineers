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
        private StateLon lon_state = StateLon.Stopped;

        private float last_power = 0f;
        private float power = 0f;
        private float max_power = 0f;
        private float delta = 0f;


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
                    default:
                        machine_state = StateMachine.TrakingLat;
                        last_power = 0f;
                        break;
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
            drawingSurface.WriteText($"\nlon_state:{lon_state}", true);
            if (lat_stators != null && !lat_stators.IsEmpty)
            {
                drawingSurface.WriteText($"\nAngle Z:{Math.Round(Util.RadToDeg(lat_stators.First.Angle), 2)}", true);
            }
            drawingSurface.WriteText($"\nCurrent Power:{Util.GetKiloFormat(power * 1e6)}W", true);
            drawingSurface.WriteText($"\nMax Power:{Util.GetKiloFormat(max_power * 1e6)}W", true);
            drawingSurface.WriteText($"\nLast Power:{Util.GetKiloFormat(last_power * 1e6)}W", true);
            drawingSurface.WriteText($"\nDelta:{delta}", true);

        }

        public void SolarPower()
        {
            if (solar_panels != null && !solar_panels.IsEmpty)
            {
                power = 0f;
                max_power = 0f;
                solar_panels.ForEach(delegate (IMySolarPanel block){
                    power += block.CurrentOutput;
                    max_power += block.MaxOutput;
                });
                
            }
        }

        void Running()
        {
            delta = max_power - last_power;
            switch (machine_state)
            {
                case StateMachine.Stopped:
                    lat_stators.Off();
                    lat_stators.Lock();
                    lon_stators.Off();
                    lon_stators.Lock();

                    if (Math.Abs(delta) > MyProperty.Lat_Delta)
                    {
                        machine_state = StateMachine.TrakingLat;
                        last_power = 0;
                    }
                    break;
                case StateMachine.TrakingLat:
                    if (Math.Abs(delta) < MyProperty.Lat_Delta)
                    {
                        machine_state = StateMachine.TrakingLon;
                    }
                    else
                    {

                        if(delta < 0)
                        {
                            lat_state = lat_state == StateLat.Forward ? StateLat.Backward : StateLat.Forward;
                        }

                        lat_stators.ForEach(delegate (IMyMotorStator block) {
                            if (lat_state == StateLat.Forward)
                            {
                                block.TargetVelocityRPM = MyProperty.Lat_Speed;
                            }
                            else
                            {
                                block.TargetVelocityRPM = -MyProperty.Lat_Speed;
                            }
                        });
                        lat_stators.On();
                        lat_stators.Unlock();
                    
                    }
                    break;
                case StateMachine.TrakingLon:
                    if (Math.Abs(delta) < MyProperty.Lat_Delta)
                    {
                        machine_state = StateMachine.Stopped;
                    }
                    else
                    {

                        if (delta < 0)
                        {
                            lon_state = lon_state == StateLon.Forward ? StateLon.Backward : StateLon.Forward;
                        }

                        lon_stators.ForEach(delegate (IMyMotorStator block) {
                            if (lon_state == StateLon.Forward)
                            {
                                block.TargetVelocityRPM = MyProperty.Lat_Speed;
                            }
                            else
                            {
                                block.TargetVelocityRPM = -MyProperty.Lat_Speed;
                            }
                        });
                        lon_stators.On();
                        lon_stators.Unlock();

                    }
                    break;
                default:
                    if(Math.Abs(delta) < MyProperty.Lat_Delta)
                    {
                        machine_state = StateMachine.Stopped;
                    } else
                    {
                        machine_state = StateMachine.TrakingLat;
                    }
                    break;
            }
            last_power = max_power;
        }

        public enum StateMachine
        {
            Stopped,
            TrakingLat,
            TrakingLon,
            Running,
            Waitting
        }
        public enum StateLat
        {
            Stopped,
            Forward,
            Backward
        }

        public enum StateLon
        {
            Stopped,
            Forward,
            Backward
        }
    }
}
