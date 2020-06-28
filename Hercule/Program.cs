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

        private BlockSystem<IMyPistonBase> bottom_pistons = null;
        private BlockSystem<IMyShipMergeBlock> bottom_mergers = null;

        private BlockSystem<IMyPistonBase> top_pistons = null;
        private BlockSystem<IMyShipMergeBlock> top_mergers = null;

        private BlockSystem<IMyShipConnector> connector = null;

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
            bottom_pistons = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Foreuse Bottom Pistons");
            bottom_pistons.ForEach(delegate (IMyPistonBase block) {
                block.MinLimit = MyProperty.locker_position_min;
                block.MaxLimit = MyProperty.locker_position_max;
            });
            bottom_mergers = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Foreuse Bottom Mergers");

            top_pistons = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Foreuse Top Pistons");
            top_pistons.ForEach(delegate (IMyPistonBase block) {
                block.MinLimit = MyProperty.locker_position_min;
                block.MaxLimit = MyProperty.locker_position_max;
            });
            top_mergers = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Foreuse Top Mergers");

            connector = BlockSystem<IMyShipConnector>.SearchByName(this, "Foreuse Connector Drills");

            levage_pistons = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Foreuse Levage Pistons");
            levage_pistons.ForEach(delegate (IMyPistonBase block) {
                block.MinLimit = MyProperty.elevator_position_min;
                block.MaxLimit = MyProperty.elevator_position_max;
            });

            grinder_stator = BlockSystem<IMyMotorStator>.SearchByName(this, "Foreuse Grinder Stator");
            grinders = BlockSystem<IMyShipGrinder>.SearchByGroup(this, "Foreuse Grinders");

            welder_stator = BlockSystem<IMyMotorStator>.SearchByName(this, "Foreuse Welder Stator");
            welders = BlockSystem<IMyShipWelder>.SearchByGroup(this, "Foreuse Welders");

            projector = BlockSystem<IMyProjector>.SearchByName(this, "Foreuse Projector Drill");

            light = BlockSystem<IMyLightingBlock>.SearchByGroup(this, "Foreuse Rotating Light");

            drills = BlockSystem<IMyShipDrill>.SearchByGroup(this, "Foreuse Drills");

            control_lcds = BlockSystem<IMyTextPanel>.SearchByName(this, "Foreuse Control LCD");
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
                        if (Mode == ModeMachine.Down) projector.On();
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

                        Sequence.Add(ActionMachine.UnlockBottom);
                        Sequence.Add(ActionMachine.StartWelder);
                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.StopWelder);
                        Sequence.Add(ActionMachine.LockBottom);
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
                        Sequence.Add(ActionMachine.Down);
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
            float position_target = 0f;
            float velocity = 0f;
            switch (action)
            {
                case ActionMachine.LockBottom:
                    velocity = -MyProperty.locker_velocity;
                    position_target = MyProperty.locker_position_min;

                    bottom_mergers.On();
                    bottom_pistons.Velocity(velocity);
                    WriteText($"Bottom mergers: On", true);
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target Position={position_target}", true);
                    closed = true;

                    bottom_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        WriteText($"Position={block.CurrentPosition}", true);
                    });

                    if (bottom_pistons.IsLessPosition(position_target))
                    {
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
                        position_target = MyProperty.locker_position_max;

                        bottom_mergers.Off();
                        bottom_pistons.Velocity(velocity);
                        WriteText($"Bottom mergers: Off", true);
                        WriteText($"Velocity: {velocity}", true);
                        WriteText($"Target Position={position_target}", true);
                        closed = true;

                        bottom_pistons.ForEach(delegate (IMyPistonBase block)
                        {
                            WriteText($"Position={block.CurrentPosition}", true);
                        });

                        if (bottom_pistons.IsMorePosition(position_target))
                        {
                            Stage++;
                        }
                    }
                    break;
                case ActionMachine.LockTop:
                    velocity = -MyProperty.locker_velocity;
                    position_target = MyProperty.locker_position_min;

                    top_mergers.On();
                    top_pistons.Velocity(velocity);
                    WriteText($"Bottom mergers: On", true);
                    WriteText($"Velocity: {velocity}", true);
                    WriteText($"Target Position={position_target}", true);
                    closed = true;

                    top_pistons.ForEach(delegate (IMyPistonBase block)
                    {
                        WriteText($"Position={block.CurrentPosition}", true);
                    });

                    if (top_pistons.IsLessPosition(position_target))
                    {
                        connector.Lock();
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
                        position_target = MyProperty.locker_position_max;

                        top_mergers.Off();
                        top_pistons.Velocity(velocity);
                        connector.Unlock();
                        WriteText($"Top mergers: Off", true);
                        WriteText($"Connector: Unlock", true);
                        WriteText($"Velocity: {velocity}", true);
                        WriteText($"Target Position={position_target}", true);
                        closed = true;

                        top_pistons.ForEach(delegate (IMyPistonBase block)
                        {
                            WriteText($"Position={block.CurrentPosition}", true);
                        });

                        if (top_pistons.IsMorePosition(position_target))
                        {
                            Stage++;
                        }
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
                    //projector.On();
                    WriteText($"Grinders: On", true);

                    Stage++;
                    break;
                case ActionMachine.StopGrinder:
                    grinders.Off();
                    //projector.Off();
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
