using System.Linq;
using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.Objects;

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

        internal static Animation GetAnimation(this Animation[] array, Animation.EMSAnimation type) => array.FirstOrDefault(anim => anim.AnimationType == type);
    }
}
