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

        private BlockSystem<IMyCockpit> cockpit = null;

        private BlockSystem<IMyMotorSuspension> wheels = null;

        private BlockSystem<IMyMotorStator> stators = null;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            Init();
        }

        private void Init()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            Search();
        }

        private void Search()
        {
            BlockFilter<IMyCockpit> cockpit_filter = BlockFilter<IMyCockpit>.Create(Me, MyProperty.cockpit_filter);
            cockpit = BlockSystem<IMyCockpit>.SearchByFilter(this, cockpit_filter);

            BlockFilter<IMyMotorSuspension> wheel_filter = BlockFilter<IMyMotorSuspension>.Create(Me, MyProperty.wheel_filter);
            wheels = BlockSystem<IMyMotorSuspension>.SearchByFilter(this, wheel_filter);

            BlockFilter<IMyMotorStator> rotor_filter = BlockFilter<IMyMotorStator>.Create(Me, MyProperty.stator_filter);
            stators = BlockSystem<IMyMotorStator>.SearchByFilter(this, rotor_filter);
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

                switch (command)
                {
                    case "reset":
                        Init();
                        break;
                    default:
                        Search();
                        break;
                }
            }
        }
        void RunContinuousLogic()
        {
            Display();
            if(cockpit.IsEmpty)
            {
                WriteText("Missing Cockpit", true);
                return;
            }
            IMyCockpit mainCockpit;
            if(cockpit.List.Count > 1)
            {
                mainCockpit = cockpit.List.Where(block => block.IsMainCockpit).First();
                if(mainCockpit == null)
                {
                    WriteText("Set a main Cockpit", true);
                    return;
                }
            }
            else
            {
                mainCockpit = cockpit.First;
            }
            WriteText("Main Cockpit Found", true);

            Vector3D relPos;
            float mover, turner;
            cockpit.ForEach(delegate (IMyCockpit block) {
                if (!block.IsMainCockpit)
                {
                    block.HandBrake = mainCockpit.HandBrake;
                }
            });
            
            wheels.ForEach(delegate (IMyMotorSuspension block) {

                if (mainCockpit.HandBrake)
                {
                    mover = 0;
                }
                else if (mainCockpit.MoveIndicator.Y > 0)
                {
                    mover = -(float)Vector3D.TransformNormal(mainCockpit.GetShipVelocities().LinearVelocity, MatrixD.Transpose(mainCockpit.WorldMatrix)).Z * MyProperty.wheel_handbrake;
                }
                else
                {
                    mover = mainCockpit.MoveIndicator.Z;
                }
                turner = mainCockpit.MoveIndicator.X;
                relPos = Vector3D.TransformNormal(block.GetPosition() - mainCockpit.CenterOfMass, MatrixD.Transpose(mainCockpit.WorldMatrix));
                if (relPos.X < 0) { mover = -mover; }
                if (relPos.Z > 0) { turner = -turner; }
                block.SetValueFloat("Propulsion override", mover);
                block.SetValueFloat("Steer override", turner);
            });
        }

        public void WriteText(string message, bool append)
        {
            message += "\n";
            drawingSurface.WriteText(message, append);
        }
        private void Display()
        {
            WriteText("=== Wheel Controller ===", false);
            WriteText($"Cockpit={cockpit.List.Count}", true);
            WriteText($"Wheels={wheels.List.Count}", true);
            WriteText($"Stators={stators.List.Count}", true);
        }

        public enum StateMachine
        {
            Stopped,
            Traking,
            Running,
            Waitting
        }
    }
}
