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

namespace IngameScript
{
    partial class Program
    {
        public class Airlock
        {
            private MyCommandLine commandLine = new MyCommandLine();
            public Program program;
            public string GroupFilter;

            private StateMachine State = StateMachine.Stop;
            private ModeMachine Mode = ModeMachine.Stop;

            private List<ActionMachine> Sequence;
            private int Stage = 0;
            private int Sleep = 0;

            private float Epsilon = 0.01f;

            private float OxygenRate = 0f;

            private BlockSystem<IMyTextPanel> control_lcds = null;
            private BlockSystem<IMyTextPanel> control_panel = null;
            private BlockSystem<IMyDoor> doors = null;
            private BlockSystem<IMyAirVent> airvent = null;
            private BlockSystem<IMyLightingBlock> light = null;
            public Airlock(Program program, String group_name)
            {
                this.program = program;
                this.GroupFilter = group_name;
                Init();
                Search();
            }

            private void Init()
            {
                Stage = 0;
                Sleep = 0;
            }

            public void Search()
            {
                BlockFilter<IMyDoor> block_filter_doors = BlockFilter<IMyDoor>.Create(program.Me, $"G:{GroupFilter}");
                doors = BlockSystem<IMyDoor>.SearchByFilter(program, block_filter_doors);
                BlockFilter<IMyAirVent> block_filter_airvent = BlockFilter<IMyAirVent>.Create(program.Me, $"G:{GroupFilter}");
                airvent = BlockSystem<IMyAirVent>.SearchByFilter(program, block_filter_airvent);
                BlockFilter<IMyTextPanel> block_filter_control = BlockFilter<IMyTextPanel>.Create(program.Me, $"G:{GroupFilter}");
                control_panel = BlockSystem<IMyTextPanel>.SearchByFilter(program, block_filter_control);
                BlockFilter<IMyLightingBlock> block_filter_light = BlockFilter<IMyLightingBlock>.Create(program.Me, $"G:{GroupFilter}");
                light = BlockSystem<IMyLightingBlock>.SearchByFilter(program, block_filter_light);

                if (control_panel != null)
                {
                    control_panel.ForEach(delegate (IMyTextPanel block)
                    {
                        block.ScriptBackgroundColor = Color.Black;
                    });
                }

                if (light != null)
                {
                    light.ForEach(delegate (IMyLightingBlock block)
                    {
                        block.Color = program.MyProperty.GetColor("Airlock", "running_color");
                    });
                    light.Off();
                }
            }

            public void RunContinuousLogic()
            {
                program.WriteText($"Stage:{Stage}", true);
                program.WriteText($"State:{State}", true);
                DisplayControl();
                LightControl();
                if (Mode != ModeMachine.Stop && Sequence.Count > Stage)
                {
                    Staging(Sequence[Stage]);
                }
            }
            public void RunCommand(string argument)
            {
                if (argument != null)
                {
                    commandLine.TryParse(argument);
                    var command = commandLine.Argument(0);
                    if (command != null)
                    {
                        switch (command.ToLower())
                        {
                            case "open":
                                Stage = 0;
                                Mode = ModeMachine.Open;
                                Sequence = new List<ActionMachine>();
                                Sequence.Add(ActionMachine.Start);

                                Sequence.Add(ActionMachine.Unlock);
                                Sequence.Add(ActionMachine.Close);
                                Sequence.Add(ActionMachine.Lock);
                                Sequence.Add(ActionMachine.Depressure);
                                Sequence.Add(ActionMachine.UnlockExt);
                                Sequence.Add(ActionMachine.OpenExt);
                                Sequence.Add(ActionMachine.SleepInit);
                                Sequence.Add(ActionMachine.Sleep);
                                Sequence.Add(ActionMachine.CloseExt);
                                Sequence.Add(ActionMachine.LockExt);
                                Sequence.Add(ActionMachine.Pressure);
                                Sequence.Add(ActionMachine.UnlockInt);

                                Sequence.Add(ActionMachine.Stop);
                                Sequence.Add(ActionMachine.Terminated);
                                break;
                            case "openint":
                                if (State != StateMachine.OpenInt)
                                {
                                    Stage = 0;
                                    Mode = ModeMachine.Open;
                                    Sequence = new List<ActionMachine>();
                                    Sequence.Add(ActionMachine.Start);

                                    Sequence.Add(ActionMachine.Unlock);
                                    Sequence.Add(ActionMachine.Close);
                                    Sequence.Add(ActionMachine.Lock);
                                    Sequence.Add(ActionMachine.Pressure);
                                    Sequence.Add(ActionMachine.UnlockInt);
                                    Sequence.Add(ActionMachine.OpenInt);

                                    Sequence.Add(ActionMachine.Stop);
                                    Sequence.Add(ActionMachine.TerminatedInt);
                                }
                                break;
                            case "openext":
                                if (State != StateMachine.OpenExt)
                                {
                                    Stage = 0;
                                    Mode = ModeMachine.Open;
                                    Sequence = new List<ActionMachine>();
                                    Sequence.Add(ActionMachine.Start);

                                    Sequence.Add(ActionMachine.Unlock);
                                    Sequence.Add(ActionMachine.Close);
                                    Sequence.Add(ActionMachine.Lock);
                                    Sequence.Add(ActionMachine.Depressure);
                                    Sequence.Add(ActionMachine.UnlockExt);
                                    Sequence.Add(ActionMachine.OpenExt);

                                    Sequence.Add(ActionMachine.Stop);
                                    Sequence.Add(ActionMachine.TerminatedExt);
                                }
                                break;
                            case "close":
                                Stage = 0;
                                Mode = ModeMachine.Open;
                                Sequence = new List<ActionMachine>();
                                Sequence.Add(ActionMachine.Start);

                                Sequence.Add(ActionMachine.Unlock);
                                Sequence.Add(ActionMachine.Close);
                                Sequence.Add(ActionMachine.Lock);
                                Sequence.Add(ActionMachine.Pressure);
                                Sequence.Add(ActionMachine.UnlockInt);

                                Sequence.Add(ActionMachine.Stop);
                                Sequence.Add(ActionMachine.Terminated);
                                break;
                        }
                    }
                    else
                    {
                        Init();
                        Search();
                    }
                }
            }

            private void Staging(ActionMachine action)
            {
                bool state = true;
                switch (action)
                {
                    case ActionMachine.Open:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            block.OpenDoor();
                            if (block.OpenRatio < 1 - Epsilon) state = false;
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.OpenInt:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("int"))
                            {
                                block.OpenDoor();
                                if (block.OpenRatio < 1 - Epsilon) state = false;
                            }
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.OpenExt:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("ext"))
                            {
                                block.OpenDoor();
                                if (block.OpenRatio < 1 - Epsilon) state = false;
                            }
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.Close:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            block.CloseDoor();
                            if (block.OpenRatio > 0 + Epsilon) state = false;
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.CloseInt:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("int"))
                            {
                                block.CloseDoor();
                                if (block.OpenRatio > 0 + Epsilon) state = false;
                            }
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.CloseExt:
                        state = true;
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("ext"))
                            {
                                block.CloseDoor();
                                if (block.OpenRatio > 0 + Epsilon) state = false;
                            }
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.Lock:
                        doors.ForEach(delegate (IMyDoor block) {
                            block.ApplyAction("OnOff_Off");
                        });
                        Stage++;
                        break;
                    case ActionMachine.LockInt:
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("int")) block.ApplyAction("OnOff_Off");
                        });
                        Stage++;
                        break;
                    case ActionMachine.LockExt:
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("ext")) block.ApplyAction("OnOff_Off");
                        });
                        Stage++;
                        break;
                    case ActionMachine.Unlock:
                        doors.ForEach(delegate (IMyDoor block) {
                            block.ApplyAction("OnOff_On");
                        });
                        Stage++;
                        break;
                    case ActionMachine.UnlockInt:
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("int")) block.ApplyAction("OnOff_On");
                        });
                        Stage++;
                        break;
                    case ActionMachine.UnlockExt:
                        doors.ForEach(delegate (IMyDoor block) {
                            if (block.CustomName.ToLower().Contains("ext")) block.ApplyAction("OnOff_On");
                        });
                        Stage++;
                        break;
                    case ActionMachine.Pressure:
                        airvent.ApplyAction("Depressurize_Off");
                        state = true;
                        airvent.ForEach(delegate (IMyAirVent block) {
                            VentStatus status = block.Status;
                            if (block.GetOxygenLevel() < 1 - Epsilon) state = false;
                            WriteText($"Status:{status}", true);
                            WriteText($"Pressure:{block.GetOxygenLevel() * 100}", true);
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.Depressure:
                        airvent.ApplyAction("Depressurize_On");
                        state = true;
                        airvent.ForEach(delegate (IMyAirVent block) {
                            VentStatus status = block.Status;
                            if (block.GetOxygenLevel() > 0 + Epsilon) state = false;
                            WriteText($"Status:{status}", true);
                            WriteText($"Pressure:{block.GetOxygenLevel() * 100}", true);
                        });
                        if (state) Stage++;
                        break;
                    case ActionMachine.Sleep:
                        Sleep--;
                        WriteText($"Sleep:{Sleep}", true);
                        if (Sleep <= 0) Stage++;
                        break;
                    case ActionMachine.SleepInit:
                        Sleep = program.MyProperty.timer;
                        Stage++;
                        break;
                    case ActionMachine.Start:
                        State = StateMachine.Running;
                        Stage++;
                        break;
                    case ActionMachine.Stop:
                        Sleep = 0;
                        Stage++;
                        break;
                    case ActionMachine.Terminated:
                        State = StateMachine.Open;
                        Stage++;
                        break;
                    case ActionMachine.TerminatedInt:
                        State = StateMachine.OpenInt;
                        Stage++;
                        break;
                    case ActionMachine.TerminatedExt:
                        State = StateMachine.OpenExt;
                        Stage++;
                        break;
                }
            }

            public void WriteText(string message, bool append)
            {
                message += "\n";
                if (control_lcds != null) control_lcds.ForEach(delegate (IMyTextPanel block)
                {
                    block.WriteText(message, append);
                });

            }

            private void LightControl()
            {
                if(light != null)
                {
                    light.ForEach(delegate (IMyLightingBlock block) {
                        string type = block.BlockDefinition.SubtypeName;
                        if (type.Contains("Rotating"))
                        {
                            if (State == StateMachine.Running) block.ApplyAction("OnOff_On");
                            else block.ApplyAction("OnOff_Off");
                        }
                        else
                        {
                            block.ApplyAction("OnOff_On");
                            if (State == StateMachine.Running)
                            {
                                block.BlinkLength = 50f;
                                block.BlinkIntervalSeconds = 0.3f;
                                block.Color = program.MyProperty.GetColor("Airlock", "running_color");
                            }
                            else
                            {
                                block.BlinkIntervalSeconds = 0f;
                                if (OxygenRate > 1 - Epsilon)
                                {
                                    block.Color = program.MyProperty.GetColor("Airlock", "pressurised_color");
                                }
                                else if (OxygenRate < 0 + Epsilon)
                                {
                                    block.Color = program.MyProperty.GetColor("Airlock", "depressurised_color");
                                }
                            }
                        }
                    });
                }
            }
            private void DisplayControl()
            {
                if (control_panel != null)
                {
                    airvent.ForEach(delegate (IMyAirVent block) {
                        OxygenRate = block.GetOxygenLevel() * 100;
                    });
                    control_panel.ForEach(delegate (IMyTextPanel block)
                    {
                        string type = block.BlockDefinition.SubtypeName;
                        if(type.Contains("Corner")) DrawSmallControl(block);
                        else DrawNormalControl(block);
                    });
                }
            }

            private void DrawNormalControl(IMyTextPanel block)
            {
                Drawing drawing = new Drawing(block);

                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Vertical,
                    Fullscreen = false,
                    Width = 80f,
                    Height = 200f,
                    Padding = new StylePadding(5),
                    RotationOrScale = 1f,
                    Percent = false
                };

                Vector2 position = new Vector2(drawing.viewport.Width / 2 - 100 + style.Padding.X, drawing.viewport.Height / 2 - 50 + style.Padding.Y);

                drawing.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Airlock",
                    Color = Color.DimGray,
                    Position = position + new Vector2(100f, 0f),
                    RotationOrScale = 2f,
                    Alignment = TextAlignment.CENTER

                });

                position += new Vector2(0, 60);
                drawing.DrawGauge(position, OxygenRate, 100, style, true);

                string icon = "No Entry";
                if (OxygenRate < 1 - Epsilon) icon = "Danger";
                drawing.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = $"{icon}",
                    Color = Color.DimGray,
                    Position = position + new Vector2(100f, 50f),
                    Size = new Vector2(100, 100),
                    Alignment = TextAlignment.LEFT

                });

                if (Sleep > 0)
                {
                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = $"{Sleep}",
                        Color = Color.DimGray,
                        Position = position + new Vector2(150f, 120f),
                        RotationOrScale = 2f,
                        Alignment = TextAlignment.CENTER

                    });
                }
                drawing.Dispose();
            }

            private void DrawSmallControl(IMyTextPanel block)
            {
                Drawing drawing = new Drawing(block);
                bool horizontal = drawing.viewport.Height < drawing.viewport.Width;
                float gauge_width = 60f;
                float gauge_height = 300f;
                float icon_size = 80f;

                StyleGauge style = new StyleGauge()
                {
                    Orientation = horizontal ? SpriteOrientation.Horizontal : SpriteOrientation.Vertical,
                    Fullscreen = false,
                    Width = horizontal ? gauge_height : gauge_width,
                    Height = horizontal ? gauge_width : gauge_height,
                    Padding = new StylePadding(2),
                    RotationOrScale = 1f,
                    Percent = false
                };

                Vector2 position = new Vector2(style.Padding.X, style.Padding.Y);
                Vector2 delta = horizontal ? new Vector2(10, icon_size - 18) : new Vector2(22, icon_size / 2);
                string icon = "No Entry";
                if (OxygenRate < 1 - Epsilon) icon = "Danger";
                drawing.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = $"{icon}",
                    Color = Color.DimGray,
                    Position = position + delta,
                    Size = new Vector2(icon_size, icon_size),
                    Alignment = TextAlignment.LEFT

                });

                delta = horizontal ? new Vector2((drawing.viewport.Width - gauge_height) / 2, 32) : new Vector2(32, (drawing.viewport.Height - gauge_height) / 2);
                drawing.DrawGauge(position + delta, OxygenRate, 100, style, true);


                delta = horizontal ? new Vector2(drawing.viewport.Width - 60, drawing.viewport.Height / 2) : new Vector2(22 + drawing.viewport.Width / 2, drawing.viewport.Height - 80);
                if (Sleep > 0)
                {
                    drawing.AddSprite(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = $"{Sleep}",
                        Color = Color.DimGray,
                        Position = position + delta,
                        RotationOrScale = 2f,
                        Alignment = TextAlignment.CENTER

                    });
                }
                drawing.Dispose();
            }

            public enum StateMachine
            {
                Open,
                OpenInt,
                OpenExt,
                Running,
                Stop
            }
            public enum ModeMachine
            {
                Open,
                Close,
                Stop
            }
            public enum ActionMachine
            {
                Open,
                OpenInt,
                OpenExt,
                Close,
                CloseInt,
                CloseExt,
                Lock,
                LockInt,
                LockExt,
                Unlock,
                UnlockInt,
                UnlockExt,
                Depressure,
                Pressure,
                Start,
                Stop,
                Sleep,
                SleepInit,
                Terminated,
                TerminatedInt,
                TerminatedExt
            }
        }
    }
}
