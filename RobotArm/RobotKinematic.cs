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
        public class RobotKinematic
        {
            public RobotKinematic()
            {
            }

            private float DistanceThreshold = 0.1f;
            private float SamplingDistance = 0.025f;
            private float LearningRate = 1f;

            private Vector3 ForwardKinematics(List<RobotJoint> joints)
            {
                Vector3 prevPoint = joints[0].Position;
                Quaternion rotation = Quaternion.Identity;
                for (int i = 0; i < joints.Count; i++)
                {
                    // Rotates around a new axis
                    var joint = joints[i];
                    var offset = joint.Offset;
                    var quaternion = Quaternion.CreateFromAxisAngle(joint.Axis, joint.TargetAngle);
                    rotation *= quaternion;
                    Vector3 nextPoint = prevPoint + rotation * offset;

                    //console.WriteLine($"NextPoint: {Console.RoundVector(prevPoint, 2)} offset: {Console.RoundVector(offset, 2)} angle: {angles[i]}");
                    prevPoint = nextPoint;
                }
                //console.WriteLine($"NextPoint: {Console.RoundVector(prevPoint, 2)}");
                return prevPoint;
            }
            private float DistanceFromTarget(Vector3 target, List<RobotJoint> joints)
            {
                Vector3 point = ForwardKinematics(joints);
                var distance = Vector3.Distance(point, target);
                return distance;
            }

            private float PartialGradient(Vector3 target, List<RobotJoint> joints, int i)
            {
                var joint = joints[i];
                // Saves the angle,
                // it will be restored later
                float angle = joint.TargetAngle;

                // Gradient : [F(x+SamplingDistance) - F(x)] / h
                float f_x = DistanceFromTarget(target, joints);

                joint.TargetAngle += SamplingDistance;
                float f_x_plus_d = DistanceFromTarget(target, joints);

                float gradient = (f_x_plus_d - f_x) / this.SamplingDistance;

                // Restores
                joint.TargetAngle = angle;

                return gradient;
            }

            private void InverseKinematics(Vector3 target, List<RobotJoint> joints)
            {
                if (DistanceFromTarget(target, joints) < this.DistanceThreshold) return;
                for (int i = joints.Count - 1; i >= 0; i--)
                {
                    var joint = joints[i];
                    // Gradient descent
                    // Update : Solution -= LearningRate * Gradient
                    float gradient = PartialGradient(target, joints, i);
                    joint.TargetAngle -= this.LearningRate * gradient;

                    // Clamp
                    joint.TargetAngle = MathHelper.Clamp(joint.TargetAngle, joint.MinAngle, joint.MaxAngle);

                    // Early termination
                    if (DistanceFromTarget(target, joints) < this.DistanceThreshold) return;
                }
            }
            
            public void Compute(Vector3 target, List<RobotJoint> joints)
            {
                joints[0].StatorRotation(target);
                var otherJoints = joints.Skip(1).ToList();
                foreach (var joint in otherJoints)
                {
                    joint.TargetAngle = joint.Angle;
                }
                InverseKinematics(target, otherJoints);
            }
            
            // Vector3.Up = X:0 Y:1 Z:0
            // Vector3.Down = X:0 Y:-1 Z:0
            // Vector3.Right = X:1 Y:0 Z:0
            // Vector3.Left = X:-1 Y:0 Z:0
            // Vector3.Forward = X:0 Y:0 Z:-1
            // Vector3.Backward = X:0 Y:0 Z:1
            public void Compute(RobotPath path, List<RobotJoint> joints)
            {
                joints[0].StatorRotation(path.Target);
                var otherJoints = joints.Skip(1).ToList();
                foreach (var joint in otherJoints)
                {
                    joint.TargetAngle = joint.Angle;
                }
                InverseKinematics(path.Target, otherJoints);
            }
        }
    }
}
