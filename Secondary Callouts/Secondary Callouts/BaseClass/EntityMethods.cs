using System.Collections.Generic;
using System.Linq;
using Fiskey111Common;
using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.BaseClass
{
    internal class EntityMethods
    {
        internal static void CheckIfOnVehicle(Ped ped)
        {
            if (!ped) return;
            var groundPos = World.GetGroundZ(ped.Position, false, true);
            while (!groundPos.HasValue)
                GameFiber.Yield();

            var dist = Vector3.Distance(new Vector3(ped.Position.X, ped.Position.Y, groundPos.Value), ped.Position);
            $"Distance from ground position for ped: {dist}".AddLog();
            if (dist < 0.5f) return;

            ped.Position = ped.RightPosition;
            ped.Position = ped.RightPosition;
            ped.Position = new Vector3(ped.Position.X, ped.Position.Y, groundPos.Value);
        }

        internal static void GiveFirearms(Ped ped, IEnumerable<WeaponAsset> weaponList, Vector3 spawnPoint, bool forceWeapons)
        {
            var random = forceWeapons
                ? Fiskey111Common.Rand.RandomNumber(1, 6)
                : Fiskey111Common.Rand.RandomNumber(1, Settings.GunFireChance);

            if (weaponList == null) weaponList = Settings.SuspectWeaponAssets();
            weaponList.ToArray();

            if (random != 1) return;

            ped.Inventory.GiveNewWeapon(weaponList.ToArray()[Rand.RandomNumber(weaponList.ToArray().Length)], 400,
                true);
        }
    }
}
