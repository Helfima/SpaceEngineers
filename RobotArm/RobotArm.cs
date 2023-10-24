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
    partial class Program
    {
        public class RobotArm
        {
            protected Program program;
            protected RobotKinematic kinematic;
            protected string filterAnchor;
            public RobotArm(Program program, string filterAnchor)
            {
                this.program = program;
                this.filterAnchor = filterAnchor;
                kinematic = new RobotKinematic();
            }
            private List<RobotJoint> joints = new List<RobotJoint>();
            private List<IMyTerminalBlock> reperes = new List<IMyTerminalBlock>();
            public void Initialize()
            {
                this.joints.Clear();
                this.reperes.Clear();
                // recherche la premiere pivot, le point d'ancrage qui est plutot un rotor
                BlockFilter<IMyMotorStator> block_filter = BlockFilter<IMyMotorStator>.Create(program.Me, filterAnchor);
                var baseStators = BlockSystem<IMyMotorStator>.SearchByFilter(program, block_filter);
                // si le stator existe
                if (baseStators.IsEmpty == false)
                {
                    var pivotAnchor = baseStators.List.First();
                    ComputeMatrixInverse(pivotAnchor);
                    // premier pivot
                    var joint = AddPivot(pivotAnchor);
                    // parcours le bras
                    FollowArms(joint);
                    foreach(var item in this.joints)
                    {
                        item.ComputeOffset();
                    }
                }
            }
            private RobotJoint AddPivot(IMyMotorStator stator, RobotJoint parent = null, List<IMyEntity> entities = null)
            {
                var pivot = new RobotJoint(stator, parent);
                pivot.MatrixInverse = this.matrixInverse;
                this.joints.Add(pivot);
                return pivot;
            }
            private void FollowArms(RobotJoint parent, int index = 1)
            {
                // recupere le block mobile du joint
                var attachable = parent.Pivot.Top;
                // cherche la prochaine pivot sur la grid du block mobile
                var blocks = BlockSystem<IMyMotorStator>.SearchByGrid(program, attachable.CubeGrid);
                
                // si la recherche n'est pas vide on continue le parcours
                if (blocks.IsEmpty == false)
                {
                    var nextStator = blocks.List.FirstOrDefault();
                    if (nextStator != null)
                    {
                        // renome l'axe pour s'y retrouver
                        nextStator.CustomName = $"Axe {index}";
                        var joint = AddPivot(nextStator, parent);
                        FollowArms(joint, index + 1);
                    }
                }
                else
                {
                    // on arrive au bout, recherche la tête
                    var headers = BlockSystem<IMyTerminalBlock>.SearchByGrid(program, attachable.CubeGrid);
                    if (headers.IsEmpty == false)
                    {
                        IMyTerminalBlock header = null;
                        if(headers.List.Count == 1)
                        {
                            header = headers.List.FirstOrDefault();
                            // renome la tête pour s'y retrouver
                            header.CustomName = $"Header";
                        }
                        else
                        {
                            header = headers.List.FirstOrDefault(x => x.CustomName.Contains("Header"));
                        }
                        this.program.Echo($"Has Header: {header != null}");
                        parent.Header = header;
                    }
                }
            }
            /// <summary>
            /// Add reperes with a filter ex: "CM:Repere"
            /// </summary>
            /// <param name="filter"></param>
            public void AddReperes(string filter)
            {
                BlockFilter<IMyTerminalBlock> repere_filter = BlockFilter<IMyTerminalBlock>.Create(program.Me, filter);
                var blocks = BlockSystem<IMyTerminalBlock>.SearchByFilter(program, repere_filter);
                this.reperes = blocks.List;
            }
            private MatrixD matrixInverse = MatrixD.Identity;
            private void ComputeMatrixInverse(IMyMotorStator stator)
            {
                matrixInverse = MatrixD.Identity;
                if (stator != null)
                {
                    var worldMatrix = stator.WorldMatrix;
                    var worldInverse = MatrixD.Invert(worldMatrix);
                    // calage du vecteur Z
                    var pivotQZ = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), -PI / 2);
                    // calage du vecteur X
                    var pivotQX = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), -PI / 2);
                    var matrixQZ = MatrixD.Invert(MatrixD.CreateFromQuaternion(pivotQZ * pivotQX));
                    matrixInverse = worldInverse;// * matrixQZ;
                }
            }
            public void Display()
            {
                if (reperes != null && reperes.Count > 0)
                {
                    Console.WriteLine($"Number of Reperes: {reperes.Count}");
                    foreach (var repere in reperes)
                    {
                        var positionRepere = Vector3D.Transform(repere.GetPosition(), matrixInverse);
                        Console.WriteLine($"Repere: {repere.CustomName} Position: {Console.RoundVector(positionRepere, 2)}");
                    }
                }
                //Console.WriteLine($"Up: {Console.RoundVector(Vector3.Up, 2)} Down: {Console.RoundVector(Vector3.Down, 2)}");
                //Console.WriteLine($"Left: {Console.RoundVector(Vector3.Left, 2)} Right: {Console.RoundVector(Vector3.Right, 2)}");
                //Console.WriteLine($"Forward: {Console.RoundVector(Vector3.Forward, 2)} Backward: {Console.RoundVector(Vector3.Backward, 2)}");
                Console.WriteLine($"Target: {Console.RoundVector(target, 2)}");
                if (joints != null && joints.Count > 0)
                {
                    var header = this.joints.FirstOrDefault(x => x.IsHeader);
                    if(header != null)
                    {
                        Console.WriteLine($"Header: {Console.RoundVector(header.PositionHeader, 2)}");
                    }

                    var distance = Vector3.Distance(header.PositionHeader, target);
                    Console.WriteLine($"Distance: {distance}");
                    Console.WriteLine($"Number of Axis: {joints.Count}");

                    var path = this.paths.Next();
                    if (path != null)
                    {
                        Console.WriteLine($"Path Target: {Console.RoundVector(path.Target, 2)} Distance:{this.paths.Distance()} Paths:{this.paths.Count}");
                    }

                    foreach (var joint in joints)
                    {
                        Console.WriteLine($"Joint: {joint.Pivot.CustomName} Angle: {Math.Round(joint.AngleDeg, 2)} TargetAngle: {Math.Round(joint.TargetAngleDeg, 2)} Position: {Console.RoundVector(joint.Position, 2)} Offset: {Console.RoundVector(joint.Offset, 2)}");
                    }
                }
            }

            private RobotState state = RobotState.Idle;
            public RobotState State
            {
                get {  return this.state; }
            }

            private Vector3 target = Vector3.Zero;
            private RobotPaths paths = new RobotPaths();
            public void Start(Vector3 target)
            {
                this.target = target != null ? target : Vector3.Zero;
                this.paths.SetTarget(target, this.joints);
                this.state = RobotState.Moving;
            }
            public void StartZero()
            {
                this.state = RobotState.Origin;
            }
            public void Stop()
            {
                this.state = RobotState.Stopping;
            }
            public void Run()
            {
                RunOrigin();
                RunMoving();
            }
            private void RunOrigin()
            {
                if (state != RobotState.Origin) return;
                foreach (var joint in this.joints)
                {
                    joint.TargetAngle = 0;
                    joint.ApplyOrigin();
                }
            }
            
            private void RunMoving()
            {
                if (state != RobotState.Moving) return;
                var path = this.paths.Next();
                if(path == null)
                {
                    state = RobotState.Idle;
                    return;
                }
                kinematic.Compute(path, this.joints);
                foreach (var joint in this.joints)
                {
                    joint.Apply(path.Speed);
                }
            }
        }

        public enum RobotState
        {
            Idle,
            Moving,
            Stopping,
            Origin
        }
    }

}
