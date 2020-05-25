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

        private BlockSystem<IMyTextPanel> projection_lcds = null;
        private BlockSystem<IMyTextPanel> check_lcds = null;
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

            projection_lcds = BlockSystem<IMyTextPanel>.SearchByName(this, "Projection LCD");

            check_lcds = BlockSystem<IMyTextPanel>.SearchByName(this, "Check LCD");

            SequenceBlocks = new List<int>() { };

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
                    case "check":
                        CheckMergers();
                        break;
                    case "stop":
                        Mode = ModeMachine.Stop;
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
            DisplayProjection();
            CheckMergers();
            if (Mode != ModeMachine.Stop && Cycle > 0 && Sequence.Count > Stage)
            {
                Staging(Sequence[Stage]);
            }
        }
        private void Display()
        {
            drawingSurface.WriteText($"Machine Mode:{Mode}", false);
            drawingSurface.WriteText($"Machine Cycle:{Cycle}", false);
            if (Sequence != null && Sequence.Count > Stage)
            {
                drawingSurface.WriteText($"\nMachine Action:{Sequence[Stage]}", true);
            }
            drawingSurface.WriteText($"\nMachine Stage:{Stage}", true);
            float deep = MyProperty.elevator_position_max * blueprint_count;
            drawingSurface.WriteText($"\nMachine Deep:{deep}", true);
        }

        private void CheckMergers()
        {
            check_lcds.First.WriteText($"Check Mergers\n", false);
            bottom_mergers.ForEach(delegate (IMyShipMergeBlock block)
            {
                check_lcds.First.WriteText($"{block.CustomName} IsConnected:{block.IsConnected}\n", true);
                check_lcds.First.WriteText($"{block.CustomName} IsWorking:{block.IsWorking}\n", true);
                check_lcds.First.WriteText($"{block.CustomName} CheckConnectionAllowed:{block.CheckConnectionAllowed}\n", true);
            });
            top_mergers.ForEach(delegate (IMyShipMergeBlock block)
            {
                check_lcds.First.WriteText($"{block.CustomName} IsConnected:{block.IsConnected}\n", true);
                check_lcds.First.WriteText($"{block.CustomName} IsWorking:{block.IsWorking}\n", true);
                check_lcds.First.WriteText($"{block.CustomName} CheckConnectionAllowed:{block.CheckConnectionAllowed}\n", true);
            });
        }
        private void DisplayProjection()
        {
            projection_lcds.ForEach(delegate (IMyTextPanel block)
            {
                block.WriteText($"TotalBlocks:{projector.First.TotalBlocks}\n", false);
                block.WriteText($"BuildableBlocksCount:{projector.First.BuildableBlocksCount}\n", true);
                block.WriteText($"RemainingBlocks:{projector.First.RemainingBlocks}\n", true);
            });
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
                    drawingSurface.WriteText($"\nBottom mergers: On", true);
                    drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                    drawingSurface.WriteText($"\nTarget position 1: {angle1}", true);
                    drawingSurface.WriteText($"\nTarget position 2: {angle2}", true);
                    closed = true;

                    bottom_stators_1.Unlock();
                    bottom_stators_1.Velocity(velocity);
                    bottom_stators_1.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nBottom angle 1: {Util.RadToDeg(block.Angle)}", true);
                    });

                    bottom_stators_2.Unlock();
                    bottom_stators_2.Velocity(-velocity);
                    bottom_stators_2.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nBottom angle 2: {Util.RadToDeg(block.Angle)}", true);
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
                        drawingSurface.WriteText($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", true);
                        drawingSurface.WriteText($"\nSecurity: Top mergers is Off", true);
                    }
                    else
                    {
                        velocity = MyProperty.locker_velocity;
                        angle1 = MyProperty.locker_position_min_1;
                        angle2 = MyProperty.locker_position_max_2;

                        bottom_mergers.Off();
                        drawingSurface.WriteText($"\nBottom mergers: Off", true);
                        drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                        drawingSurface.WriteText($"\nTarget position 1: {angle1}", true);
                        drawingSurface.WriteText($"\nTarget position 2: {angle2}", true);
                        closed = true;

                        bottom_stators_1.Unlock();
                        bottom_stators_1.Velocity(-velocity);
                        bottom_stators_1.ForEach(delegate (IMyMotorStator block)
                        {
                            drawingSurface.WriteText($"\nBottom angle 1: {Util.RadToDeg(block.Angle)}", true);
                        });

                        bottom_stators_2.Unlock();
                        bottom_stators_2.Velocity(velocity);
                        bottom_stators_2.ForEach(delegate (IMyMotorStator block)
                        {
                            drawingSurface.WriteText($"\nBottom angle 2: {Util.RadToDeg(block.Angle)}", true);
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
                    drawingSurface.WriteText($"\nTop mergers: On", true);
                    drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                    drawingSurface.WriteText($"\nTarget position 1: {angle1}", true);
                    drawingSurface.WriteText($"\nTarget position 2: {angle2}", true);
                    closed = true;

                    top_stators_1.Unlock();
                    top_stators_1.Velocity(velocity);
                    top_stators_1.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nTop angle 1: {Util.RadToDeg(block.Angle)}", true);
                    });

                    top_stators_2.Unlock();
                    top_stators_2.Velocity(-velocity);
                    top_stators_2.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nTop angle 2: {Util.RadToDeg(block.Angle)}", true);
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
                        drawingSurface.WriteText($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", true);
                        drawingSurface.WriteText($"\nSecurity: Bottom mergers is Off", true);
                    }
                    else
                    {
                        velocity = MyProperty.locker_velocity;
                        angle1 = MyProperty.locker_position_min_1;
                        angle2 = MyProperty.locker_position_max_2;

                        top_mergers.Off();
                        drawingSurface.WriteText($"\nTop mergers: Off", true);
                        drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                        drawingSurface.WriteText($"\nTarget position 1: {angle1}", true);
                        drawingSurface.WriteText($"\nTarget position 2: {angle2}", true);
                        closed = true;

                        top_stators_1.Unlock();
                        top_stators_1.Velocity(-velocity);
                        top_stators_1.ForEach(delegate (IMyMotorStator block)
                        {
                            drawingSurface.WriteText($"\nTop angle 1: {Util.RadToDeg(block.Angle)}", true);
                        });

                        top_stators_2.Unlock();
                        top_stators_2.Velocity(velocity);
                        top_stators_2.ForEach(delegate (IMyMotorStator block)
                        {
                            drawingSurface.WriteText($"\nTop angle 2: {Util.RadToDeg(block.Angle)}", true);
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
                    drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                    drawingSurface.WriteText($"\nTarget position: {position_target}", true);
                    closed = true;
                    connector_stator.Velocity(velocity);
                    connector_stator.Unlock();
                    connector.On();
                    connector_stator.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nAngle: {Util.RadToDeg(block.Angle)}", true);
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
                    drawingSurface.WriteText($"\nVelocity: {velocity}", true);
                    drawingSurface.WriteText($"\nTarget position: {position_target}", true);
                    closed = true;
                    connector_stator.Unlock();
                    connector_stator.Velocity(velocity);
                    connector.Unlock();
                    connector.Off();
                    connector_stator.ForEach(delegate (IMyMotorStator block)
                    {
                        drawingSurface.WriteText($"\nAngle: {Util.RadToDeg(block.Angle)}", true);
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
                    drawingSurface.WriteText($"\nPiston Levage: On", true);

                    velocity = MyProperty.elevator_velocity_min;
                    if (Mode == ModeMachine.Up) velocity = MyProperty.elevator_velocity_max;
                    drawingSurface.WriteText($"\nPiston Velocity: {velocity}", true);
                    levage_pistons.Velocity(velocity);

                    position_target = MyProperty.elevator_position_max;
                    drawingSurface.WriteText($"\nTarget Position={position_target}", true);

                    levage_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPosition={block.CurrentPosition}", true);
                    });

                    //projector_count = projector.List[0].RemainingBlocks;
                    if (levage_pistons.IsMorePosition(position_target))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.Up:
                    levage_pistons.On();
                    drawingSurface.WriteText($"\nPiston Levage: On", true);

                    velocity = -MyProperty.elevator_velocity_max;
                    if (Mode == ModeMachine.Up) velocity = -MyProperty.elevator_velocity_min;
                    drawingSurface.WriteText($"\nPiston Velocity: {velocity}", true);
                    levage_pistons.Velocity(velocity);

                    position_target = MyProperty.elevator_position_min;
                    drawingSurface.WriteText($"\nTarget Position={position_target}", true);

                    levage_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPosition={block.CurrentPosition}", true);
                    });
                    //projector_count = projector.List[0].RemainingBlocks;
                    if (levage_pistons.IsLessPosition(position_target))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.StartWelder:
                    projector.On();
                    drawingSurface.WriteText($"\nWelders: On", true);
                    
                    velocity = -MyProperty.welder_velocity;
                    welder_stator.Velocity(velocity);
                    drawingSurface.WriteText($"\nWelder Velocity: {velocity}", true);

                    position_target = MyProperty.welder_position_min;
                    drawingSurface.WriteText($"\nTarget Position: {position_target}", true);
                    drawingSurface.WriteText($"\nPosition: {Util.RadToDeg(welder_stator.First.Angle)}", true);
                    welder_stator.Unlock();
                    if (welder_stator.IsLessPosition(position_target))
                    {
                        welder_stator.Lock();
                        welders.On();
                        Stage++;
                    }
                    break;
                case ActionMachine.StopWelder:
                    welders.Off();
                    projector.Off();
                    drawingSurface.WriteText($"\nWelders: Off", true);

                    velocity = MyProperty.welder_velocity;
                    welder_stator.Velocity(velocity);
                    drawingSurface.WriteText($"\nWelder Velocity: {velocity}", true);

                    position_target = MyProperty.welder_position_max;
                    drawingSurface.WriteText($"\nTarget Position: {position_target}", true);
                    drawingSurface.WriteText($"\nPosition: {Util.RadToDeg(welder_stator.First.Angle)}", true);
                    welder_stator.Unlock();
                    if (welder_stator.IsMorePosition(position_target))
                    {
                        welder_stator.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.StartGrinder:
                    grinders.On();
                    projector.On();
                    drawingSurface.WriteText($"\nGrinders: On", true);

                    velocity = MyProperty.grinder_velocity;
                    grinder_stator.Velocity(velocity);
                    drawingSurface.WriteText($"\nGrinder Velocity: {velocity}", true);

                    position_target = MyProperty.grinder_position_max;
                    drawingSurface.WriteText($"\nTarget Position: {position_target}", true);
                    grinder_stator.Unlock();
                    if (grinder_stator.IsMorePosition(position_target))
                    {
                        grinder_stator.Lock();
                        Stage++;
                    }
                    break;
                case ActionMachine.StopGrinder:
                    grinders.Off();
                    projector.Off();
                    drawingSurface.WriteText($"\nGrinders: Off", true);

                    velocity = -MyProperty.grinder_velocity;
                    grinder_stator.Velocity(velocity);
                    drawingSurface.WriteText($"\nGrinder Velocity: {velocity}", true);

                    position_target = MyProperty.grinder_position_min;
                    drawingSurface.WriteText($"\nTarget Position: {position_target}", true);
                    grinder_stator.Unlock();
                    if (grinder_stator.IsLessPosition(position_target))
                    {
                        grinder_stator.Lock();
                        Stage++;
                    }
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
