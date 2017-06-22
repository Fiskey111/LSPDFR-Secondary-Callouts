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

        internal static void GiveFirearms(Ped ped, Vector3 spawnPoint, bool forceWeapons)
        {
            var random = forceWeapons
                ? Fiskey111Common.Rand.RandomNumber(1, 6)
                : Fiskey111Common.Rand.RandomNumber(1, Settings.GunFireChance);

            WeaponAsset weapon;
            switch (random)
            {
                case 1:
                    weapon = new WeaponAsset((uint)WeaponHash.Bat);
                    break;
                case 2:
                    weapon = new WeaponAsset((uint)WeaponHash.Pistol);
                    break;
                case 3:
                    weapon = new WeaponAsset((uint)WeaponHash.PumpShotgun);
                    break;
                case 4:
                    weapon = new WeaponAsset((uint)WeaponHash.AssaultRifle);
                    break;
                case 5:
                    ped.Armor = Fiskey111Common.Rand.RandomNumber(25, 101);
                    break;
            }
            if (random > 4) return;
            var gun = new Weapon(weapon, spawnPoint, 400);
            gun.GiveTo(ped);
        }
    }
}
