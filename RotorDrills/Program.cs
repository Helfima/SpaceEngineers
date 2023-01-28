using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        KProperty MyProperty;

        MyCommandLine commandLine = new MyCommandLine();

        BlockSystem<IMyMotorStator> stators = null;
        BlockSystem<IMyPistonBase> pistonUp = null;
        BlockSystem<IMyPistonBase> pistonDown = null;
        BlockSystem<IMyPistonBase> pistonRayon = null;
        BlockSystem<IMyShipDrill> drills = null;

        private StateMachine stateMachine = StateMachine.Stopped;
        private Phase phase = Phase.None;

        private IMyTextSurface drawingSurface;

        private bool brasOut = true;
        private bool slowDown = false;

        private float targetRayon;
        private float deltaRayon = 2.5f;
        private float targetAngle;
        private float deltaAngle = 10f; // 15° non optimal

        private float SPEED_MIN = 0.15f*3;
        private float SPEED_MAX = 1f; // 1 trop rapide

        private float ANGLE_RPM_MIN = 0.1f;
        private float ANGLE_RPM_MAX = 0.5f;
        private float ANGLE_DELTA = 0f;
        private float ANGLE_MIN = 30f;
        private float ANGLE_MAX = 140f;

        private double quantity = 0f;
        private double lastQuantity = 0f;

        public Program()
        {
            MyProperty = new KProperty(this);
            MyProperty.Load();
            drawingSurface = Me.GetSurface(0);
            drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            // Set the continuous update frequency of this script
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            phase = Phase.None;
            Prepare();
        }

        public void Save()
        {
            MyProperty.Save();
        }

        private void Prepare()
        {
            stators = BlockSystem<IMyMotorStator>.SearchByTag(this, MyProperty.Search_Rotor);
            pistonUp = BlockSystem<IMyPistonBase>.SearchByTag(this, MyProperty.Search_Piston_Up);
            pistonDown = BlockSystem<IMyPistonBase>.SearchByTag(this, MyProperty.Search_Piston_Down);
            pistonRayon = BlockSystem<IMyPistonBase>.SearchByTag(this, MyProperty.Search_Piston_Rayon);
            drills = BlockSystem<IMyShipDrill>.SearchByTag(this, MyProperty.Search_Drill);
            SetTargetValues();
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

        void RunContinuousLogic()
        {
            quantity = DrillCount();
            if (quantity > 10 && quantity > lastQuantity) slowDown = true;
            lastQuantity = quantity;
            Display();
            if (quantity > 10000) stateMachine = StateMachine.Waitting;
            switch (stateMachine)
            {
                case StateMachine.Initializing:
                    Initializing();
                    break;
                case StateMachine.Running:
                    Running();
                    break;
                case StateMachine.RotorZero:
                    RotorZero();
                    break;
                case StateMachine.Stopping:
                    pistonUp.Off();
                    pistonDown.Off();
                    pistonRayon.Off();
                    stators.Lock();
                    drills.Off();
                    break;
                case StateMachine.Waitting:
                    pistonUp.Off();
                    pistonDown.Off();
                    pistonRayon.Off();
                    stators.Lock();
                    drills.Off();
                    if (quantity == 0) stateMachine = StateMachine.Running;
                    break;
            }
        }

        private void Running()
        {
            int factor = pistonDown.List.Count + pistonUp.List.Count;
            if (stators.IsEmpty) Prepare();

            switch (phase)
            {
                case Phase.None:
                    phase = Phase.PistonDown;
                    break;
                case Phase.PistonDown:
                    drills.On();
                    pistonDown.ForEach(delegate (IMyPistonBase block) {
                        if (slowDown) block.Velocity = SPEED_MIN / factor;
                        else block.Velocity = SPEED_MAX / factor;
                    });
                    pistonUp.ForEach(delegate (IMyPistonBase block) {
                        if (slowDown) block.Velocity = -SPEED_MIN / factor;
                        else block.Velocity = -SPEED_MAX / factor;
                    });
                    pistonDown.On();
                    pistonUp.On();
                    if (pistonDown.IsPositionMax() && pistonUp.IsPositionMin()) phase = Phase.PistonUp;
                    break;
                case Phase.PistonUp:
                    slowDown = false;
                    pistonDown.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = -SPEED_MAX;
                    });
                    pistonUp.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = SPEED_MAX;
                    });
                    pistonDown.On();
                    pistonUp.On();
                    if (pistonDown.IsPositionMin() && pistonUp.IsPositionMax()) phase = Phase.Bras;
                    break;
                case Phase.Bras:
                    if (brasOut && pistonRayon.IsPositionMax())
                    {
                        targetAngle += deltaAngle;
                        brasOut = false;
                        phase = Phase.Rotor;
                    }
                    else if (!brasOut && pistonRayon.IsPositionMin())
                    {
                        targetAngle += deltaAngle;
                        brasOut = true;
                        phase = Phase.Rotor;
                    }
                    else
                    {
                        if (brasOut)
                        {
                            targetRayon += Math.Min(deltaRayon, 10f);
                            if (targetRayon > 10f) targetRayon = 10f;
                            phase = Phase.BrasOut;
                        }
                        else
                        {
                            targetRayon -= Math.Min(deltaRayon, 10f);
                            if (targetRayon < 0f) targetRayon = 0f;
                            phase = Phase.BrasIn;
                        }
                    }
                    break;
                case Phase.BrasIn:
                    pistonRayon.On();
                    pistonRayon.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = -SPEED_MAX;
                    });
                    if (pistonRayon.IsPosition(targetRayon))
                    {
                        pistonRayon.Off();
                        phase = Phase.PistonDown;
                    }
                    break;
                case Phase.BrasOut:
                    pistonRayon.On();
                    pistonRayon.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = SPEED_MAX;
                    });
                    if (pistonRayon.IsPosition(targetRayon))
                    {
                        pistonRayon.Off();
                        phase = Phase.PistonDown;
                    }
                    break;
                case Phase.Rotor:
                    stators.ForEach(delegate (IMyMotorStator block) {
                        block.TargetVelocityRPM = ANGLE_RPM_MIN;
                    });
                    stators.Unlock();
                    if (stators.IsPositionMax())
                    {
                        stateMachine = StateMachine.Stopping;
                        phase = Phase.None;
                    }
                    else if (stators.IsPosition(targetAngle))
                    {
                        stators.Lock();
                        phase = Phase.PistonDown;
                    }
                    break;
            }
        }

        private void Initializing()
        {
            switch (phase)
            {
                case Phase.None:
                    targetRayon = 0f;
                    targetAngle = ANGLE_MIN;
                    phase = Phase.PistonUp;
                    break;
                case Phase.PistonUp:
                    pistonDown.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = -SPEED_MAX;
                    });
                    pistonUp.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = SPEED_MAX;
                    });
                    pistonDown.On();
                    pistonUp.On();
                    if (pistonDown.IsPositionMin() && pistonUp.IsPositionMax()) phase = Phase.Rotor;
                    break;
                case Phase.Rotor:
                    InitRotor();
                    if (stators.IsPositionMin())
                    {
                        stators.Lock();
                        phase = Phase.BrasIn;
                    }
                    break;
                case Phase.BrasIn:
                    pistonRayon.On();
                    pistonRayon.ForEach(delegate (IMyPistonBase block) {
                        block.Velocity = -SPEED_MAX;
                    });
                    if (pistonRayon.IsPositionMin())
                    {
                        drills.Off();
                        phase = Phase.None;
                        stateMachine = StateMachine.Stopped;
                    }
                    break;
            }
        }

        private void RotorZero()
        {

            switch (phase)
            {
                case Phase.None:
                    phase = Phase.PistonUp;
                    stators.ForEach(delegate (IMyMotorStator block) {
                        block.ApplyAction("OnOff_On");
                        block.TargetVelocityRPM = -ANGLE_RPM_MAX;
                        block.LowerLimitDeg = -360f;
                        block.UpperLimitDeg = 360f;
                        block.RotorLock = false;
                        targetAngle = 0f;
                    });
                    break;
                case Phase.Rotor:
                    if (stators.IsPosition(targetAngle))
                    {
                        stators.Lock();
                        phase = Phase.None;
                        stateMachine = StateMachine.Stopped;
                    }
                    break;
            }
        }

        private void Display()
        {
            drawingSurface.WriteText($"Machine Status:{stateMachine}", false);
            drawingSurface.WriteText($"\nPhase:{phase}", true);
            drawingSurface.WriteText($"\nTarget Rayon:{Math.Round(targetRayon, 2)}", true);
            drawingSurface.WriteText($"\nTarget Angle:{Math.Round(targetAngle, 2)}", true);
            drawingSurface.WriteText($"\nDrill Quantity:{Util.GetKiloFormat(quantity)}", true);

            pistonUp.ForEach(delegate (IMyPistonBase block) {
                drawingSurface.WriteText($"\n{block.CustomName}: {block.Velocity} | P={Math.Round(block.CurrentPosition, 2)} | On={block.IsWorking}", true);
            });
            pistonDown.ForEach(delegate (IMyPistonBase block) {
                drawingSurface.WriteText($"\n{block.CustomName}: {block.Velocity} | P={Math.Round(block.CurrentPosition, 2)} | On={block.IsWorking}", true);
            });
            pistonRayon.ForEach(delegate (IMyPistonBase block) {
                drawingSurface.WriteText($"\n{block.CustomName}: {block.Velocity} | P={Math.Round(block.CurrentPosition, 2)} | On={block.IsWorking}", true);
            });
            stators.ForEach(delegate (IMyMotorStator block) {
                drawingSurface.WriteText($"\n{block.CustomName}: {block.TargetVelocityRPM} | A°={Math.Round(Util.RadToDeg(block.Angle) + ANGLE_DELTA, 2)} | Lock={block.RotorLock}", true);
            });
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
                    case "start":
                        slowDown = false;
                        stateMachine = StateMachine.Running;
                        break;
                    case "stop":
                        stateMachine = StateMachine.Stopping;
                        break;
                    case "reset":
                        Prepare();
                        stateMachine = StateMachine.Initializing;
                        break;
                    case "prepare":
                        Prepare();
                        break;
                    case "rotorZero":
                        stateMachine = StateMachine.RotorZero;
                        phase = Phase.None;
                        break;
                    default:
                        Prepare();
                        break;
                }
            }
        }

        private void SetTargetValues()
        {
            pistonRayon.ForEach(delegate (IMyPistonBase block) {
                targetRayon = block.CurrentPosition;
            });
            stators.ForEach(delegate (IMyMotorStator block) {
                targetAngle = float.Parse(Util.RadToDeg(block.Angle).ToString());
            });
        }

        private void InitRotor()
        {
            stators.ForEach(delegate (IMyMotorStator block) {
                block.ApplyAction("OnOff_On");
                block.TargetVelocityRPM = -ANGLE_RPM_MAX;
                block.LowerLimitDeg = ANGLE_MIN;
                block.UpperLimitDeg = ANGLE_MAX;
                block.RotorLock = false;
                targetAngle = block.LowerLimitDeg;
            });
        }

        private double DrillCount()
        {
            if (drills == null || drills.IsEmpty) return 0;
            double count = 0;
            foreach (IMyShipDrill drill in drills.List)
            {
                IMyInventory drill_inventory = drill.GetInventory(0);
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                drill_inventory.GetItems(items);
                foreach (MyInventoryItem item in items)
                {
                    double amount = 0;
                    Double.TryParse(item.Amount.ToString(), out amount);
                    count += amount;
                }
            }
            return count;
        }

        public enum StateMachine
        {
            Stopped,
            Stopping,
            Initializing,
            Running,
            RotorZero,
            Waitting
        }

        public enum Phase
        {
            None,
            PistonUp,
            PistonDown,
            Rotor,
            BrasIn,
            Bras,
            BrasOut
        }
        
    }
}
