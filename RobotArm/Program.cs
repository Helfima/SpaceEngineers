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
        private StateMachine stateMachine = StateMachine.Stopped;
        private Console console;
        public Program()
        {
            var drawingSurface = Me.GetSurface(0);
            console = new Console(drawingSurface);
            
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
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

        private BlockSystem<IMyMotorStator> base_stator = null;
        private BlockSystem<IMyMotorStator> axe_stators = null;
        private BlockSystem<IMyTerminalBlock> headers = null;
        private BlockSystem<IMyTerminalBlock> reperes = null;
        private bool search = true;
        
        private void Search()
        {
            BlockFilter<IMyMotorStator> block_filter = BlockFilter<IMyMotorStator>.Create(Me, "CM:Pivot");
            base_stator = BlockSystem<IMyMotorStator>.SearchByFilter(this, block_filter);

            //BlockFilter<IMyMotorStator> axe_filter = BlockFilter<IMyMotorStator>.Create(Me, "CM:Axe");
            axe_stators = new BlockSystem<IMyMotorStator>(this);

            BlockFilter<IMyTerminalBlock> header_filter = BlockFilter<IMyTerminalBlock>.Create(Me, "CM:Header");
            headers = BlockSystem<IMyTerminalBlock>.SearchByFilter(this, header_filter);

            BlockFilter<IMyTerminalBlock> repere_filter = BlockFilter<IMyTerminalBlock>.Create(Me, "CM:Repere");
            reperes = BlockSystem<IMyTerminalBlock>.SearchByFilter(this, repere_filter);

            if (base_stator.IsEmpty == false)
            {
                FollowArms(base_stator.List[0]);
            }

            search = false;
        }
        private void FollowArms(IMyMotorStator pivot, int index = 1)
        {
            Echo($"Pivot: {pivot.CustomName}");
            var attachable = pivot.Top;
            var blocks = BlockSystem<IMyMotorStator>.SearchByGrid(this, attachable.CubeGrid);
            var nextPivot = blocks.List.FirstOrDefault();
            if(nextPivot != null)
            {
                nextPivot.CustomName = $"Axe {index}";
                axe_stators.List.Add(nextPivot);
                FollowArms(nextPivot, index + 1);
            }
        }
        private void Display()
        {
            console.Clear();
            console.WriteLine($"Pivot list size: {base_stator.List.Count}");
            console.WriteLine($"Axe list size: {axe_stators.List.Count}");
            console.WriteLine($"Repere list size: {reperes.List.Count}");
            console.WriteLine($"Status: {stateMachine}");
        }
        private void DisplayRepere()
        {
            if (base_stator.List.Count > 0)
            {
                var pivot = base_stator.List[0];
                var worldMatrix = pivot.WorldMatrix;
                if (reperes.List.Count > 0)
                {
                    foreach (var repere in reperes.List)
                    {
                        var positionRepere = Vector3D.Transform(repere.GetPosition(), MatrixD.Invert(worldMatrix));
                        console.WriteLine($"Repere: {repere.CustomName} Position: {Console.RoundVector(positionRepere, 2)}");
                    }
                }
            }
        }
        void RunContinuousLogic()
        {
            if (search) Search();
            Display();
            DisplayRepere();
            RunRobot();
            RunZero();
            TestQuaternion();
        }

        MyCommandLine commandLine = new MyCommandLine();
        private void RunCommand(string argument)
        {
            if (argument != null)
            {
                commandLine.TryParse(argument);
                var command = commandLine.Argument(0);

                switch (command)
                {
                    case "target":
                        {
                            var values = argument.Split(' ');
                            string x = values[1];
                            string y = values[2];
                            string z = values[3];
                            var target = new Vector3();
                            target.X = float.Parse(x);
                            target.Y = float.Parse(y);
                            target.Z = float.Parse(z);
                            Target = target;
                            stateMachine = StateMachine.Running;
                        }
                        break;
                    case "start":
                        {
                            stateMachine = StateMachine.Running;
                        }
                        break;
                    case "zero":
                        {
                            stateMachine = StateMachine.RotorZero;
                        }
                        break;
                    case "stop":
                        {
                            stateMachine = StateMachine.Stopped;
                        }
                        break;
                    case "quaternion":
                        {
                            stateMachine = StateMachine.Quaternion;
                        }
                        break;
                    case "search":
                        Search();
                        break;
                    case "gettype":
                        string name = commandLine.Argument(1);
                        DiplayGetType(name);
                        break;
                    default:

                        break;
                }
            }
        }

        private void TestQuaternion()
        {
            if (stateMachine != StateMachine.Quaternion) return;
            var vector = new Vector3(1, 0, 0);
            var quaternion = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), PI / 2);
            var matrix = Matrix.CreateFromQuaternion(quaternion);
            var target = quaternion * vector;
            console.WriteLine($"Origin: {vector}");
            console.WriteLine($"Quaternion: {quaternion}");
            console.WriteLine($"Target: {target}");
        }
        private void RunZero()
        {
            if (stateMachine != StateMachine.RotorZero) return;
            var header = headers.List[0];
            var pivot = base_stator.List[0];
            var origin = pivot.GetPosition();

            var worldMatrix = pivot.WorldMatrix;

            Vector3D headerPosition = Vector3D.Transform(header.GetPosition(), MatrixD.Invert(worldMatrix));

            var joints = new List<Kinematic.RobotJoint>();
            // premier rotor
            var jointPivot = new Kinematic.RobotJoint();
            jointPivot.Item = pivot;
            jointPivot.Angle = pivot.Angle;
            jointPivot.TargetAngle = 0f;
            jointPivot.Position = new Vector3();
            jointPivot.MaxAngle = 2 * PI;
            jointPivot.MinAngle = 0f;
            jointPivot.StartOffset = new Vector3(0f, 0f, 0f);
            jointPivot.Axis = new Vector3(0f, 1f, 0f);
            joints.Add(jointPivot);

            foreach (var axe in axe_stators.List)
            {
                var positionAxe = Vector3D.Transform(axe.GetPosition(), MatrixD.Invert(worldMatrix));
                var jointAxe = new Kinematic.RobotJoint();
                jointAxe.Item = axe;
                jointAxe.Angle = axe.Angle;
                jointAxe.TargetAngle = MathHelper.ToRadians(5);
                jointAxe.Position = positionAxe;
                jointAxe.MaxAngle = PI / 4;
                jointAxe.MinAngle = -PI / 4;
                jointAxe.StartOffset = new Vector3(5f, 0f, 0f);
                jointAxe.Axis = new Vector3(0f, 1f, 0f);
                joints.Add(jointAxe);
            }

            console.WriteLine($"Origin: {Console.RoundVector(origin, 2)}");
            console.WriteLine($"Header: {Console.RoundVector(headerPosition, 2)}");
            console.WriteLine($"Target: {Target}");
            foreach (var joint in joints)
            {
                console.WriteLine($"Item: {joint.Item.CustomName} Angle: {joint.AngleDeg} TargetAngle: {joint.TargetAngleDeg}");
                joint.Apply();
            }
        }

        private Vector3 Target = new Vector3(0, 5, 5);
        const float PI = (float)Math.PI;
        private void RunRobot()
        {
            if (stateMachine != StateMachine.Running) return;
            var header = headers.List[0];
            var pivot = base_stator.List[0];
            var origin = pivot.GetPosition();

            var worldMatrix = pivot.WorldMatrix;

            Vector3D headerPosition = Vector3D.Transform(header.GetPosition(), MatrixD.Invert(worldMatrix));

            var joints = new List<Kinematic.RobotJoint>();
            // premier rotor
            var jointPivot = new Kinematic.RobotJoint();
            jointPivot.Item = pivot;
            jointPivot.Angle = pivot.Angle;
            jointPivot.Position = new Vector3();
            jointPivot.MaxAngle = 2*PI;
            jointPivot.MinAngle = 0f;
            jointPivot.StartOffset = new Vector3(0f, 2.5f, 0f);
            jointPivot.Axis = new Vector3(0f, 1f, 0f);
            joints.Add(jointPivot);

            foreach(var axe in axe_stators.List)
            {
                var positionAxe = Vector3D.Transform(axe.GetPosition(), MatrixD.Invert(worldMatrix));
                var jointAxe = new Kinematic.RobotJoint();
                jointAxe.Item = axe;
                jointAxe.Angle = axe.Angle;
                jointAxe.MaxAngle = PI / 4;
                jointAxe.MinAngle = -PI / 4;
                jointAxe.Position = positionAxe;
                jointAxe.StartOffset = new Vector3(0f, 5f, 0f); // decalage de tête
                jointAxe.Axis = new Vector3(1f, 0f, 0f);
                joints.Add(jointAxe);
            }
            // calcul decalage
            for (var i = 1; i < (joints.Count - 1); i++)
            {
                var current = joints[i];
                var next = joints[i + 1];
                var distance = Vector3.Distance(current.Position, next.Position);
                current.StartOffset = new Vector3(0f, distance, 0f);
            }

            console.WriteLine($"Origin: {Console.RoundVector(origin,2)}");
            console.WriteLine($"Header: {Console.RoundVector(headerPosition, 2)}");
            console.WriteLine($"Target: {Target}");
            var kinematic = new Kinematic(console);
            kinematic.InverseKinematics(Target, joints);
            foreach (var joint in joints)
            {
                console.WriteLine($"Item: {joint.Item.CustomName} Position: {Console.RoundVector(joint.Position, 2)} TargetAngle: {joint.TargetAngleDeg}");
                joint.Apply();
            }

            
        }
        
        private void DiplayGetType(string name)
        {
            IMyTerminalBlock block = (IMyTerminalBlock)GridTerminalSystem.GetBlockWithName(name);
            IMyTextPanel lcdResult2 = GridTerminalSystem.GetBlockWithName("Result Type") as IMyTextPanel;
            if (lcdResult2 != null)
            {
                lcdResult2.ContentType = ContentType.TEXT_AND_IMAGE;
                lcdResult2.WriteText($"Block {name}\n", false);
                lcdResult2.WriteText($"Type Name={block.GetType().Name}\n", true);
                lcdResult2.WriteText($"SubtypeName={block.BlockDefinition.SubtypeName}\n", true);
                lcdResult2.WriteText($"SubtypeId={block.BlockDefinition.SubtypeId}\n", true);
            }
            else
            {
                Echo($"Block {name}");
                Echo($"Type Name={block.GetType().Name}");
                Echo($"SubtypeName={block.BlockDefinition.SubtypeName}");
                Echo($"SubtypeId={block.BlockDefinition.SubtypeId}");
            }
        }
        public enum StateMachine
        {
            Stopped,
            Stopping,
            Initializing,
            Running,
            RotorZero,
            Waitting,
            Quaternion
        }
    }
}
