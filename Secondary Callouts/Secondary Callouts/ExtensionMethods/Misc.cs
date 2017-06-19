using LSPD_First_Response.Mod.API;
using Rage;

namespace Secondary_Callouts.ExtensionMethods
{
    internal static class Misc
    {
        internal static string GetZoneGameName(this Vector3 spawnPoint)
        {
            var zone = Functions.GetZoneAtPosition(spawnPoint);
            return zone.GameName;
        }

        internal static string GetZoneAudioName(this Vector3 spawnPoint)
        {
            var zone = Functions.GetZoneAtPosition(spawnPoint);
            return zone.AudioName;
        }
    }
}
