using System;
using System.Collections.Generic;
using AppsInventory.Extensions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AppsInventory.Surfaces
{
    public class SurfaceDrawing
    {
        public string Font { get; } = "Monospace";
        public Sandbox.ModAPI.Ingame.IMyTextSurface Surface;
        private FrameDrawing frame;
        public RectangleF Viewport;
        public SurfaceDrawing(Sandbox.ModAPI.Ingame.IMyTextSurface surface)
        {
            this.Surface = surface;
            Initialize();
        }
        public Dictionary<string, string> Symbol = new Dictionary<string, string>();
        public void Initialize()
        {
            // background color
            Surface.ScriptBackgroundColor = Color.Black;
            // Calculate the viewport by centering the surface size onto the texture size
            Viewport = new RectangleF((Surface.TextureSize - Surface.SurfaceSize) / 2f, Surface.SurfaceSize);
            PrepareSprite();

            Symbol.Add("Cobalt", "Co");
            Symbol.Add("Nickel", "Ni");
            Symbol.Add("Magnesium", "Mg");
            Symbol.Add("Platinum", "Pt");
            Symbol.Add("Iron", "Fe");
            Symbol.Add("Gold", "Au");
            Symbol.Add("Silicon", "Si");
            Symbol.Add("Silver", "Ag");
            Symbol.Add("Stone", "Stone");
            Symbol.Add("Uranium", "U");
            Symbol.Add("Ice", "Ice");
        }
        public FrameDrawing GetFrameDrawing()
        {
            this.frame = new FrameDrawing(this);
            return this.frame;
        }
        public Dictionary<string, string> Sprites_component = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ingot = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ore = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_tool = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_seed = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_ammo = new Dictionary<string, string>();
        public Dictionary<string, string> Sprites_other = new Dictionary<string, string>();

        private bool isPrepared = false;
        private void AddSafe(Dictionary<string, string> ditionary, string key, string value)
        {
            if (ditionary.ContainsKey(key)) return;
            ditionary.Add(key, value);
        }
        private void PrepareSprite()
        {
            if (isPrepared) return;
            isPrepared = true;
            var names = new List<string>();
            Surface.GetSprites(names);
            foreach (var name in names)
            {
                if (name.Contains("/"))
                {
                    try
                    {
                        var words = name.Split('/');
                        var type = words[0];
                        var icon = words[1];
                        switch (type)
                        {
                            case "MyObjectBuilder_AmmoMagazine":
                                AddSafe(Sprites_ammo, icon, name);
                                break;
                            case "MyObjectBuilder_Component":
                                AddSafe(Sprites_component, icon, name);
                                break;
                            case "MyObjectBuilder_Ingot":
                                AddSafe(Sprites_ingot, icon, name);
                                break;
                            case "MyObjectBuilder_Ore":
                                AddSafe(Sprites_ore, icon, name);
                                break;
                            case "MyObjectBuilder_PhysicalGunObject":
                                AddSafe(Sprites_tool, icon, name);
                                break;
                            case "MyObjectBuilder_SeedItem":
                                AddSafe(Sprites_seed, icon, name);
                                break;
                            default:
                                AddSafe(Sprites_other, icon, name);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        VRage.Utils.MyLog.Default.WriteLine(ex);
                    }
                }
            }
        }
    }
}
