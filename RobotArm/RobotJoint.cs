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
        public class RobotJoint
        {
            public RobotJoint(IMyMotorStator pivot, RobotJoint parent = null)
            {
                this.pivot = pivot;
                if (pivot is IMyMotorAdvancedStator)
                {
                    this.MaxAngle = MathHelper.ToRadians(85);
                    this.MinAngle = MathHelper.ToRadians(0);
                    this.Axis = new Vector3(1f, 0f, 0f);
                }
                else
                {
                    this.MaxAngle = 2 * PI;
                    this.MinAngle = - 2 * PI;
                    this.Axis = new Vector3(0f, -1f, 0f);
                    this.deltaAngle = 0;
                }
                if (parent != null)
                {
                    this.parent = parent;
                    this.parent.child = this;
                }
            }

            #region ====== Properties ======
            private float deltaAngle = 0f;
            private RobotJoint parent;
            public RobotJoint Parent
            {
                get { return this.parent; }
            }

            private RobotJoint child;
            public RobotJoint Child
            {
                get { return this.child; }
            }

            private IMyMotorStator pivot;
            public IMyMotorStator Pivot
            {
                get { return this.pivot; }
            }

            public float Angle
            {
                get
                {
                    if (this.pivot == null) return float.NaN;
                    return this.pivot.Angle + deltaAngle;
                }
            }
            public float AngleDeg
            {
                get
                {
                    if (this.pivot == null) return float.NaN;
                    return MathHelper.ToDegrees(this.Angle);
                }
            }

            private float targetAngle;
            public float TargetAngle
            {
                get { return this.targetAngle + deltaAngle; }
                set { this.targetAngle = value - deltaAngle; }
            }
            public float TargetAngleDeg
            {
                get { return MathHelper.ToDegrees(this.TargetAngle); }
                set { this.TargetAngle = MathHelper.ToRadians(value); }
            }

            public Vector3 Position
            {
                get
                {
                    return Vector3D.Transform(pivot.GetPosition(), matrixInverse);
                }
            }

            public Vector3 PositionHeader
            {
                get
                {
                    if (this.header == null) return this.Position;
                    return Vector3D.Transform(header.GetPosition(), matrixInverse);
                }
            }

            private MatrixD matrixInverse = MatrixD.Identity;
            public MatrixD MatrixInverse
            {
                get { return this.matrixInverse; }
                set { this.matrixInverse = value; }
            }

            private Vector3 axis;
            public Vector3 Axis
            {
                get { return this.axis; }
                set { this.axis = value; }
            }

            private Vector3 offset = Vector3.Zero;
            public Vector3 Offset
            {
                get { return this.offset; }
                set { this.offset = value; }
            }

            private float minAngle;
            public float MinAngle
            {
                get { return this.minAngle; }
                set { this.minAngle = value; }
            }

            private float maxAngle;
            public float MaxAngle
            {
                get { return this.maxAngle; }
                set { this.maxAngle = value; }
            }

            public bool IsRoot
            {
                get { return this.parent == null; }
            }
            public bool IsHeader
            {
                get { return this.header != null; }
            }
            private IMyTerminalBlock header;
            public IMyTerminalBlock Header
            {
                get { return this.header; }
                set { this.header = value; }
            }
            #endregion

            #region ====== Methodes ======
            public Vector3 ForwardKinematic()
            {
                var quaternion = Quaternion.CreateFromAxisAngle(Axis, TargetAngle);
                Vector3 offsetPoint = Position + quaternion * Offset;
                return offsetPoint;
            }
            // Can only compute parent offset when MatrixInverse setted
            public void ComputeOffset()
            {
                if (this.child == null && this.header == null) return;
                if (this.child != null)
                {
                    var worldMatrix = this.child.pivot.WorldMatrix;
                    var worldInverse = MatrixD.Invert(worldMatrix);
                    var pivotQZ = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -PI / 2);
                    var matrixQZ = MatrixD.Invert(MatrixD.CreateFromQuaternion(pivotQZ));
                    var matrixInverse = worldInverse;// * matrixQZ;
                    var start = Vector3.Transform(this.child.pivot.GetPosition(), matrixInverse);
                    var end = Vector3.Transform(this.pivot.GetPosition(), matrixInverse);
                    var distance = Vector3.Distance(start, end);
                    this.Offset = new Vector3(0, distance,0);
                } else if (this.header != null)
                {
                    var worldMatrix = this.header.WorldMatrix;
                    var worldInverse = MatrixD.Invert(worldMatrix);
                    var start = Vector3.Transform(this.pivot.GetPosition(), worldInverse);
                    var end = Vector3.Transform(this.header.GetPosition(), worldInverse);
                    var distance = Vector3.Distance(start, end);
                    this.Offset = new Vector3(0, distance, 0);
                }
            }
            #endregion

            #region ====== Actions ======
            public void ApplyOrigin()
            {
                Rotate(MathHelper.ToRadians(10));
            }
            public void Apply()
            {
                Rotate(this.TargetAngle);
            }
            public float PreviousError;
            private void Rotate(float targetAngle)
            {
                var kp = 5f;
                var maxRpm = 1f;
                var error = targetAngle - Angle;
                PreviousError = error;
                var cmd = error * kp;
                var velocity = MathHelper.Clamp(cmd, -maxRpm, maxRpm);
                Pivot.TargetVelocityRPM = velocity;
                Console.WriteLine($"Rotate {pivot.CustomName} error={targetAngle}-{Angle}");
            }
            #endregion
        }
    }
}
