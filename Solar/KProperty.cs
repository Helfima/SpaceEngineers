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
        public class KProperty
        {
            protected MyIni MyIni = new MyIni();
            protected Program program;

            public string Name;
            public float Lat_Speed;
            public float Lat_Delta;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                Name = MyIni.Get("Common", "Name").ToString("Solar Rotor Z");
                Lat_Speed = MyIni.Get("Latitude", "Speed").ToSingle(0.5f);
                Lat_Delta = MyIni.Get("Latitude", "Delta").ToSingle(0.01f);
            }

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Common", "Name", Name);
                MyIni.Set("Latitude", "Speed", Lat_Speed);
                MyIni.Set("Latitude", "Delta", Lat_Delta);
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
