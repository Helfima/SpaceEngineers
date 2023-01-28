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
            public void ParserTitle(IMyTerminalBlock block)
            {
                WriteText($"{block.BlockDefinition.SubtypeId}:{block.CustomName}", false);
            }

            public void ParserCockpit(IMyCockpit block)
            {
                WriteText($"=== Cockpit Info ===", true);
                WriteText($"OxygenCapacity={block.OxygenCapacity}", true);
                WriteText($"OxygenFilledRatio={block.OxygenFilledRatio}", true);
            }
            public void ParserMotorSuspension(IMyMotorSuspension block)
            {
                WriteText($"=== Motor Suspension Info ===", true);
                WriteText($"Steering={block.Steering}", true);
                WriteText($"Propulsion={block.Propulsion}", true);
                WriteText($"InvertSteer={block.InvertSteer}", true);
                WriteText($"InvertPropulsion={block.InvertPropulsion}", true);
                WriteText($"Strength={block.Strength}", true);
                WriteText($"Friction={block.Friction}", true);
                WriteText($"Power={block.Power}", true);
                WriteText($"Height={block.Height}", true);
                WriteText($"SteerAngle={block.SteerAngle}", true);
                WriteText($"MaxSteerAngle={block.MaxSteerAngle}", true);
                WriteText($"Brake={block.Brake}", true);
                WriteText($"AirShockEnabled={block.AirShockEnabled}", true);
            }
            public void ParserShipController(IMyShipController block)
            {
                WriteText($"HasWheels={block.HasWheels}", true);
                WriteText($"RollIndicator={block.RollIndicator}", true);
                WriteText($"RotationIndicator={block.RotationIndicator}", true);
                WriteText($"MoveIndicator={block.MoveIndicator}", true);
                WriteText($"ShowHorizonIndicator={block.ShowHorizonIndicator}", true);
                WriteText($"DampenersOverride={block.DampenersOverride}", true);
                WriteText($"HandBrake={block.HandBrake}", true);
                WriteText($"ControlThrusters={block.ControlThrusters}", true);
                WriteText($"ControlWheels={block.ControlWheels}", true);
                WriteText($"CenterOfMass={block.CenterOfMass}", true);
                WriteText($"IsMainCockpit={block.IsMainCockpit}", true);
                WriteText($"CanControlShip={block.CanControlShip}", true);
                WriteText($"IsUnderControl={block.IsUnderControl}", true);
                
                WriteText($"CalculateShipMass={block.CalculateShipMass()}", true);
                WriteText($"GetArtificialGravity={block.GetArtificialGravity()}", true);
                WriteText($"GetNaturalGravity={block.GetNaturalGravity()}", true);
                WriteText($"GetShipSpeed={block.GetShipSpeed()}", true);
                WriteText($"GetShipVelocities={block.GetShipVelocities()}", true);
                WriteText($"GetTotalGravity={block.GetTotalGravity()}", true);
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
                WriteText($"BlockDefinition={block.BlockDefinition}", true);
                WriteText($"DisassembleRatio={block.DisassembleRatio}", true);
            }
        }
    }
}
