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
        private float epsilonAngle = 0.01f;

        private BlockSystem<IMyMotorStator> stators_pince1 = null;
        private BlockSystem<IMyMotorStator> stators_pince2 = null;
        private BlockSystem<IMyShipMergeBlock> merger_pince = null;

        private BlockSystem<IMyPistonBase> piston_pince = null;
        private BlockSystem<IMyShipMergeBlock> merger_piston = null;
        private BlockSystem<IMyShipConnector> connector_piston = null;

        private BlockSystem<IMyPistonBase> piston_levage = null;
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
            stators_pince1 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince 1");
            stators_pince2 = BlockSystem<IMyMotorStator>.SearchByName(this, "Rotor Pince 2");
            merger_pince = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Merge Block Pince");

            piston_pince = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Piston Pince");
            merger_piston = BlockSystem<IMyShipMergeBlock>.SearchByGroup(this, "Merge Block Piston");
            connector_piston = BlockSystem<IMyShipConnector>.SearchByName(this, "Connector Drill");

            piston_levage = BlockSystem<IMyPistonBase>.SearchByGroup(this, "Piston Levage");
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

                switch (command)
                {
                    case "init":
                        Init();
                        break;
                    case "stop":
                        Mode = ModeMachine.Stop;
                        break;
                    case "down":
                        Stage = 0;
                        Mode = ModeMachine.Down;
                        Sequence = new List<ActionMachine>();
                        Sequence.Add(ActionMachine.OpenPince);
                        Sequence.Add(ActionMachine.WaitOpenPince);
                        Sequence.Add(ActionMachine.Down);
                        Sequence.Add(ActionMachine.WaitDown);
                        Sequence.Add(ActionMachine.ClosePince);
                        Sequence.Add(ActionMachine.WaitClosePince);
                        Sequence.Add(ActionMachine.OpenPiston);
                        Sequence.Add(ActionMachine.WaitOpenPiston);
                        Sequence.Add(ActionMachine.Up);
                        Sequence.Add(ActionMachine.WaitUp);
                        Sequence.Add(ActionMachine.ClosePiston);
                        Sequence.Add(ActionMachine.WaitClosePiston);
                        Sequence.Add(ActionMachine.Stop);
                        break;
                    case "up":
                        Stage = 0;
                        Mode = ModeMachine.Up;
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
            if(Mode != ModeMachine.Stop)
            {
                Staging(Sequence[Stage]);
            }
        }
        private void Display()
        {
            drawingSurface.WriteText($"Machine Mode:{Mode}", false);
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
                case ActionMachine.OpenPince:
                    // deverrouillage pince
                    stators_pince1.On();
                    drawingSurface.WriteText($"\nPince 1: On", true);
                    stators_pince1.Unlock();
                    drawingSurface.WriteText($"\nPince 1: Unlock", true);
                    stators_pince2.On();
                    drawingSurface.WriteText($"\nPince 2: On", true);
                    stators_pince2.Unlock();
                    drawingSurface.WriteText($"\nPince 2: Unlock", true);
                    merger_pince.Off();
                    drawingSurface.WriteText($"\nDeverouillage pince: Off", true);
                    stators_pince1.Velocity(-0.2f);
                    stators_pince2.Velocity(0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenPince:
                    if (stators_pince1.IsPosition(345f, epsilonAngle) && stators_pince2.IsPosition(15f, epsilonAngle)) Stage++;
                    drawingSurface.WriteText($"\nPince 1: Angle={Util.RadToDeg(stators_pince1.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPince 2: Angle={Util.RadToDeg(stators_pince2.List[0].Angle)}", true);
                    break;
                case ActionMachine.ClosePince:
                    // verrouillage pince
                    stators_pince1.On();
                    drawingSurface.WriteText($"\nPince 1: On", true);
                    stators_pince1.Unlock();
                    drawingSurface.WriteText($"\nPince 1: Unlock", true);
                    stators_pince2.On();
                    drawingSurface.WriteText($"\nPince 2: On", true);
                    stators_pince2.Unlock();
                    drawingSurface.WriteText($"\nPince 2: Unlock", true);
                    merger_pince.On();
                    drawingSurface.WriteText($"\nVerouillage pince: On", true);
                    stators_pince1.Velocity(0.2f);
                    stators_pince2.Velocity(-0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitClosePince:
                    if (stators_pince1.IsPosition(360f, epsilonAngle) && stators_pince2.IsPosition(0f, epsilonAngle)) Stage++;
                    drawingSurface.WriteText($"\nPince 1: Angle={Util.RadToDeg(stators_pince1.List[0].Angle)}", true);
                    drawingSurface.WriteText($"\nPince 2: Angle={Util.RadToDeg(stators_pince2.List[0].Angle)}", true);
                    break;
                case ActionMachine.OpenPiston:
                    // deverrouillage piston
                    connector_piston.ForEach(delegate (IMyShipConnector block)
                    {
                        block.Disconnect();
                    });
                    drawingSurface.WriteText($"\nConnector: Off", true);
                    piston_pince.On();
                    drawingSurface.WriteText($"\nPiston Pince: On", true);
                    merger_piston.Off();
                    drawingSurface.WriteText($"\nDeverouillage piston: Off", true);
                    piston_pince.Velocity(-0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitOpenPiston:
                    if (piston_pince.IsPosition(1.4f)) Stage++;
                    break;
                case ActionMachine.ClosePiston:
                    // Verrouillage piston
                    piston_pince.On();
                    drawingSurface.WriteText($"\nPiston Pince: On", true);
                    merger_piston.On();
                    drawingSurface.WriteText($"\nDeverouillage piston: on", true);
                    piston_pince.Velocity(0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitClosePiston:
                    if (piston_pince.IsPosition(2.3f))
                    {
                        connector_piston.ForEach(delegate (IMyShipConnector block)
                        {
                            block.Connect();
                        });
                        drawingSurface.WriteText($"\nConnector: On", true);
                        Stage++;
                    }
                    break;
                case ActionMachine.Down:
                    piston_levage.On();
                    drawingSurface.WriteText($"\nPiston Levage: On", true);
                    piston_levage.Velocity(-0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitDown:
                    piston_levage.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                    });
                    if (piston_levage.IsPosition(0f)) Stage++;
                    break;
                case ActionMachine.Up:
                    piston_levage.On();
                    drawingSurface.WriteText($"\nPiston Levage: On", true);
                    piston_levage.Velocity(0.2f);
                    Stage++;
                    break;
                case ActionMachine.WaitUp:
                    piston_levage.List.ForEach(delegate (IMyPistonBase block)
                    {
                        drawingSurface.WriteText($"\nPiston Position={block.CurrentPosition}", true);
                    });
                    if (piston_levage.IsPosition(7.6f)) Stage++;
                    break;
                case ActionMachine.Terminated:
                    Stage = 0;
                    break;
                case ActionMachine.Stop:
                    break;
            }
        }

        public enum ModeMachine
        {
            Stop,
            Down,
            Up,
            Reset
        }
        public enum ActionMachine
        {
            OpenPince,
            WaitOpenPince,
            ClosePince,
            WaitClosePince,
            OpenPiston,
            WaitOpenPiston,
            ClosePiston,
            WaitClosePiston,
            Stop,
            Up,
            WaitUp,
            Down,
            WaitDown,
            Terminated
        }
    }
}
