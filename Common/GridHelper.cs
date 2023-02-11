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
        public class GridHelper
        {
            public static List<IMySlimBlock> GetBlocks(IMyCubeGrid cubeGrid)
            {
                var max = cubeGrid.Max;
                var min = cubeGrid.Min;
                var cubes = new List<IMySlimBlock>();
                for (var x = min.X; x <= max.X; x++)
                {
                    for (var y = min.Y; y <= max.Y; y++)
                    {
                        for (var z = min.Z; z <= max.Z; z++)
                        {
                            var location = new Vector3I(x, y, z);
                            var cubeExist = cubeGrid.CubeExists(location);
                            if (cubeExist)
                            {
                                var cube = cubeGrid.GetCubeBlock(location);
                                if (cube != null)
                                {
                                    cubes.Add(cube);
                                }
                            }
                        }
                    }
                }
                return cubes;
            }
        }
    }
}
