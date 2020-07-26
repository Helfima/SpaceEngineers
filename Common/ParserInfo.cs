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
        public class ParserInfo
        {
            protected Program program;

            private BlockSystem<IMyTextPanel> lcds = null;

            readonly private string lcd_filter = "C:Parser";
            public ParserInfo(Program program)
            {
                this.program = program;
                Search();
            }

            private void Search()
            {
                BlockFilter<IMyTextPanel> block_filter = BlockFilter<IMyTextPanel>.Create(program.Me, lcd_filter);
                lcds = BlockSystem<IMyTextPanel>.SearchByFilter(program, block_filter);
            }

            public void WriteText(string message, bool append)
            {
                message += "\n";
                if (lcds != null) lcds.ForEach(delegate (IMyTextPanel block)
                {
                    block.WriteText(message, append);
                });
            }
            public void ParserTitle(IMyThrust block)
            {
                WriteText($"{block.BlockDefinition.SubtypeId}:{block.CustomName}", false);
            }
            public void ParserThrust(IMyThrust block)
            {
                WriteText($"=== Thrust Info ===", true);
                WriteText($"ThrustOverride={block.ThrustOverride}", true);
                WriteText($"ThrustOverridePercentage={block.ThrustOverridePercentage}", true);
                WriteText($"MaxThrust={block.MaxThrust}", true);
                WriteText($"MaxEffectiveThrust={block.MaxEffectiveThrust}", true);
                WriteText($"CurrentThrust={block.CurrentThrust}", true);
                WriteText($"GridThrustDirection={block.GridThrustDirection}", true);
            }

            public void ParserProperty(IMyTerminalBlock block)
            {
                WriteText($"=== Properties Info ===", true);
                List<ITerminalProperty> propertyList = new List<ITerminalProperty>();
                block.GetProperties(propertyList, null);
                propertyList.ForEach(delegate (ITerminalProperty property) {
                    if (property.Is<float>()) WriteText($"{property.Id}={block.GetValueFloat(property.Id)}", true);
                    if (property.Is<bool>()) WriteText($"{property.Id}={block.GetValueBool(property.Id)}", true);
                });
            }

            public void ParserTerminalBlock(IMyTerminalBlock block)
            {
                WriteText($"=== TerminalBlock Info ===", true);
                WriteText($"ShowInInventory={block.ShowInInventory}", true);
                WriteText($"ShowInTerminal={block.ShowInTerminal}", true);
                WriteText($"ShowOnHUD={block.ShowOnHUD}", true);
                WriteText($"CustomData={block.CustomData}", true);
                WriteText($"CustomInfo={block.CustomInfo}", true);
                WriteText($"ShowInToolbarConfig={block.ShowInToolbarConfig}", true);
                WriteText($"CustomName={block.CustomName}", true);
                WriteText($"CustomNameWithFaction={block.CustomNameWithFaction}", true);
                WriteText($"DetailedInfo={block.DetailedInfo}", true);
            }
            public void ParserEntity(IMyEntity block)
            {
                WriteText($"=== Entity Info ===", true);
                WriteText($"EntityId={block.EntityId}", true);
                WriteText($"Name={block.Name}", true);
                WriteText($"DisplayName={block.DisplayName}", true);
                WriteText($"HasInventory={block.HasInventory}", true);
                WriteText($"InventoryCount={block.InventoryCount}", true);
                WriteText($"WorldAABB={block.WorldAABB}", true);
                WriteText($"WorldAABBHr={block.WorldAABBHr}", true);
                WriteText($"WorldMatrix={block.WorldMatrix}", true);
                WriteText($"WorldVolume={block.WorldVolume}", true);
                WriteText($"WorldVolumeHr={block.WorldVolumeHr}", true);
            }

            public void ParserCubeBlock(IMyCubeBlock block)
            {
                WriteText($"=== CubeBlock Info ===", true);
                WriteText($"DisplayNameText={block.DisplayNameText}", true);
                WriteText($"Orientation={block.Orientation}", true);
                WriteText($"NumberInGrid={block.NumberInGrid}", true);
                WriteText($"Min={block.Min}", true);
                WriteText($"Mass={block.Mass}", true);
                WriteText($"Max={block.Max}", true);
                WriteText($"IsWorking={block.IsWorking}", true);
                WriteText($"IsFunctional={block.IsFunctional}", true);
                WriteText($"IsBeingHacked={block.IsBeingHacked}", true);
                WriteText($"OwnerId={block.OwnerId}", true);
                WriteText($"Position={block.Position}", true);
                WriteText($"DefinitionDisplayNameText={block.DefinitionDisplayNameText}", true);
                WriteText($"CubeGrid={block.CubeGrid}", true);
                WriteText($"CheckConnectionAllowed={block.CheckConnectionAllowed}", true);
                WriteText($"BlockDefinition={block.BlockDefinition}", true);
                WriteText($"DisassembleRatio={block.DisassembleRatio}", true);
            }
        }
    }
}
