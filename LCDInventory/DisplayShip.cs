﻿using Sandbox.Game.EntityComponents;
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

            public bool search = true;

            private BlockSystem<IMyThrust> thrusts_up = null;
            private BlockSystem<IMyThrust> thrusts_down = null;
            private BlockSystem<IMyThrust> thrusts_left = null;
            private BlockSystem<IMyThrust> thrusts_right = null;
            private BlockSystem<IMyThrust> thrusts_forward = null;
            private BlockSystem<IMyThrust> thrusts_backward = null;
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
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Ship", "panel", panel);
                MyIni.Set("Ship", "on", enable);
                MyIni.Set("Ship", "scale", scale);
            }
            private void Search()
            {
                cockpit = BlockSystem<IMyCockpit>.SearchBlocks(DisplayLcd.program);
                thrusts_up = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Up");
                thrusts_down = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Down");
                thrusts_left = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Left");
                thrusts_right = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Right");
                thrusts_forward = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Forward");
                thrusts_backward = BlockSystem<IMyThrust>.SearchByGroup(DisplayLcd.program, "Thrusters Backward");

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

                float force = 0f;
                float mass = 0f;
                if (!cockpit.IsEmpty)
                {
                    MyShipMass shipMass = cockpit.First.CalculateShipMass();
                    mass = shipMass.TotalMass;
                }
                string direction = "none";

                Dictionary<string, float> forces = new Dictionary<string, float>();
                thrusts_up.ForEach(delegate (IMyThrust block)
                {
                    direction = "Up";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                thrusts_down.ForEach(delegate (IMyThrust block)
                {
                    direction = "Down";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                thrusts_left.ForEach(delegate (IMyThrust block)
                {
                    direction = "Left";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                thrusts_right.ForEach(delegate (IMyThrust block)
                {
                    direction = "Right";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                thrusts_forward.ForEach(delegate (IMyThrust block)
                {
                    direction = "Forward";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                thrusts_backward.ForEach(delegate (IMyThrust block)
                {
                    direction = "Backward";
                    if (forces.ContainsKey(direction)) forces[direction] += block.MaxThrust;
                    else forces.Add(direction, block.MaxThrust);
                });
                MySprite text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Color = Color.DimGray,
                    Position = surface.Position + new Vector2(0, 0),
                    RotationOrScale = (float)scale,
                    FontId = surface.Font,
                    Alignment = TextAlignment.LEFT,
                };
                float offset_y = 40f * (float)scale;
                // Up
                force = 0f;
                forces.TryGetValue("Up", out force);
                text.Data = $"Up: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                surface.AddSprite(text);
                // Down
                surface.Position += new Vector2(0, offset_y);
                force = 0f;
                forces.TryGetValue("Down", out force);
                text.Data = $"Down: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                text.Position = surface.Position;
                surface.AddSprite(text);
                // Forward
                surface.Position += new Vector2(0, offset_y);
                force = 0f;
                forces.TryGetValue("Forward", out force);
                text.Data = $"Forward: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                text.Position = surface.Position;
                surface.AddSprite(text);
                // Backward
                surface.Position += new Vector2(0, offset_y);
                force = 0f;
                forces.TryGetValue("Backward", out force);
                text.Data = $"Backward: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                text.Position = surface.Position;
                surface.AddSprite(text);
                // Right
                surface.Position += new Vector2(0, offset_y);
                force = 0f;
                forces.TryGetValue("Right", out force);
                text.Data = $"Right: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                text.Position = surface.Position;
                surface.AddSprite(text);
                // Left
                surface.Position += new Vector2(0, offset_y);
                force = 0f;
                forces.TryGetValue("Left", out force);
                text.Data = $"Left: {force / 1000}kN / {Math.Round(force / mass, 1)}m/s²";
                text.Position = surface.Position;
                surface.AddSprite(text);

                surface.Position += new Vector2(0, offset_y);
            }
        }
    }
}
