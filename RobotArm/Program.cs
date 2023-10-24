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
        const float PI = (float)Math.PI;
        const UpdateType CommandUpdate = UpdateType.Trigger | UpdateType.Terminal;
        private StateMachine stateMachine = StateMachine.Stopped;
        public Program()
        {
            var drawingSurface = Me.GetSurface(0);
            Console.Initialize(drawingSurface);
            
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
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
            if ((updateType & UpdateType.Update10) != 0)
            {
                RunContinuousLogic();
            }
        }

        private BlockSystem<IMyMotorStator> base_stator = null;
        private BlockSystem<IMyMotorStator> axe_stators = null;
        private BlockSystem<IMyTerminalBlock> headers = null;
        private BlockSystem<IMyTerminalBlock> reperes = null;
        private bool search = true;

        private RobotArm robot;
        private void Search()
        {
            robot = new RobotArm(this, "CM:Pivot");
            robot.Initialize();
            robot.AddReperes("CM:Repere");
            //robot.Display(console);
            search = false;
        }
        private void FollowArms(IMyMotorStator pivot, int index = 1)
        {
            Echo($"Pivot: {pivot.CustomName}");
            var attachable = pivot.Top;
            
            var blocks = BlockSystem<IMyMotorStator>.SearchByGrid(this, attachable.CubeGrid);
            //var entities = BlockSystem<IMyCubeBlock>.SearchByGrid(this, attachable.CubeGrid);

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
            Console.Clear();
            Console.WriteLine($"Status: {stateMachine}");
            if(this.robot != null)
            {
                this.robot.Display();
            }
        }
        void RunContinuousLogic()
        {
            if (search) Search();
            //ComputeMatrixInverse();
            Display();
            RunRobot();
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

                            if (this.search) Search();
                            this.robot.Start(target);
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
                            this.robot.StartZero();
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
        private MatrixD matrixInverse = MatrixD.Identity;
        private void ComputeMatrixInverse()
        {
            if (base_stator.List.Count > 0)
            {
                var pivot = base_stator.List[0];
                var worldMatrix = pivot.WorldMatrix;
                var worldInverse = MatrixD.Invert(worldMatrix);
                var pivotQZ = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), -PI / 2);
                var matrixQZ = MatrixD.Invert(MatrixD.CreateFromQuaternion(pivotQZ));
                matrixInverse = worldInverse * matrixQZ;
            }
        }

        private void TestQuaternion()
        {
            if (stateMachine != StateMachine.Quaternion) return;
            var vector = new Vector3(1, 0, 0);
            var quaternion = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), PI / 2);
            var matrix = Matrix.CreateFromQuaternion(quaternion);
            var target = quaternion * vector;
            Console.WriteLine($"Origin: {vector}");
            Console.WriteLine($"Quaternion: {quaternion}");
            Console.WriteLine($"Target: {target}");
        }
        private void RunRobot()
        {
            if (this.robot == null) return;
            this.robot.Run();
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
            Quaternion,
            TestRobotArm
        }
    }
}
