using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppsInventory.Common;
using AppsInventory.Extensions;
using AppsInventory.Surfaces;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.VoiceChat;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.Scripting;
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
                if (this.surfaceDrawing == null)
                {
                    this.surfaceDrawing = new SurfaceDrawing(Surface);
                }
                this.Load();
                this.Draw();
            }
            catch (Exception ex)
            {
                VRage.Utils.MyLog.Default.WriteLine(ex);
                DrawError(ex);
            }
        }
        private void DrawError(Exception error)
        {
            try
            {
                var message = new StringBuilder();
                message.AppendLine("App Inventory Error");
                message.AppendLine(error.Message);
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
            catch (Exception ex)
            {
                VRage.Utils.MyLog.Default.WriteLine(ex);
            }
        }
        protected SurfaceDrawing surfaceDrawing;
        protected string Version = "1.3";
        private void Draw()
        {
            TerminalBlock.ClearDetailedInfo();
            TerminalBlock.GetDetailedInfo().AppendLine($"Version: {Version}");
            OnSearch();
            using (var frame = this.surfaceDrawing.GetFrameDrawing())
            {
                OnDraw(frame);
                DrawVersion(frame);
            }
        }
        private void DrawVersion(FrameDrawing frame)
        {
            var position = new Vector2(frame.Parent.Viewport.Right-5, frame.Parent.Viewport.Bottom-12);
            frame.AddSprite(new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = $"V{Version}",
                Alignment = TextAlignment.RIGHT,
                FontId = MyFontEnum.White,
                Color = Color.Gray,
                Position = position,
                Size = null,
                RotationOrScale = 0.4f,
            });
        }
        protected abstract void OnSearch();
        protected abstract void OnDraw(FrameDrawing frame);
        protected string PropertiesSection = "Common";
        private void Load()
        {
            Properties.Load();
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
