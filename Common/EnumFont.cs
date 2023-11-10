using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    internal struct EnumFont
    {
        public const string Debug = "Debug";

        public const string Red = "Red";

        public const string Green = "Green";

        public const string Blue = "Blue";

        public const string White = "White";

        public const string DarkBlue = "DarkBlue";

        public const string UrlNormal = "UrlNormal";

        public const string UrlHighlight = "UrlHighlight";

        public const string ErrorMessageBoxCaption = "ErrorMessageBoxCaption";

        public const string ErrorMessageBoxText = "ErrorMessageBoxText";

        public const string InfoMessageBoxCaption = "InfoMessageBoxCaption";

        public const string InfoMessageBoxText = "InfoMessageBoxText";

        public const string ScreenCaption = "ScreenCaption";

        public const string GameCredits = "GameCredits";

        public const string LoadingScreen = "LoadingScreen";

        public const string BuildInfo = "BuildInfo";

        public const string BuildInfoHighlight = "BuildInfoHighlight";

        public const string Monospace = "Monospace";
    }
}
