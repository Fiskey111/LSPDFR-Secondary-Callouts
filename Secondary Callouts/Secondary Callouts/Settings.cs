using System.Globalization;
using Rage;

namespace Secondary_Callouts
{
    class Settings
    {
        private static string _location = "Plugins/LSPDFR/Secondary Callouts/Settings.ini";

        public static InitializationFile InitializeIni()
        {
            var ini = new InitializationFile(_location, CultureInfo.InvariantCulture, false);
            ini.Create();
            return ini;
        }

        public static bool AiAudio()
        {
            var ini = InitializeIni();
            return ini.ReadBoolean("Options", "AIAudio", true);
        }
        
        public static bool StartingAudio()
        {
            var ini = InitializeIni();
            return ini.ReadBoolean("Options", "StartingAudio", true);
        }
    }
}
