using System.Collections.Generic;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using Secondary_Callouts.Objects;

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

            list.Clear();
        }

        public static bool PedCheck(this Ped ped)
        {
            if (!ped.Exists()) return false;
            return Functions.IsPedArrested(ped) || ped.IsDead;
        }

        public static void PlayAnimation(this Ped ped, Animation animation) => ped.Tasks.PlayAnimation(
            animation.Dictionary, animation.AnimationName, animation.BlendInSpeed, animation.Flags);

        public static void Task_Scenario(this Ped ped, string scenario) => NativeFunction.Natives.TASK_START_SCENARIO_IN_PLACE(ped, scenario.ToString(), 0, true);

        public static void Task_Scenario(this Ped ped, string scenario, Vector3 position, float heading) => NativeFunction.Natives.TASK_START_SCENARIO_AT_POSITION(ped, scenario, position.X, position.Y, position.Z, heading, 0, 0, 1);

        /// <summary>
        /// Gives peds specific components
        /// </summary>
        /// <param name="comp">IS ZERO BASED INDEX (SNT is 1 based)</param>
        /// <param name="drawIndex">IS ZERO BASED INDEX (SNT is 1 based)</param>
        /// <param name="variation">IS ZERO BASED INDEX (SNT is 1 based)</param>
        public static void GivePedVariation(this Ped ped, PedComponent comp, int drawIndex, int variation = 0) => ped.SetVariation((int)comp, drawIndex, variation);

        /// <summary>
        /// Gives peds specific props (includes hats)
        /// </summary>
        /// <param name="comp">IS ZERO BASED INDEX (SNT is 1 based)</param>
        /// <param name="drawIndex">IS ZERO BASED INDEX (SNT is 1 based)</param>
        /// <param name="variation">IS ZERO BASED INDEX (SNT is 1 based)</param>
        public static void GivePedProp(this Ped ped, PropID comp, int drawIndex, int variation = 0, bool attach = true)
            => NativeFunction.Natives.SET_PED_PROP_INDEX(ped, (int) comp, drawIndex, variation, attach);

        public enum PedComponent { Head, Beard, Hair, Torso, Legs, Hands, Foot, Acc1 = 8, Acc2 = 9, AuxTorso = 11 }

        public enum PropID { Helmets, Glasses, EarAcc }
    }
}
