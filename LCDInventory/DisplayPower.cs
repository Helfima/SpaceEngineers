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
        public class DisplayPower
        {
            protected DisplayLcd DisplayLcd;

            private MyDefinitionId PowerDefinitionId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

            private bool enable = false;
            public DisplayPower(DisplayLcd DisplayLcd)
            {
                this.DisplayLcd = DisplayLcd;
            }
            public void Load(MyIni MyIni)
            {
                enable = MyIni.Get("Power", "on").ToBoolean(false);
            }

            public void Save(MyIni MyIni)
            {
                MyIni.Set("Power", "on", enable);
            }
            public Vector2 Draw(Drawing drawing, Vector2 position)
            {
                if (!enable) return position;
                BlockSystem<IMyTerminalBlock> producers = BlockSystem<IMyTerminalBlock>.SearchBlocks(DisplayLcd.program, block => block.Components.Has<MyResourceSourceComponent>());
                BlockSystem<IMyTerminalBlock> consummers = BlockSystem<IMyTerminalBlock>.SearchBlocks(DisplayLcd.program, block => block.Components.Has<MyResourceSinkComponent>());
                float current_output = 0f;
                float current_input = 0f;
                float max_output = 0f;
                float max_input = 0f;
                float width = 50f;
                StyleGauge style = new StyleGauge()
                {
                    Orientation = SpriteOrientation.Horizontal,
                    Fullscreen = true,
                    Width = width,
                    Height = width,
                    Padding = new StylePadding(0),
                    Round = false,
                    RotationOrScale = 0.5f
                };

                MySprite text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Color = Color.DimGray,
                    Position = position + new Vector2(0, 0),
                    RotationOrScale = .8f,
                    FontId = drawing.Font,
                    Alignment = TextAlignment.LEFT

                };

                producers.ForEach(delegate (IMyTerminalBlock block)
                {
                    MyResourceSourceComponent resourceSource;
                    block.Components.TryGet<MyResourceSourceComponent>(out resourceSource);
                    if (resourceSource != null)
                    {
                        ListReader<MyDefinitionId> myDefinitionIds = resourceSource.ResourceTypes;
                        if (myDefinitionIds.Contains(PowerDefinitionId))
                        {
                            current_output += resourceSource.CurrentOutputByType(PowerDefinitionId);
                            max_output += resourceSource.MaxOutputByType(PowerDefinitionId);
                        }
                    }
                });

                drawing.DrawGauge(position, current_output, max_output, style);
                position += new Vector2(0, 60);
                text.Data = $"Power Out: {Math.Round(current_output, 2)}MW / {Math.Round(max_output, 2)}MW";
                text.Position = position;
                drawing.AddSprite(text);

                consummers.ForEach(delegate (IMyTerminalBlock block)
                {
                    MyResourceSinkComponent resourceSink;
                    block.Components.TryGet<MyResourceSinkComponent>(out resourceSink);
                    if (resourceSink != null)
                    {
                        ListReader<MyDefinitionId> myDefinitionIds = resourceSink.AcceptedResources;
                        if (myDefinitionIds.Contains(PowerDefinitionId))
                        {
                            max_input += resourceSink.RequiredInputByType(PowerDefinitionId);
                            current_input += resourceSink.CurrentInputByType(PowerDefinitionId);
                        }
                    }
                });
                position += new Vector2(0, 60);
                drawing.DrawGauge(position, current_input, max_input, style);
                position += new Vector2(0, 60);
                text.Data = $"Power In: {Math.Round(current_input, 2)}MW / {Math.Round(max_input, 2)}MW";
                text.Position = position;
                drawing.AddSprite(text);

                return position + new Vector2(0, 60);
            }
        }
    }
}
