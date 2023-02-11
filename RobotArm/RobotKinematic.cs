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

            private float DistanceThreshold = 0.05f;
            private float SamplingDistance = 0.05f;
            private float LearningRate = 0.01f;
            private int limitLoop = 50;
            private int loop = 0;
            public int Loop
            {
                get { return this.loop; }
            }

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
                if (this.loop > this.limitLoop) return;
                if (DistanceFromTarget(target, joints) < this.DistanceThreshold) return;
                for (int i = joints.Count - 1; i >= 0; i--)
                {
                    var joint = joints[i];
                    //if (joint.IsRoot)
                    //{
                    //    SimpleRotation(target, joint);
                    //    continue;
                    //}
                    // Gradient descent
                    // Update : Solution -= LearningRate * Gradient
                    float gradient = PartialGradient(target, joints, i);
                    joint.TargetAngle -= this.LearningRate * gradient;

                    // Clamp
                    joint.TargetAngle = MathHelper.Clamp(joint.TargetAngle, joint.MinAngle, joint.MaxAngle);

                    // Early termination
                    if (DistanceFromTarget(target, joints) < this.DistanceThreshold) return;
                }
                //this.loop++;
                //InverseKinematics(target, joints);
            }
            public void SimpleRotation(Vector3 target, RobotJoint joint)
            {
                var PI = (float) Math.PI;
                var origin = joint.Position;
                var vectorX = new Vector3(1, 0, 0);
                var vectorY = new Vector3(0, 1, 0);
                var vectorZ = new Vector3(0, 0, 1);
                var pointOnPlane = Vector3.ProjectOnPlane(ref target, ref vectorY);
                pointOnPlane.Normalize();
                var dot = Vector3.Dot(vectorZ, pointOnPlane);
                var angle = Math.Acos(dot);
                if (dot > 0)
                {
                    angle = 2 * PI - angle;
                }
                Console.WriteLine($"Angle: {MathHelper.ToDegrees(angle)}, dot: {dot}");
                //joint.TargetAngle = (float) angle;
            }
            public void Compute(Vector3 target, List<RobotJoint> joints)
            {
                foreach(var joint in joints)
                {
                    joint.TargetAngle = joint.Angle;
                }
                InverseKinematics(target, joints);
            }
        }
    }
}
