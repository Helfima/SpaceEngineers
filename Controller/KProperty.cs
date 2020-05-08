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

            public string Search_Rotor;
            public string Search_Piston_Up;
            public string Search_Piston_Down;
            public string Search_Piston_Rayon;

            public KProperty(Program program)
            {
                this.program = program;
            }

            public void Load()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                Search_Rotor = MyIni.Get("Search", "Rotor").ToString("Rotor Z1");
                Search_Piston_Up = MyIni.Get("Search", "PistonUp").ToString("Up1");
                Search_Piston_Down = MyIni.Get("Search", "PistonDown").ToString("Down1");
                Search_Piston_Rayon = MyIni.Get("Search", "PistonRayon").ToString("Rayon1");
                if (program.Me.CustomData.Equals(""))
                {
                    Save();
                }
            }

            public void Save()
            {
                MyIniParseResult result;
                if (!MyIni.TryParse(program.Me.CustomData, out result))
                    throw new Exception(result.ToString());
                MyIni.Set("Search", "Rotor", Search_Rotor);
                MyIni.Set("Search", "PistonUp", Search_Piston_Up);
                MyIni.Set("Search", "PistonDown", Search_Piston_Down);
                MyIni.Set("Search", "PistonRayon", Search_Piston_Rayon);
                program.Me.CustomData = MyIni.ToString();
            }
        }
    }
}
