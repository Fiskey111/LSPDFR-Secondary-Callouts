using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ComputerPlus;
using ComputerPlus.API;
using Rage;

namespace Secondary_Callouts.API
{
    class BetterEMSAPI
    {
        public static void RequestEms(Vector3 loc)
        {
            if (PluginCheck.IsBetterEMSRunning()) EmsRespond(loc);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void EmsRespond(Vector3 location) => BetterEMS.API.EMSFunctions.RespondToLocation(location, false);

        public static void SetVictimData(Ped ped, string injury, string cause, float survivability)
        {
            if (PluginCheck.IsBetterEMSRunning()) VictimData(ped, injury, cause, survivability);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VictimData(Ped ped, string injury, string cause, float survivability)
            =>
                BetterEMS.API.EMSFunctions.OverridePedDeathDetails(ped, injury, cause,
                    Convert.ToUInt32(Fiskey111Common.Rand.RandomNumber(0000, 24000)), survivability);


        public static bool? WasPedRevived(Ped ped) => PluginCheck.IsBetterEMSRunning() ? EmsRevivePed(ped) : null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool? EmsRevivePed(Ped ped) => BetterEMS.API.EMSFunctions.DidEMSRevivePed(ped);

        public static void OverridePedDetails(Ped ped, string bone, string cause, uint time, float survival)
            => OverridePedDetail(ped, bone, cause, time, survival);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OverridePedDetail(Ped p, string boneName, string cause, uint tod, float survival)
            => BetterEMS.API.EMSFunctions.OverridePedDeathDetails(p, boneName, cause, tod, survival);
    }

    class ComputerPlusAPI
    {
        public static Guid CreateCallout(string name, Vector3 loc, int resType, string desc = "", int status = 1,
            List<Ped> peds = null, List<Vehicle> vehs = null)
            =>
                Functions.CreateCallout(new CalloutData(name, name, loc, (ComputerPlus.EResponseType) resType, desc,
                    (ComputerPlus.ECallStatus) status, peds, vehs));

        public static void UpdateCalloutStatus(Guid id, int Status)
            => Functions.UpdateCalloutStatus(id, (ECallStatus) Status);

        public static void UpdateCalloutDescription(Guid id, string desc)
            => Functions.UpdateCalloutDescription(id, desc);

        public static void SetCalloutStatusToAtScene(Guid id) => Functions.SetCalloutStatusToAtScene(id);

        public static void ConcludeCallout(Guid id) => Functions.ConcludeCallout(id);

        public static void CancelCallout(Guid id) => Functions.CancelCallout(id);

        public static void SetCalloutStatusToUnitResponding(Guid id) => Functions.SetCalloutStatusToUnitResponding(id);

        public static void AddPedToCallout(Guid id, Ped ped) => Functions.AddPedToCallout(id, ped);

        public static void AddUpdateToCallout(Guid id, string update) => Functions.AddUpdateToCallout(id, update);

        public static void AddVehicleToCallout(Guid id, Vehicle vehicle) => Functions.AddVehicleToCallout(id, vehicle);

        public static void AssignCallToAIUnit(Guid id) => Functions.AssignCallToAIUnit(id);
    }
}
