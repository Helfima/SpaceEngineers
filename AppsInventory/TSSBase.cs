using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public abstract class TSSBase : MyTSSCommon
    {
        public override ScriptUpdate NeedsUpdate { get; } = ScriptUpdate.Update10;

        protected readonly IMyTerminalBlock TerminalBlock;
        protected DataProperties Properties;

        public TSSBase(Sandbox.ModAPI.Ingame.IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            TerminalBlock = (IMyTerminalBlock)block;
            TerminalBlock.OnMarkForClose += BlockDeleted;
            Properties = new DataProperties(TerminalBlock);
        }

        public override void Dispose()
        {
            base.Dispose();
            TerminalBlock.OnMarkForClose -= BlockDeleted;
        }

        void BlockDeleted(IMyEntity _)
        {
            Dispose();
        }

        public override void Run()
        {
            base.Run();
            try
            {
                this.Load();
                this.Draw();
            }
            catch (Exception ex)
            {
                VRage.Utils.MyLog.Default.WriteLine(ex);
                DrawError(ex);
            }
        }
        private void DrawError(Exception ex)
        {
            var message = new StringBuilder();
            message.AppendLine("App Inventory Error");
            message.AppendLine(ex.Message);
            message.AppendLine("You can contact modder");
            using (var frame = this.surfaceDrawing.GetFrameDrawing())
            {
                frame.AddSprite(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = message.ToString(),
                    Alignment = TextAlignment.CENTER,
                    FontId = MyFontEnum.White,
                    Color = Color.Red,
                    Position = null,
                    Size = null,
                    RotationOrScale = 1,
                });
            }
        }
        protected SurfaceDrawing surfaceDrawing;

        private void Draw()
        {
            if (this.surfaceDrawing == null)
            {
                this.surfaceDrawing = new SurfaceDrawing(Surface);
            }
            TerminalBlock.ClearDetailedInfo();
            OnSearch();
            using (var frame = this.surfaceDrawing.GetFrameDrawing())
            {
                OnDraw(frame);
            }
        }
        protected abstract void OnSearch();
        protected abstract void OnDraw(FrameDrawing frame);
        protected string PropertiesSection = "Common";
        private void Load()
        {
            DataLoad();
            if (Properties.HasSection(this.PropertiesSection) == false)
            {
                DataSave();
            }
        }
        protected abstract void DataLoad();
        protected abstract void DataSave();
    }
}
