using System.Collections.Generic;
using LSPD_First_Response.Mod.API;
using Rage;

namespace Secondary_Callouts.ExtensionMethods
{
    public static class Entities
    {
        public static void MakeMissionPed(this Ped ped) => ped.IsPersistent = true;

        public static void Dismiss<T>(this List<T> list) where T : Entity
        {
            foreach (var ent in list)
            {
                if (!ent) continue;
                ent.Dismiss();
            }
        }

        public static bool PedCheck(this Ped ped)
        {
            if (!ped.Exists()) return false;
            return Functions.IsPedArrested(ped) || ped.IsDead;
        }

        public static bool PedCheck(this List<Ped> peds)
        {
            if (peds.Count < 1) return false;

            foreach (var ped in peds)
            {
                if (!ped) return false;
                if (Functions.IsPedArrested(ped) || ped.IsDead) continue;

                return false;
            }
            return true;
        }
    }
}
