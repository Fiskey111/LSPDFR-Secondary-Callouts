using System.Collections.Generic;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.BaseClass
{
    internal class SpawnMethods
    {
        internal static List<Ped> SpawnPeds(Vector3 spawnPoint, string model, int number, float around1, float around2, float heading)
        {
            var list = new List<Ped>();
            var ped = new Ped();
            var position = World.GetNextPositionOnStreet(spawnPoint.Around2D(around1, around2));

            for (var l = 0; l < number; l++)
            {
                ped = model == null ? new Ped(position) : new Ped(new Model(model), position, heading);

                EntityMethods.CheckIfOnVehicle(ped);
                ped.MakeMissionPed();
                list.Add(ped);
            }
            return list;
        }

        internal static List<Ped> SpawnCops(Vector3 spawnPoint, int number, bool isBusy, bool kill, out List<PedType> pedTypeList)
        {
            var array = GetPeds(spawnPoint);
            var typeList = new List<PedType>();
            var list = new List<Ped>();

            for (var i = 0; i < number; i++)
            {
                var cop = new Ped(new Model(array[Fiskey111Common.Rand.RandomNumber(array.Length)]), spawnPoint.Around2D(2f), 0f);
                Functions.SetPedAsCop(cop);
                Functions.SetCopAsBusy(cop, isBusy);
                switch (i)
                {
                    case 0:
                        cop.Inventory.GiveNewWeapon(WeaponHash.Pistol, 100, true);
                        break;
                    case 1:
                        cop.Inventory.GiveNewWeapon(WeaponHash.PumpShotgun, 100, true);
                        break;
                    default:
                        cop.Inventory.GiveNewWeapon(WeaponHash.CarbineRifle, 400, true);
                        break;
                }
                cop.MakeMissionPed();
                EntityMethods.CheckIfOnVehicle(cop);

                typeList.Add(new PedType(cop, PedType.Type.Cop, cop.Inventory.EquippedWeaponObject));
                list.Add(cop);

                if (kill) cop.Kill();
            }

            pedTypeList = typeList;
            return list;
        }

        internal static string[] GetPeds(Vector3 spawnpoint)
        {
            var zone = Functions.GetZoneAtPosition(spawnpoint);
            $"Getting peds at zone {zone.GameName}".AddLog();
            switch (zone.County)
            {
                case EWorldZoneCounty.LosSantos:
                    var lossantos = new[]
                    {
                        "s_m_y_cop_01",
                        "s_f_y_cop_01"
                    };
                    return lossantos;
                default:
                    var county = new[]
                    {
                        "s_m_y_sheriff_01",
                        "s_f_y_sheriff_01"
                    };
                    return county;
            }
        }
    }
}
