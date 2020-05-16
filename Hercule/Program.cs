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

        private ModeMachine Mode = ModeMachine.Stop;
        private List<ActionMachine> Sequence;
        private int Stage = 0;
        private int Cycle = 0;
        private int projector_count = 0;
        private int blueprint_count = 0;
        private float epsilonAngle = 0.01f;
        private float current_position = 0f;
        private float last_position = 0f;
        private float velocity = 0f;
        private float rpm = 1.2f;
        private float velocity_tool = 4f;

        private BlockSystem<IMyMotorStator> stators_pince_fixe_1 = null;
        private BlockSystem<IMyMotorStator> stators_pince_fixe_2 = null;
        private BlockSystem<IMyShipMergeBlock> merger_pince_fixe = null;

        private BlockSystem<IMyMotorStator> stators_pince_mobile_1 = null;
        private BlockSystem<IMyMotorStator> stators_pince_mobile_2 = null;
        private BlockSystem<IMyShipMergeBlock> merger_pince_mobile = null;

        private BlockSystem<IMyShipConnector> connector_drill = null;

        private BlockSystem<IMyPistonBase> piston_connector_drill = null;

        private BlockSystem<IMyMotorStator> stators_levage = null;
        private BlockSystem<IMyPistonBase> piston_levage = null;

        private BlockSystem<IMyPistonBase> piston_grinder = null;
        private BlockSystem<IMyShipGrinder> grinder = null;
        private BlockSystem<IMyShipGrinder> grinder_middle1 = null;
        private BlockSystem<IMyShipGrinder> grinder_middle2 = null;

        private BlockSystem<IMyPistonBase> piston_welder = null;
        private BlockSystem<IMyShipWelder> welder = null;

        private BlockSystem<IMyProjector> projector = null;

        private BlockSystem<IMyLightingBlock> light = null;
        private BlockSystem<IMyShipDrill> drill = null;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
        }

        private void Init()
        {
            Stage = 0;
            stators_pince_fixe_1 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince Fixe 1");
            stators_pince_fixe_2 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince Fixe 2");
            merger_pince_fixe = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Merge Pince Fixe");

            stators_pince_mobile_1 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince Mobile 1");
            stators_pince_mobile_2 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince Mobile 2");
            merger_pince_mobile = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Merge Pince Mobile");

            connector_drill = BlockSystem<IMyShipConnector>.SearchByName(this, "Connector Drill");
            
            piston_connector_drill = BlockSystem<IMyPistonBase>.SearchByName(this, "Piston Connector Drill");

            piston_levage = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Piston Levage");
            stators_levage = BlockSystem<IMyMotorStator>.SearchByGroup(this, "Advanced Rotor Levage");

            piston_grinder = BlockSystem<IMyPistonBase>.SearchByName(this, "Piston Grinder");
            grinder = BlockSystem<IMyShipGrinder>.SearchByGroup(this, "Grinder");
            grinder_middle1 = BlockSystem<IMyShipGrinder>.SearchByName(this, "Grinder Middle 1");
            grinder_middle2 = BlockSystem<IMyShipGrinder>.SearchByGroup(this, "Grinder Middle 2");

            piston_welder = BlockSystem<IMyPistonBase>.SearchByName(this, "Piston Welder");
            welder = BlockSystem<IMyShipWelder>.SearchByGroup(this, "Welder");

            projector = BlockSystem<IMyProjector>.SearchByName(this, "Projector Drill");

            projector.List[0].ProjectionOffset = new Vector3I(1, 1 + 3 * blueprint_count, 0);

            light = BlockSystem<IMyLightingBlock>.SearchByGroup(this, "Rotating Light");

            drill = BlockSystem<IMyShipDrill>.SearchByGroup(this, "Drills");

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
                    case "stop":
                        Mode = ModeMachine.Stop;
                        break;
                    case "open":
                        Stage = 0;
                        Mode = ModeMachine.Open;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenPinceFixe);
                        Sequence.Add(ActionMachine.WaitOpenPinceFixe);
                        Sequence.Add(ActionMachine.OpenPinceMobile);
                        Sequence.Add(ActionMachine.WaitOpenPinceMobile);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "openfixe":
                        Stage = 0;
                        Mode = ModeMachine.OpenFixe;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenPinceFixe);
                        Sequence.Add(ActionMachine.WaitOpenPinceFixe);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "openmobile":
                        Stage = 0;
                        Mode = ModeMachine.OpenMobile;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenPinceMobile);
                        Sequence.Add(ActionMachine.WaitOpenPinceMobile);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "close":
                        Stage = 0;
                        Mode = ModeMachine.Close;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.ClosePinceFixe);
                        Sequence.Add(ActionMachine.WaitClosePinceFixe);
                        Sequence.Add(ActionMachine.ClosePinceMobile);
                        Sequence.Add(ActionMachine.WaitClosePinceMobile);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "closefixe":
                        Stage = 0;
                        Mode = ModeMachine.CloseFixe;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.ClosePinceFixe);
                        Sequence.Add(ActionMachine.WaitClosePinceFixe);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "closemobile":
                        Stage = 0;
                        Mode = ModeMachine.CloseMobile;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.ClosePinceMobile);
                        Sequence.Add(ActionMachine.WaitClosePinceMobile);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "opengrinder":
                        Stage = 0;
                        Mode = ModeMachine.OpenGrinder;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenGrinder);
                        Sequence.Add(ActionMachine.WaitOpenGrinder);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "closegrinder":
                        Stage = 0;
                        Mode = ModeMachine.CloseGrinder;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.CloseGrinder);
                        Sequence.Add(ActionMachine.WaitCloseGrinder);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "openwelder":
                        Stage = 0;
                        Mode = ModeMachine.OpenWelder;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenWelder);
                        Sequence.Add(ActionMachine.WaitOpenWelder);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "closewelder":
                        Stage = 0;
                        Mode = ModeMachine.CloseWelder;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.CloseWelder);
                        Sequence.Add(ActionMachine.WaitCloseWelder);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "down":
                        Stage = 0;
                        Mode = ModeMachine.Down;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenPinceMobile);
                        Sequence.Add(ActionMachine.WaitOpenPinceMobile);

                        Sequence.Add(ActionMachine.OpenWelder);
                        Sequence.Add(ActionMachine.WaitOpenWelder);

                        Sequence.Add(ActionMachine.Up);
                        Sequence.Add(ActionMachine.WaitUpWelder1);
                        Sequence.Add(ActionMachine.WaitUpWelder2);
                        Sequence.Add(ActionMachine.WaitUpWelder3);
                        Sequence.Add(ActionMachine.WaitUp);
                        
                        Sequence.Add(ActionMachine.CloseWelder);
                        Sequence.Add(ActionMachine.WaitCloseWelder);
                        
                        Sequence.Add(ActionMachine.ClosePinceMobile);
                        Sequence.Add(ActionMachine.WaitClosePinceMobile);
                        
                        Sequence.Add(ActionMachine.OpenPinceFixe);
                        Sequence.Add(ActionMachine.WaitOpenPinceFixe);
                        
                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.WaitDown);
                        
                        Sequence.Add(ActionMachine.ClosePinceFixe);
                        Sequence.Add(ActionMachine.WaitClosePinceFixe);
                        Sequence.Add(ActionMachine.Stop);

                        Sequence.Add(ActionMachine.Terminated);
                        break;
                    case "up":
                        Stage = 0;
                        Mode = ModeMachine.Up;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.Start);

                        Sequence.Add(ActionMachine.OpenPinceFixe);
                        Sequence.Add(ActionMachine.WaitOpenPinceFixe);

                        Sequence.Add(ActionMachine.Up);
                        Sequence.Add(ActionMachine.WaitUp);

                        Sequence.Add(ActionMachine.ClosePinceFixe);
                        Sequence.Add(ActionMachine.WaitClosePinceFixe);

                        Sequence.Add(ActionMachine.OpenPinceMobile);
                        Sequence.Add(ActionMachine.WaitOpenPinceMobile);

                        Sequence.Add(ActionMachine.OpenGrinder);
                        Sequence.Add(ActionMachine.WaitOpenGrinder);

                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.WaitDownGrinder1);
                        Sequence.Add(ActionMachine.WaitDownGrinder2);
                        Sequence.Add(ActionMachine.WaitDownGrinder3);
                        Sequence.Add(ActionMachine.WaitDownGrinder4);
                        Sequence.Add(ActionMachine.WaitDownGrinder5);
                        Sequence.Add(ActionMachine.WaitDown);

                        Sequence.Add(ActionMachine.CloseGrinder);
                        Sequence.Add(ActionMachine.WaitCloseGrinder);

                        Sequence.Add(ActionMachine.ClosePinceMobile);
                        Sequence.Add(ActionMachine.WaitClosePinceMobile);
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
            if(Mode != ModeMachine.Stop && Cycle != 0)
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
        }

        private void Staging(ActionMachine action)
        {
            switch (action)
            {
                case ActionMachine.OpenPinceFixe:
                    // Open pince
                    stators_pince_fixe_1.On();
                    drawingSurface.WriteText($"\nPince Fixe 1: On", true);
                    stators_pince_fixe_1.Unlock();
                    drawingSurface.WriteText($"\nPince Fixe 1: Unlock", true);
                    stators_pince_fixe_2.On();
                    drawingSurface.WriteText($"\nPince Fixe 2: On", true);
                    stators_pince_fixe_2.Unlock();
                    drawingSurface.WriteText($"\nPince Fixe 2: Unlock", true);
                    merger_pince_fixe.Off();
                    drawingSurface.WriteText($"\nMerge Pince Fixe: Off", true);
                    stators_pince_fixe_1.Velocity(rpm);
                    stators_pince_fixe_2.Velocity(-rpm);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenPinceFixe:
                    if (stators_pince_fixe_1.IsPosition(15f, epsilonAngle) && stators_pince_fixe_2.IsPosition(345f, epsilonAngle))
                    {
                        stators_pince_fixe_1.Lock();
                        stators_pince_fixe_2.Lock();
                        Stage++;
                    }
                    drawingSurface.WriteText($"\nPince Fixe 1: Angle={Util.RadToDeg(stators_pince_fixe_1.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPince Fixe 2: Angle={Util.RadToDeg(stators_pince_fixe_2.List[0].Angle)}", true);
                    break;
                case ActionMachine.ClosePinceFixe:
                    // Close pince
                    stators_pince_fixe_1.On();
                    drawingSurface.WriteText($"\nPince Fixe 1: On", true);
                    stators_pince_fixe_1.Unlock();
                    drawingSurface.WriteText($"\nPince Fixe 1: Unlock", true);
                    stators_pince_fixe_2.On();
                    drawingSurface.WriteText($"\nPince Fixe 2: On", true);
                    stators_pince_fixe_2.Unlock();
                    drawingSurface.WriteText($"\nPince Fixe 2: Unlock", true);
                    merger_pince_fixe.On();
                    drawingSurface.WriteText($"\nMerge Pince Fixe: On", true);
                    stators_pince_fixe_1.Velocity(-rpm);
                    stators_pince_fixe_2.Velocity(rpm);
                    Stage++;
                    break;
                case ActionMachine.WaitClosePinceFixe:
                    if (stators_pince_fixe_1.IsPosition(0f, epsilonAngle) && stators_pince_fixe_2.IsPosition(360f, epsilonAngle))
                    {
                        stators_pince_fixe_1.Lock();
                        stators_pince_fixe_2.Lock();
                        Stage++;
                    }
                    drawingSurface.WriteText($"\nPince Fixe 1: Angle={Util.RadToDeg(stators_pince_fixe_1.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPince Fixe 2: Angle={Util.RadToDeg(stators_pince_fixe_2.List[0].Angle)}", true);
                    break;
                case ActionMachine.OpenPinceMobile:
                    // Open pince
                    stators_pince_mobile_1.On();
                    drawingSurface.WriteText($"\nPince Mobile 1: On", true);
                    stators_pince_mobile_1.Unlock();
                    drawingSurface.WriteText($"\nPince Mobile 1: Unlock", true);
                    stators_pince_mobile_2.On();
                    drawingSurface.WriteText($"\nPince Mobile 2: On", true);
                    stators_pince_mobile_2.Unlock();
                    drawingSurface.WriteText($"\nPince Mobile 2: Unlock", true);
                    merger_pince_mobile.Off();
                    drawingSurface.WriteText($"\nMerge Pince Mobile: Off", true);
                    stators_pince_mobile_1.Velocity(-rpm);
                    stators_pince_mobile_2.Velocity(rpm);
                    connector_drill.ForEach(delegate (IMyShipConnector block)
                    {
                        block.Disconnect();
                    });
                    drawingSurface.WriteText($"\nConnector Drill: Disconnect", true);
                    piston_connector_drill.Velocity(-rpm);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenPinceMobile:
                    if (stators_pince_mobile_1.IsPosition(345f, epsilonAngle)
                        && stators_pince_mobile_2.IsPosition(15f, epsilonAngle)
                        && piston_connector_drill.IsPosition(0f))
                    {
                        stators_pince_mobile_1.Lock();
                        stators_pince_mobile_2.Lock();
                        Stage++;
                    }
                    drawingSurface.WriteText($"\nPince Mobile 1: Angle={Util.RadToDeg(stators_pince_mobile_1.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPince Mobile 2: Angle={Util.RadToDeg(stators_pince_mobile_2.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPiston Connector Drill: Position={piston_connector_drill.List[0].CurrentPosition}", true);
                    break;
                case ActionMachine.ClosePinceMobile:
                    // Close pince
                    stators_pince_mobile_1.On();
                    drawingSurface.WriteText($"\nPince Mobile 1: On", true);
                    stators_pince_mobile_1.Unlock();
                    drawingSurface.WriteText($"\nPince Mobile 1: Unlock", true);
                    stators_pince_mobile_2.On();
                    drawingSurface.WriteText($"\nPince Mobile 2: On", true);
                    stators_pince_mobile_2.Unlock();
                    drawingSurface.WriteText($"\nPince Mobile 2: Unlock", true);
                    merger_pince_mobile.On();
                    drawingSurface.WriteText($"\nMerge Mobile Fixe: On", true);
                    stators_pince_mobile_1.Velocity(rpm);
                    stators_pince_mobile_2.Velocity(-rpm);
                    piston_connector_drill.Velocity(rpm);
                    Stage++;
                    break;
                case ActionMachine.WaitClosePinceMobile:
                    if (stators_pince_mobile_1.IsPosition(360f, epsilonAngle)
                        && stators_pince_mobile_2.IsPosition(0f, epsilonAngle)
                        && piston_connector_drill.IsPosition(2.35f))
                    {
                        stators_pince_mobile_1.Lock();
                        stators_pince_mobile_2.Lock();
                        connector_drill.ForEach(delegate (IMyShipConnector block)
                        {
                            block.Connect();
                        });
                        drawingSurface.WriteText($"\nConnector Drill: Connected", true);
                        Stage++;
                    };
                    drawingSurface.WriteText($"\nPince Mobile 1: Angle={Util.RadToDeg(stators_pince_mobile_1.List[0].Angle)}|{stators_pince_mobile_1.IsPosition(360f, epsilonAngle)}", true);
                    drawingSurface.WriteText($"\nPince Mobile 2: Angle={Util.RadToDeg(stators_pince_mobile_2.List[0].Angle)}|{stators_pince_mobile_2.IsPosition(0f, epsilonAngle)}", true);
                    drawingSurface.WriteText($"\nPiston Connector Drill: Position={piston_connector_drill.List[0].CurrentPosition}|{piston_connector_drill.IsPosition(2.35f)}", true);
                    break;
                case ActionMachine.Down:
                    piston_levage.On();
                    drawingSurface.WriteText($"\nPiston Levage: On", true);
                    velocity = -0.2f;
                    if(Mode == ModeMachine.Up) velocity = -0.5f;
                    piston_levage.Velocity(velocity);
                    if (Mode == ModeMachine.Up)
                    {
                        blueprint_count -= 1;
                        projector.List[0].ProjectionOffset = new Vector3I(1, 1 + 3 * blueprint_count, 0);
                    }
                    Stage++;
                    break;
                case ActionMachine.WaitDownGrinder1:
                    if (Mode == ModeMachine.Up)
                    {
                        grinder.On();
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.IsPosition(5.2f))
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 5)
                        {
                            Stage++;
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitDownGrinder2:
                    if (Mode == ModeMachine.Up)
                    {
                        grinder_middle2.On();
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (projector_count == 7)
                        {
                            Stage++;
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitDownGrinder3:
                    if (Mode == ModeMachine.Up)
                    {
                        grinder_middle1.On();
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (projector_count == 8)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitDownGrinder4:
                    if (Mode == ModeMachine.Up)
                    {
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.IsPosition(2.6f))
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 9)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitDownGrinder5:
                    if (Mode == ModeMachine.Up)
                    {
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.IsPosition(0.5f))
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 10)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitDown:
                    current_position = 0f;
                    piston_levage.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        current_position = block.CurrentPosition;
                    });
                    if (piston_levage.IsPosition(0f))
                    {
                        Stage++;
                    }
                    else
                    {
                        if(last_position == current_position)
                        {
                            stators_levage.ForEach(delegate (IMyMotorStator block)
                            {
                                if (block.Displacement == -0.4f) block.Displacement = -0.3f;
                                else block.Displacement = -0.4f;
                            });
                        }
                        else
                        {
                            stators_levage.ForEach(delegate (IMyMotorStator block)
                            {
                                block.Displacement = -0.4f;
                            });
                        }
                        last_position = current_position;
                    }
                    break;
                case ActionMachine.Up:
                    piston_levage.On();
                    drawingSurface.WriteText($"\nPiston Levage: On", true);
                    velocity = 1f;
                    piston_levage.Velocity(velocity);
                    Stage++;
                    break;
                case ActionMachine.WaitUpWelder1:
                    if (Mode == ModeMachine.Down)
                    {
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.List[0].CurrentPosition > 0.5f)
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 9)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitUpWelder2:
                    if (Mode == ModeMachine.Down)
                    {
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.List[0].CurrentPosition > 2.6f)
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 8)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitUpWelder3:
                    if (Mode == ModeMachine.Down)
                    {
                        piston_levage.List.ForEach(delegate (IMyPistonBase block)
                        {
                            drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        });
                        projector_count = projector.List[0].RemainingBlocks;
                        drawingSurface.WriteText($"\nProjector Count={projector_count}", true);
                        if (piston_levage.List[0].CurrentPosition > 5.2f)
                        {
                            piston_levage.Velocity(0f);
                        }
                        if (projector_count == 0)
                        {
                            Stage++;
                            piston_levage.Velocity(velocity);
                        }
                    }
                    else
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.WaitUp:
                    current_position = 0f;
                    piston_levage.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                        current_position = block.CurrentPosition;
                    });
                    if (piston_levage.IsPosition(7.6f))
                    {
                        Stage++;
                        if(Mode == ModeMachine.Down)
                        {
                            blueprint_count += 1;
                            projector.List[0].ProjectionOffset = new Vector3I(1, 1 + 3 * blueprint_count, 0);
                        }
                    }
                    else
                    {
                        if (last_position == current_position)
                        {
                            stators_levage.ForEach(delegate (IMyMotorStator block)
                            {
                                if (block.Displacement == -0.4f) block.Displacement = -0.3f;
                                else block.Displacement = -0.4f;
                            });
                        }
                        else
                        {
                            stators_levage.ForEach(delegate (IMyMotorStator block)
                            {
                                block.Displacement = -0.4f;
                            });
                        }
                        last_position = current_position;
                    }
                    break;
                case ActionMachine.OpenGrinder:
                    piston_grinder.On();
                    drawingSurface.WriteText($"\nPiston Grinder: On", true);
                    velocity = velocity_tool;
                    piston_grinder.Velocity(velocity);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenGrinder:
                    piston_grinder.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Grinder Position={block.CurrentPosition}", true);
                    });
                    if (piston_grinder.IsPosition(10f))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.CloseGrinder:
                    grinder.Off();
                    grinder_middle1.Off();
                    grinder_middle2.Off();
                    piston_grinder.On();
                    drawingSurface.WriteText($"\nPiston Grinder: On", true);
                    velocity = velocity_tool;
                    piston_grinder.Velocity(-velocity);
                    Stage++;
                    break;
                case ActionMachine.WaitCloseGrinder:
                    piston_grinder.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Grinder Position={block.CurrentPosition}", true);
                    });
                    if (piston_grinder.IsPosition(0f))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.OpenWelder:
                    piston_welder.On();
                    drawingSurface.WriteText($"\nPiston Welder: On", true);
                    velocity = velocity_tool;
                    piston_welder.Velocity(velocity);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenWelder:
                    piston_welder.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Welder Position={block.CurrentPosition}", true);
                    });
                    if (piston_welder.IsPosition(10f))
                    {
                        welder.On();
                        Stage++;
                    }
                    break;
                case ActionMachine.CloseWelder:
                    welder.Off();
                    piston_welder.On();
                    drawingSurface.WriteText($"\nPiston Welder: On", true);
                    velocity = velocity_tool;
                    piston_welder.Velocity(-velocity);
                    Stage++;
                    break;
                case ActionMachine.WaitCloseWelder:
                    piston_welder.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Welder Position={block.CurrentPosition}", true);
                    });
                    if (piston_welder.IsPosition(0f))
                    {
                        Stage++;
                    }
                    break;
                case ActionMachine.Terminated:
                    Cycle -= 1;
                    if(Cycle == 0) Mode = ModeMachine.Stop;
                    Stage = 0;
                    break;
                case ActionMachine.Start:
                    light.On();
                    drill.On();
                    Stage++;
                    break;
                case ActionMachine.Stop:
                    light.Off();
                    drill.Off();
                    Stage++;
                    break;
            }
        }

        public enum ModeMachine
        {
            Stop,
            Down,
            Up,
            Reset,
            Open,
            OpenFixe,
            OpenMobile,
            OpenGrinder,
            OpenWelder,
            Close,
            CloseFixe,
            CloseMobile,
            CloseGrinder,
            CloseWelder
        }
        public enum ActionMachine
        {
            OpenPinceFixe,
            WaitOpenPinceFixe,
            ClosePinceFixe,
            WaitClosePinceFixe,
            OpenPinceMobile,
            WaitOpenPinceMobile,
            ClosePinceMobile,
            WaitClosePinceMobile,
            OpenGrinder,
            WaitOpenGrinder,
            CloseGrinder,
            WaitCloseGrinder,
            OpenWelder,
            WaitOpenWelder,
            CloseWelder,
            WaitCloseWelder,
            Stop,
            Start,
            Up,
            WaitUp,
            WaitUpWelder1,
            WaitUpWelder2,
            WaitUpWelder3,
            Down,
            WaitDown,
            WaitDownGrinder1,
            WaitDownGrinder2,
            WaitDownGrinder3,
            WaitDownGrinder4,
            WaitDownGrinder5,
            Terminated
        }
    }
}
