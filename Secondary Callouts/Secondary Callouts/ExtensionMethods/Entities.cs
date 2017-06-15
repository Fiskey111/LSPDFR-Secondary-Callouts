using System.Collections.Generic;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;

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

        public static void Task_Scenario(this Ped ped, string scenario) => NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(ped, scenario.ToString(), 0, true);

        public static void Task_Scenario(this Ped ped, string scenario, Vector3 position, float heading) => NativeFunction.Natives.TASK_START_SCENARIO_AT_POSITION(ped, scenario, position.X, position.Y, position.Z, heading, 0, 0, 1);

    }
}
