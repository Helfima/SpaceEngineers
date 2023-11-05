using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using static System.Net.Mime.MediaTypeNames;
using static VRage.Render11.Shader.CacheGenerator;

namespace IngameScript
{
    partial class Program
    {
        public class DisplayShip
        {
            protected DisplayLcd DisplayLcd;
            private int panel = 0;

            private bool enable = false;
            private double scale = 1d;
            private bool oneLine = false;

            public bool search = true;

            private BlockSystem<IMyThrust> thrusts = null;
            private BlockSystem<IMyCockpit> cockpit = null;

            public DisplayShip(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }

            public void Load(MyIni MyIni)
            {
                panel = MyIni.Get("Ship", "panel").ToInt32(0);
                enable = MyIni.Get("Ship", "on").ToBoolean(false);
                scale = MyIni.Get("Ship", "scale").ToDouble(1d);
                oneLine = MyIni.Get("Ship", "one_line").ToBoolean(false);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Ship", "panel", panel);
                MyIni.Set("Ship", "on", enable);
                MyIni.Set("Ship", "scale", scale);
                MyIni.Set("Ship", "one_line", oneLine);
            }
            private void Search()
            {
                cockpit = BlockSystem<IMyCockpit>.SearchBlocks(DisplayLcd.program);
                thrusts = BlockSystem<IMyThrust>.SearchBlocks(DisplayLcd.program);
                search = false;
            }
            public void Draw(Drawing drawing)
            {
                if (!enable) return;
                var surface = drawing.GetSurfaceDrawing(panel);
                surface.Initialize();
                Draw(surface);
            }
            public void Draw(SurfaceDrawing surface)
            {
                if (!enable) return;
                if (search) Search();

                float mass = 0f;
                if (!cockpit.IsEmpty)
                {
                    MyShipMass shipMass = cockpit.First.CalculateShipMass();
                    mass = shipMass.TotalMass;
                }
                Dictionary<string, List<IMyThrust>> forces = new Dictionary<string, List<IMyThrust>>();

                var valueUp = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Down).ToList();
                forces.Add("Up", valueUp);

                var valueDown = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Up).ToList();
                forces.Add("Down", valueDown);

                var valueLeft = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Right).ToList();
                forces.Add("Left", valueLeft);

                var valueRight = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Left).ToList();
                forces.Add("Right", valueRight);

                var valueForward = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Backward).ToList();
                forces.Add("Forward", valueForward);

                var valueBackward = thrusts.List.Where(x => x.GridThrustDirection == Vector3I.Forward).ToList();
                forces.Add("Backward", valueBackward);

                MySprite text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Position = surface.Position + new Vector2(0, 0),
                    RotationOrScale = (float)scale,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT,
                };
                float offset_y = 40f * (float)scale;

                if (oneLine == true)
                {
                    text.Data = "Thrusts:";
                    text.Color = Color.LightGreen;
                    text.Position = surface.Position;
                    surface.AddSprite(text);
                    surface.Position += new Vector2(0, offset_y);
                }
                foreach ( var item in forces)
                {
                    if (oneLine)
                    {
                        Draw1Line(surface, item, text, mass, offset_y);
                    }
                    else
                    {
                        Draw2Line(surface, item, text, mass, offset_y);
                    }
                }
            }
            private void Draw2Line(SurfaceDrawing surface, KeyValuePair<string, List<IMyThrust>> item, MySprite text, float mass, float offset_y)
            {
                var force = item.Value.Select(x => x.MaxThrust).Sum();
                var speed = Math.Round(force / mass, 1);
                var count = item.Value.Count();
                text.Data = $"Thrusts {item.Key}: {count}";
                text.Color = Color.DimGray;
                text.Position = surface.Position;
                surface.AddSprite(text);
                surface.Position += new Vector2(0, offset_y);

                text.Data = $"{force / 1000,8}kN {speed,8}m/s²";
                text.Color = Color.LightGreen;
                text.Position = surface.Position;
                surface.AddSprite(text);
                surface.Position += new Vector2(0, offset_y);
            }
            private void Draw1Line(SurfaceDrawing surface, KeyValuePair<string, List<IMyThrust>> item, MySprite text, float mass, float offset_y)
            {
                var force = item.Value.Select(x => x.MaxThrust).Sum();
                var speed = Math.Round(force / mass, 1);
                var count = item.Value.Count();
                text.Data = $"{force / 1000,6}kN {speed,6}m/s² {item.Key}";
                text.Color = Color.LightGreen;
                text.Position = surface.Position;
                surface.AddSprite(text);
                surface.Position += new Vector2(0, offset_y);
            }
        }
    }
}
