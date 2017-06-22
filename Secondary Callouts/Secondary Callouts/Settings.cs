using System.Globalization;
using Rage;

namespace Secondary_Callouts
{
    class Settings
    {
        private static string _location = @"Plugins\LSPDFR\Secondary Callouts\Settings.ini";

        public static string UnitName = Fiskey111Common.OfficerSettings.UnitName();

        public static InitializationFile InitializeIni()
        {
            var ini = new InitializationFile(_location, CultureInfo.InvariantCulture, false);
            ini.Create();
            return ini;
        }

        public static bool AiAudio => InitializeIni().ReadBoolean("Options", "AIAudio", true);

        public static bool StartingAudio => InitializeIni().ReadBoolean("Options", "StartingAudio", true);

        public static bool ShotsFiredCallAudio => InitializeIni().ReadBoolean("Options", "ShotsFiredAudio", true);

        public static int GunFireChance => InitializeIni().ReadInt32("Options", "ChanceOfFirearms", 13);

        public static bool AllowEscapeSuspect => InitializeIni().ReadBoolean("Options", "AllowSuspectEscape", true);
    }
}
