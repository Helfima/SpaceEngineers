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

        private ModeMachine Mode = ModeMachine.Stop;
        private ModeMachine LastMode = ModeMachine.Stop;
        private List<ActionMachine> Sequence;
        private List<int> SequenceBlocks;
        private int Stage = 0;
        private int Cycle = 0;
        private int projector_count = 0;
        private int blueprint_count = 0;
        private float current_position = 0f;
        private float last_position = 0f;
        private float velocity = 0f;

        private BlockSystem<IMyMotorStator> bottom_stators_1 = null;
        private BlockSystem<IMyMotorStator> bottom_stators_2 = null;
        private BlockSystem<IMyShipMergeBlock> bottom_mergers = null;

        private BlockSystem<IMyMotorStator> top_stators_1 = null;
        private BlockSystem<IMyMotorStator> top_stators_2 = null;
        private BlockSystem<IMyShipMergeBlock> top_mergers = null;

        private BlockSystem<IMyShipConnector> connector = null;
        private BlockSystem<IMyMotorStator> connector_stator = null;

        private BlockSystem<IMyPistonBase> levage_pistons = null;

        private BlockSystem<IMyMotorStator> grinder_stator = null;
        private BlockSystem<IMyShipGrinder> grinders = null;

        private BlockSystem<IMyMotorStator> welder_stator = null;
        private BlockSystem<IMyShipWelder> welders = null;

        private BlockSystem<IMyProjector> projector = null;

        private BlockSystem<IMyLightingBlock> light = null;
        private BlockSystem<IMyShipDrill> drills = null;

        private BlockSystem<IMyTextPanel> control_lcds = null;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
        }

        private void Init()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Stage = 0;
            bottom_stators_1 = BlockSystem<IMyMotorStator>.SearchByName(this, "Bottom Stator 1");
            bottom_stators_2 = BlockSystem<IMyMotorStator>.SearchByName(this, "Bottom Stator 2");
            bottom_mergers = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Bottom Mergers");

            top_stators_1 = BlockSystem<IMyMotorStator>.SearchByName(this, "Top Stator 1");
            top_stators_2 = BlockSystem<IMyMotorStator>.SearchByName(this, "Top Stator 2");
            top_mergers = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Top Mergers");

            connector = BlockSystem<IMyShipConnector>.SearchByName(this, "Connector Drill");
            connector_stator = BlockSystem<IMyMotorStator>.SearchByName(this, "Connector Stator");

            levage_pistons = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Levage Pistons");
            levage_pistons.ForEach(delegate (IMyPistonBase block) {
                block.MinLimit = MyProperty.elevator_position_min;
                block.MaxLimit = MyProperty.elevator_position_max;
            });

            grinder_stator = BlockSystem<IMyMotorStator>.SearchByName(this, "Grinder Stator");
            grinders = BlockSystem<IMyShipGrinder>.SearchByGroup(this, "Grinders");

            welder_stator = BlockSystem<IMyMotorStator>.SearchByName(this, "Welder Stator");
            welders = BlockSystem<IMyShipWelder>.SearchByGroup(this, "Welders");

            projector = BlockSystem<IMyProjector>.SearchByName(this, "Projector Drill");

            light = BlockSystem<IMyLightingBlock>.SearchByGroup(this, "Rotating Light");

            drills = BlockSystem<IMyShipDrill>.SearchByGroup(this, "Drills");

            control_lcds = BlockSystem<IMyTextPanel>.SearchByName(this, "Control LCD");
            control_lcds.ForEach(delegate (IMyTextPanel block)
            {
                block.ContentType = ContentType.TEXT_AND_IMAGE;
            });

            SequenceBlocks = new List<int>() { };

        }

        public void Save()
        {

        }

        public void WriteText(string message, bool append)
        {
            message += "\n";
            drawingSurface.WriteText(message, append);
            control_lcds.ForEach(delegate (IMyTextPanel block)
            {
                block.WriteText(message, append);
            });
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
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);
                if(commandLine.ArgumentCount > 1)
                {
                    string cycle = commandLine.Argument(1);
                    int.TryParse(cycle, out Cycle);
                }
                else
                {
                    Cycle = 1;
                }

                switch (command.ToLower())
                {
                    case "init":
                        Init();
                        break;
                    case "stop":
                        LastMode = Mode;
                        drills.Off();
                        Mode = ModeMachine.Stop;
                        break;
                    case "start":
                        drills.On();
                        Mode = LastMode;
                        break;
                    case "lock":
                        Stage = 0;
                        Mode = ModeMachine.Lock;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.LockBottom);
                        Sequence.Add(ActionMachine.LockTop);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "lockbottom":
                        Stage = 0;
                        Mode = ModeMachine.LockBottom;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.LockBottom);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "unlockbottom":
                        Stage = 0;
                        Mode = ModeMachine.UnlockBottom;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.UnlockBottom);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "locktop":
                        Stage = 0;
                        Mode = ModeMachine.LockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.LockTop);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "unlocktop":
                        Stage = 0;
                        Mode = ModeMachine.UnlockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.UnlockTop);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "lockconnector":
                        Stage = 0;
                        Mode = ModeMachine.LockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.LockConnector);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "unlockconnector":
                        Stage = 0;
                        Mode = ModeMachine.UnlockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.UnlockConnector);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "startwelder":
                        Stage = 0;
                        Mode = ModeMachine.LockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.StartWelder);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "stopwelder":
                        Stage = 0;
                        Mode = ModeMachine.UnlockTop;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.StopWelder);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "down":
                        Stage = 0;
                        Mode = ModeMachine.Down;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.LockConnector);
                        Sequence.Add(ActionMachine.UnlockBottom);
                        Sequence.Add(ActionMachine.StartWelder);
                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.StopWelder);
                        Sequence.Add(ActionMachine.LockBottom);
                        Sequence.Add(ActionMachine.UnlockConnector);
                        Sequence.Add(ActionMachine.UnlockTop);
                        Sequence.Add(ActionMachine.Up);
                        Sequence.Add(ActionMachine.LockTop);

                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "up":
                        Stage = 0;
                        Mode = ModeMachine.Up;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.UnlockTop);
                        Sequence.Add(ActionMachine.UnlockConnector);
                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.LockConnector);
                        Sequence.Add(ActionMachine.LockTop);
                        Sequence.Add(ActionMachine.UnlockBottom);
                        Sequence.Add(ActionMachine.StartGrinder);
                        Sequence.Add(ActionMachine.Up);
                        Sequence.Add(ActionMachine.StopGrinder);
                        Sequence.Add(ActionMachine.LockBottom);

                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "reset":
                        Mode = ModeMachine.Reset;
                        Stage = 0;
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            Display();
            if (Mode != ModeMachine.Stop && Cycle > 0 && Sequence.Count > Stage)
            {
                Staging(Sequence[Stage]);
            }
        }
        private void Display()
        {
            WriteText($"Machine Mode:{Mode}", false);
            WriteText($"Machine Cycle:{Cycle}", true);
            if (Sequence != null && Sequence.Count > Stage)
            {
                WriteText($"Machine Action:{Sequence[Stage]}", true);
            }
            WriteText($"Machine Stage:{Stage}", true);
            BlockSystem<IMyShipConnector> connectors = BlockSystem<IMyShipConnector>.SearchBlocks(this);
            float deep = MyProperty.elevator_position_max * connectors.List.Count;
            WriteText($"Machine Deep:{deep}", true);
        }


        private void Staging(ActionMachine action)
        {
            bool closed = true;
            bool opened = true;
            float position_target = 0f;
            float angle1 = 0f;
            float angle2 = 0f;
            switch (action)
            {
                case ActionMachine.LockBottom:
                    velocity = MyProperty.locker_velocity;
                    angle1 = MyProperty.locker_position_max_1;
                    angle2 = MyProperty.locker_position_min_2;

                    bottom_mergers.On();
                    WriteText($"Bottom mergers: On", true);
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target position 1: {angle1}", true);
                    WriteText($"Target position 2: {angle2}", true);
                    closed = true;

                    bottom_stators_1.Unlock();
                    bottom_stators_1.Velocity(velocity);
                    bottom_stators_1.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Bottom angle 1: {Util.RadToDeg(block.Angle)}", true);
                    });

                    bottom_stators_2.Unlock();
                    bottom_stators_2.Velocity(-velocity);
                    bottom_stators_2.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Bottom angle 2: {Util.RadToDeg(block.Angle)}", true);
                    });
                    if(bottom_stators_1.IsLessPosition(angle1) || bottom_stators_2.IsMorePosition(angle2)) closed = false;
                    if (closed)
                    {
                        bottom_stators_1.Lock();
                        bottom_stators_2.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.UnlockBottom:
                    closed = true;
                    top_mergers.ForEach(delegate (IMyShipMergeBlock block) {
                        if (!block.IsConnected) closed = false;
                    });
                    if (!closed)
                    {
                        WriteText($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", true);
                        WriteText($"Security: Top mergers is Off", true);
                    }
                    else
                    {
                        velocity = MyProperty.locker_velocity;
                        angle1 = MyProperty.locker_position_min_1;
                        angle2 = MyProperty.locker_position_max_2;

                        bottom_mergers.Off();
                        WriteText($"Bottom mergers: Off", true);
                        WriteText($"Velocity: {velocity}", true);
                        WriteText($"Target position 1: {angle1}", true);
                        WriteText($"Target position 2: {angle2}", true);
                        closed = true;

                        bottom_stators_1.Unlock();
                        bottom_stators_1.Velocity(-velocity);
                        bottom_stators_1.ForEach(delegate (IMyMotorStator block)
                        {
                            WriteText($"Bottom angle 1: {Util.RadToDeg(block.Angle)}", true);
                        });

                        bottom_stators_2.Unlock();
                        bottom_stators_2.Velocity(velocity);
                        bottom_stators_2.ForEach(delegate (IMyMotorStator block)
                        {
                            WriteText($"Bottom angle 2: {Util.RadToDeg(block.Angle)}", true);
                        });
                        if (bottom_stators_1.IsMorePosition(angle1) || bottom_stators_2.IsLessPosition(angle2)) closed = false;
                        if (closed)
                        {
                            bottom_stators_1.Lock();
                            bottom_stators_2.Lock();
                            
                            Stage++;
                        }
                    }
                    break;
                case ActionMachine.LockTop:
                    velocity = MyProperty.locker_velocity;
                    angle1 = MyProperty.locker_position_max_1;
                    angle2 = MyProperty.locker_position_min_2;

                    top_mergers.On();
                    WriteText($"Top mergers: On", true);
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target position 1: {angle1}", true);
                    WriteText($"Target position 2: {angle2}", true);
                    closed = true;

                    top_stators_1.Unlock();
                    top_stators_1.Velocity(velocity);
                    top_stators_1.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Top angle 1: {Util.RadToDeg(block.Angle)}", true);
                    });

                    top_stators_2.Unlock();
                    top_stators_2.Velocity(-velocity);
                    top_stators_2.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Top angle 2: {Util.RadToDeg(block.Angle)}", true);
                    });
                    if (top_stators_1.IsLessPosition(angle1) || top_stators_2.IsMorePosition(angle2)) closed = false;
                    if (closed)
                    {
                        top_stators_1.Lock();
                        top_stators_2.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.UnlockTop:
                    closed = true;
                    bottom_mergers.ForEach(delegate (IMyShipMergeBlock block) {
                        if (!block.IsConnected) closed = false;
                    });
                    if (!closed)
                    {
                        WriteText($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", true);
                        WriteText($"Security: Bottom mergers is Off", true);
                    }
                    else
                    {
                        velocity = MyProperty.locker_velocity;
                        angle1 = MyProperty.locker_position_min_1;
                        angle2 = MyProperty.locker_position_max_2;

                        top_mergers.Off();
                        WriteText($"Top mergers: Off", true);
                        WriteText($"Velocity: {velocity}", true);
                        WriteText($"Target position 1: {angle1}", true);
                        WriteText($"Target position 2: {angle2}", true);
                        closed = true;

                        top_stators_1.Unlock();
                        top_stators_1.Velocity(-velocity);
                        top_stators_1.ForEach(delegate (IMyMotorStator block)
                        {
                            WriteText($"Top angle 1: {Util.RadToDeg(block.Angle)}", true);
                        });

                        top_stators_2.Unlock();
                        top_stators_2.Velocity(velocity);
                        top_stators_2.ForEach(delegate (IMyMotorStator block)
                        {
                            WriteText($"Top angle 2: {Util.RadToDeg(block.Angle)}", true);
                        });
                        if (top_stators_1.IsMorePosition(angle1) || top_stators_2.IsLessPosition(angle2)) closed = false;
                        if (closed)
                        {
                            top_stators_1.Lock();
                            top_stators_2.Lock();

                            Stage++;
                        }
                    }
                    break;
                case ActionMachine.LockConnector:
                    velocity = MyProperty.connector_velocity;
                    position_target = MyProperty.connector_position_max;
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target position: {position_target}", true);
                    closed = true;
                    connector_stator.Velocity(velocity);
                    connector_stator.Unlock();
                    connector.On();
                    connector_stator.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Angle: {Util.RadToDeg(block.Angle)}", true);
                        if (connector_stator.IsLessPosition(position_target))
                        {
                            closed = false;
                        }
                    });
                    if (closed)
                    {
                        connector.Lock();
                        connector_stator.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.UnlockConnector:
                    velocity = -MyProperty.connector_velocity;
                    position_target = MyProperty.connector_position_min;
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target position: {position_target}", true);
                    closed = true;
                    connector_stator.Unlock();
                    connector_stator.Velocity(velocity);
                    connector.Unlock();
                    connector.Off();
                    connector_stator.ForEach(delegate (IMyMotorStator block)
                    {
                        WriteText($"Angle: {Util.RadToDeg(block.Angle)}", true);
                        if (connector_stator.IsMorePosition(position_target))
                        {
                            closed = false;
                        }
                    });
                    if (closed)
                    {
                        connector_stator.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.Down:
                    levage_pistons.On();
                    WriteText($"Piston Levage: On", true);

                    velocity = MyProperty.elevator_velocity_min;
                    if (Mode == ModeMachine.Up) velocity = MyProperty.elevator_velocity_max;
                    WriteText($"Piston Velocity: {velocity}", true);
                    levage_pistons.Velocity(velocity);

                    position_target = MyProperty.elevator_position_max;
                    WriteText($"Target Position={position_target}", true);

                    levage_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        WriteText($"Position={block.CurrentPosition}", true);
                    });

                    //projector_count = projector.List[0].RemainingBlocks;
                    if (levage_pistons.IsMorePosition(position_target))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.Up:
                    levage_pistons.On();
                    WriteText($"Piston Levage: On", true);

                    velocity = -MyProperty.elevator_velocity_max;
                    if (Mode == ModeMachine.Up) velocity = -MyProperty.elevator_velocity_min;
                    WriteText($"Piston Velocity: {velocity}", true);
                    levage_pistons.Velocity(velocity);

                    position_target = MyProperty.elevator_position_min;
                    WriteText($"Target Position={position_target}", true);

                    levage_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        WriteText($"Position={block.CurrentPosition}", true);
                    });
                    //projector_count = projector.List[0].RemainingBlocks;
                    if (levage_pistons.IsLessPosition(position_target+0.1f))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.StartWelder:
                    projector.On();
                    welders.On();
                    WriteText($"Welders: On", true);
                    Stage++;
                    break;
                case ActionMachine.StopWelder:
                    welders.Off();
                    projector.Off();
                    WriteText($"Welders: Off", true);

                    Stage++;
                    break;
                case ActionMachine.StartGrinder:
                    grinders.On();
                    projector.On();
                    WriteText($"Grinders: On", true);

                    Stage++;
                    break;
                case ActionMachine.StopGrinder:
                    grinders.Off();
                    projector.Off();
                    WriteText($"Grinders: Off", true);

                    Stage++;
                    break;
                case ActionMachine.Terminated:
                    Cycle -= 1;
                    if(Cycle == 0) Mode = ModeMachine.Stop;
                    Stage = 0;
                    break;
                case ActionMachine.Start:
                    MyProperty.Load();
                    light.On();
                    drills.On();
                    Stage++;
                    break;
                case ActionMachine.Stop:
                    projector.Off();
                    light.Off();
                    drills.Off();
                    Stage++;
                    break;
            }
        }

        public enum ModeMachine
        {
            Lock,
            LockBottom,
            UnlockBottom,
            LockTop,
            UnlockTop,
            Stop,
            Down,
            Up,
            Reset
        }
        public enum ActionMachine
        {
            LockBottom,
            UnlockBottom,
            LockTop,
            UnlockTop,
            LockConnector,
            UnlockConnector,
            Start,
            Stop,
            StartWelder,
            StopWelder,
            StartGrinder,
            StopGrinder,
            Up,
            Down,
            Terminated
        }
    }
}
