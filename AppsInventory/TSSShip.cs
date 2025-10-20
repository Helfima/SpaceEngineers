using System;
using System.Collections.Generic;
using System.Linq;
using AppsInventory.Common;
using AppsInventory.Extensions;
using AppsInventory.Surfaces;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory
{
    [MyTextSurfaceScript("Helfima_TSSShip", "Inventory Ship")]
    public class TSSShip : TSSBase
    {
        public TSSShip(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            this.PropertiesSection = "Ship";
        }

        private List<IMyThrust> thrusts = null;
        private List<IMyCockpit> cockpit = null;
        protected override void OnSearch()
        {
            if (TerminalBlock != null)
            {
                this.cockpit = TerminalBlock.SearchBlocks<IMyCockpit>();
                this.thrusts = TerminalBlock.SearchBlocks<IMyThrust>();
            }
        }
        protected override void OnDraw(FrameDrawing frame)
        {
            float mass = 0f;
            if (this.cockpit.Count > 0)
            {
                Sandbox.ModAPI.Ingame.MyShipMass shipMass = cockpit.First().CalculateShipMass();
                mass = shipMass.TotalMass;
            }
            Dictionary<string, List<IMyThrust>> forces = new Dictionary<string, List<IMyThrust>>();

            var valueUp = thrusts.Where(x => x.GridThrustDirection == Vector3I.Down).ToList();
            forces.Add("Up", valueUp);

            var valueDown = thrusts.Where(x => x.GridThrustDirection == Vector3I.Up).ToList();
            forces.Add("Down", valueDown);

            var valueLeft = thrusts.Where(x => x.GridThrustDirection == Vector3I.Right).ToList();
            forces.Add("Left", valueLeft);

            var valueRight = thrusts.Where(x => x.GridThrustDirection == Vector3I.Left).ToList();
            forces.Add("Right", valueRight);

            var valueForward = thrusts.Where(x => x.GridThrustDirection == Vector3I.Backward).ToList();
            forces.Add("Forward", valueForward);

            var valueBackward = thrusts.Where(x => x.GridThrustDirection == Vector3I.Forward).ToList();
            forces.Add("Backward", valueBackward);

            MySprite text = new MySprite()
            {
                Type = SpriteType.TEXT,
                Position = frame.Position + new Vector2(0, 0),
                RotationOrScale = (float)scale,
                FontId = EnumFont.Monospace,
                Alignment = TextAlignment.LEFT,
            };
            float offset_y = 35f * (float)scale;

            if (oneLine == true)
            {
                text.Data = "Thrusts:";
                text.Color = Color.LightGreen;
                text.Position = frame.Position;
                frame.AddSprite(text);
                frame.Position += new Vector2(0, offset_y);
            }
            foreach (var item in forces)
            {
                if (oneLine)
                {
                    Draw1Line(frame, item, text, mass, offset_y);
                }
                else
                {
                    Draw2Line(frame, item, text, mass, offset_y);
                }
            }
        }
        private void Draw2Line(FrameDrawing frame, KeyValuePair<string, List<IMyThrust>> item, MySprite text, float mass, float offset_y)
        {
            var force = item.Value.Select(x => x.MaxThrust).Sum();
            var speed = Math.Round(force / mass, 1);
            var count = item.Value.Count();
            text.Data = $"Thrusts {item.Key}: {count}";
            text.Color = Color.DimGray;
            text.Position = frame.Position;
            frame.AddSprite(text);
            frame.Position += new Vector2(0, offset_y);

            text.Data = $"{force / 1000,8}kN {speed,8}m/s²";
            text.Color = Color.LightGreen;
            text.Position = frame.Position;
            frame.AddSprite(text);
            frame.Position += new Vector2(0, offset_y);
        }
        private void Draw1Line(FrameDrawing surface, KeyValuePair<string, List<IMyThrust>> item, MySprite text, float mass, float offset_y)
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
        private float scale = 1f;
        private bool oneLine = false;
        protected override void DataLoad()
        {
            this.scale = Properties.GetSingle("Ship", "scale", 1f);
            this.oneLine = Properties.GetBoolean("Ship", "one_line", true);
        }
        protected override void DataSave()
        {
            Properties.Set("Ship", "scale", this.scale);
            Properties.Set("Ship", "one_line", this.oneLine);
            Properties.Save();
        }
    }
}
