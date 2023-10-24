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
        public class RobotPaths
        {
            private float DistanceThreshold = 0.5f;
            private Vector3 topMax = new Vector3(0, 15, 0);
            private List<RobotPath> paths = new List<RobotPath>();
            private RobotJoint header;

            public int Count
            {
                get { return this.paths.Count; }
            }
            public void SetTarget(Vector3 target, List<RobotJoint> joints)
            {
                this.paths.Clear();
                this.header = joints.FirstOrDefault(x => x.IsHeader);
                if(this.header != null)
                {
                    var origin1 = this.header.PositionHeader;
                    var target1 = Vector3.ProjectOnPlane(ref origin1, ref this.topMax) + this.topMax;
                    var path1 = new RobotPath(origin1, target1, 0.5f);
                    this.paths.Add(path1);

                    var target2 = Vector3.ProjectOnPlane(ref target, ref this.topMax) + this.topMax;
                    var path2 = new RobotPath(target1, target2, 0.5f);
                    this.paths.Add(path2);

                    var path3 = new RobotPath(target2, target, 0.5f);
                    this.paths.Add(path3);

                }
            }
            public float Distance()
            {
                if (paths.Count == 0) return 0;
                var currentPath = paths[0];
                var distance = Vector3.Distance(currentPath.Target, header.PositionHeader);
                return distance;
            }
            public RobotPath Next()
            {
                if (paths.Count == 0) return null;
                var currentPath = paths[0];
                var distance = Vector3.Distance(currentPath.Target, header.PositionHeader);
                if(distance < this.DistanceThreshold)
                {
                    paths.RemoveAt(0);
                }
                return paths[0];
            }
        }
        public class RobotPath
        {
            public RobotPath(Vector3 origin, Vector3 target, float speed)
            {
                this.origin = origin;
                this.target = target;
                this.speed = speed;
            }
            private Vector3 origin;
            public Vector3 Origin
            {
                get { return this.origin; }
                set { this.origin = value; }
            }
            private Vector3 target;
            public Vector3 Target
            {
                get { return this.target; }
                set { this.target = value; }
            }
            private float speed;
            public float Speed
            {
                get { return this.speed; }
                set { this.speed = value; }
            }
            private Vector3 orientation = Vector3.Down;
            public Vector3 Orientation
            {
                get { return this.orientation; }
                set { this.orientation = value; }
            }
        }
    }
}
