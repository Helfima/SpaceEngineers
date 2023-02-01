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
        public class Kinematic
        {

            private Console console;
            public Kinematic(Console console)
            {
                this.console = console;
            }

            float DistanceThreshold = 0.05f;
            float SamplingDistance = 0.15f;
            float LearningRate = 0.15f;
            
            List<RobotJoint> Joints;
            public Vector3 ForwardKinematics(float[] angles)
            {
                Vector3 prevPoint = Joints[0].Position;
                Quaternion rotation = Quaternion.Identity;
                for (int i = 0; i < Joints.Count; i++)
                {
                    // Rotates around a new axis
                    var offset = Joints[i].StartOffset;
                    var quaternion = Quaternion.CreateFromAxisAngle(Joints[i].Axis, angles[i]);
                    rotation *= quaternion;
                    Vector3 nextPoint = prevPoint + rotation * offset;

                    //console.WriteLine($"NextPoint: {Console.RoundVector(prevPoint, 2)} offset: {Console.RoundVector(offset, 2)} angle: {angles[i]}");
                    prevPoint = nextPoint;
                }
                //console.WriteLine($"NextPoint: {Console.RoundVector(prevPoint, 2)}");
                return prevPoint;
            }
            public float DistanceFromTarget(Vector3 target, float[] angles)
            {
                Vector3 point = ForwardKinematics(angles);
                var distance = Vector3.Distance(point, target);
                return distance;
            }

            public float PartialGradient(Vector3 target, float[] angles, int i)
            {
                // Saves the angle,
                // it will be restored later
                float angle = angles[i];

                // Gradient : [F(x+SamplingDistance) - F(x)] / h
                float f_x = DistanceFromTarget(target, angles);

                angles[i] += SamplingDistance;
                float f_x_plus_d = DistanceFromTarget(target, angles);

                float gradient = (f_x_plus_d - f_x) / SamplingDistance;

                // Restores
                angles[i] = angle;

                return gradient;
            }

            public float[] InverseKinematics(Vector3 target, float[] angles)
            {
                if (DistanceFromTarget(target, angles) < DistanceThreshold)
                    return angles;

                for (int i = Joints.Count - 1; i >= 0; i--)
                {
                    // Gradient descent
                    // Update : Solution -= LearningRate * Gradient
                    float gradient = PartialGradient(target, angles, i);
                    angles[i] -= LearningRate * gradient;

                    // Clamp
                    angles[i] = MathHelper.Clamp(angles[i], Joints[i].MinAngle, Joints[i].MaxAngle);
                    
                    // Early termination
                    if (DistanceFromTarget(target, angles) < DistanceThreshold)
                        return angles;
                }
                return angles;
            }
            public void InverseKinematics(Vector3 target, List<RobotJoint> joints)
            {
                this.Joints = joints;
                var angles = joints.Select(x => x.Angle).ToArray();
                var targetAngles = InverseKinematics(target, angles);
                for(var i = 0; i < joints.Count; i++)
                {
                    joints[i].TargetAngle = targetAngles[i];
                }
            }
            public class RobotJoint
            {
                public Vector3 Axis;
                public Vector3 StartOffset;
                public Vector3 Position;
                public float Angle;
                public float TargetAngle;
                public float MinAngle;
                public float MaxAngle;
                public IMyMotorStator Item;

                public float AngleDeg
                {
                    get { return (float)(Angle * 180 / Math.PI); }
                }
                public float TargetAngleDeg
                {
                    get { return (float)(TargetAngle * 180 / Math.PI); }
                }

                public void Apply()
                {
                    var kp = 1f;
                    var maxRpm = 1f;
                    var error = TargetAngle - Angle;
                    var velocity = MathHelper.Clamp(error* kp, -maxRpm, maxRpm);
                    Item.TargetVelocityRPM = velocity;
                }
            }
        }
    }
}
