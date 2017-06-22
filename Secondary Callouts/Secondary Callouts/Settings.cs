using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rage;

namespace Secondary_Callouts
{
    class Settings
    {
        private static string _location = @"Plugins\LSPDFR\Secondary Callouts\Settings.ini";

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

        public static string UnitName() => InitializeIni().ReadString("Personal", "OfficerName", "Pete Malloy");

        public static string UnitCallsign => InitializeIni().ReadString("Personal", "Callsign", "DIV_01 ADAM BEAT_12");

        public static WeaponAsset[] SuspectWeaponAssets()
        {
            var weapons = InitializeIni().ReadString("Weapons", "SuspectWeaponArray",
                "WEAPON_PISTOL,WEAPON_BAT,WEAPON_PUMPSHOTGUN,WEAPON_ASSAULTRIFLE,WEAPON_KNIFE,WEAPON_STUNGUN,WEAPON_MICROSMG,WEAPON_BULLPUPSHOTGUN,WEAPON_SAWNOFFSHOTGUN");
            return weapons.Split(',').Select(weaponName => new WeaponAsset(weaponName)).ToArray();
        }
        
        public static WeaponAsset[] CopWeaponAssets()
        {
            var weapons = InitializeIni().ReadString("Weapons", "CopWeaponArray", "WEAPON_COMBATPISTOL,WEAPON_PUMPSHOTGUN,WEAPON_CARBINERIFLE");
            return weapons.Split(',').Select(weaponName => new WeaponAsset(weaponName)).ToArray();
        }
    }
}
