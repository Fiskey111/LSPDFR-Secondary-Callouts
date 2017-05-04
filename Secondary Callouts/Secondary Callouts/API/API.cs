using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ComputerPlus;
using ComputerPlus.API;
using Rage;

namespace Secondary_Callouts.API
{
    public class BetterEMSAPI
    {
        public static void RequestEms(Vector3 loc)
        {
            if (PluginCheck.IsBetterEMSRunning()) EmsRespond(loc);
        }
        
        private static void EmsRespond(Vector3 location) => BetterEMS.API.EMSFunctions.RespondToLocation(location, false);

        public static void SetVictimData(Ped ped, string injury, string cause, float survivability)
        {
            if (PluginCheck.IsBetterEMSRunning()) VictimData(ped, injury, cause, survivability);
        }
        
        private static void VictimData(Ped ped, string injury, string cause, float survivability)
            =>
                BetterEMS.API.EMSFunctions.OverridePedDeathDetails(ped, injury, cause,
                    Convert.ToUInt32(Fiskey111Common.Rand.RandomNumber(0000, 24000)), survivability);


        public static bool? WasPedRevived(Ped ped) => PluginCheck.IsBetterEMSRunning() ? EmsRevivePed(ped) : null;
        
        private static bool? EmsRevivePed(Ped ped) => BetterEMS.API.EMSFunctions.DidEMSRevivePed(ped);

        public static void OverridePedDetails(Ped ped, string bone, string cause, uint time, float survival)
            => OverridePedDetail(ped, bone, cause, time, survival);
        
        private static void OverridePedDetail(Ped p, string boneName, string cause, uint tod, float survival)
            => BetterEMS.API.EMSFunctions.OverridePedDeathDetails(p, boneName, cause, tod, survival);
    }

    public class ComputerPlusAPI
    {
        public static Guid CreateCallout(string name, Vector3 loc, int resType, string desc = "", int status = 1,
            List<Ped> peds = null, List<Vehicle> vehs = null)
        {
            if (!PluginCheck.IsComputerPlusRunning()) return Guid.Empty;

            return Functions.CreateCallout(new CalloutData(name, name, loc, (ComputerPlus.EResponseType) resType, desc,
                    (ComputerPlus.ECallStatus) status, peds, vehs));
        }

        public static void UpdateCalloutStatus(Guid id, int status)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.UpdateCalloutStatus(id, (ECallStatus) status);
        }

        public static void UpdateCalloutDescription(Guid id, string desc)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.UpdateCalloutDescription(id, desc);
        }

        public static void SetCalloutStatusToAtScene(Guid id)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.SetCalloutStatusToAtScene(id);
        }

        public static void ConcludeCallout(Guid id)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.ConcludeCallout(id);
        }

        public static void CancelCallout(Guid id)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.CancelCallout(id);
        }

        public static void SetCalloutStatusToUnitResponding(Guid id)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.SetCalloutStatusToUnitResponding(id);
        }

        public static void AddPedToCallout(Guid id, Ped ped)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.AddPedToCallout(id, ped);
        }

        public static void AddUpdateToCallout(Guid id, string update)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.AddUpdateToCallout(id, update);
        }

        public static void AddVehicleToCallout(Guid id, Vehicle vehicle)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.AddVehicleToCallout(id, vehicle);
        }

        public static void AssignCallToAIUnit(Guid id)
        {
            if (PluginCheck.IsComputerPlusRunning()) Functions.AssignCallToAIUnit(id);
        }

        public enum ResponseType { Code_2, Code_3 }
    }
}
