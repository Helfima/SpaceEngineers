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
        public class RobotKinematicJacobian
        {
            // EPS = Epsilon, how far end effector position will be from target position
            public float EPS = 0.5f;
            // Step value = How many angle change for each joint per iteration
            public float step = 0.015f;
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

            private float[] GetDeltaOrientation()
            {
                float[,] Jt = GetJacobianTranspose();

                Vector3 V = (target.transform.position - joints[joints.Length - 1].transform.position);

                //dO = Jt * V;
                float[,] dO = Tools.M_Multiply(Jt, new float[,] { { V.x }, { V.y }, { V.z } });
                return new float[] { dO[0, 0], dO[1, 0], dO[2, 0] };
            }

            private float[,] GetJacobianTranspose()
            {

                Vector3 J_A = Vector3.Cross(joints[0].transform.forward, (joints[joints.Length - 1].transform.position - joints[0].transform.position));
                Vector3 J_B = Vector3.Cross(joints[1].transform.forward, (joints[joints.Length - 1].transform.position - joints[1].transform.position));
                Vector3 J_C = Vector3.Cross(joints[2].transform.forward, (joints[joints.Length - 1].transform.position - joints[2].transform.position));

                float[,] matrix = new float[3, 3];

                matrix = Tools.M_Populate(matrix, new Vector3[] { J_A, J_B, J_C });

                return Tools.M_Transpose(matrix);
            }
        }
    }
}
