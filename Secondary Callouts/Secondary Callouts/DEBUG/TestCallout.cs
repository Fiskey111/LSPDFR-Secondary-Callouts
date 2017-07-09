using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.DEBUG
{
    [CalloutInfo("TestCallout", CalloutProbability.Medium)]
    public class TestCallout : BaseCallout
    {
        private const string CallName = "Test Pursuit";
        private const string CalloutMsg = "This is a test callout";
        private const string CalloutResponseInfo = "Respond to the callout";
        private const string ComputerPlusUpdate =
            "Test callout";

        private readonly string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01";
        private readonly string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} EN_ROUTE_CODE3 CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01 NONLETHAL_WEAPONS";

        public override bool OnBeforeCalloutDisplayed()
        {
            "Starting OnBeforeCalloutDisplayed".AddLog();
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;
            FalseCall = false;
            StartScannerAudio = _startScanner;
            ComputerPlus_CallMsg = $"Pursuit on: {World.GetStreetName(SpawnPoint)}";
            "Initial data set".AddLog();
            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            "Returning base.OnBeforeCalloutDisplayed".AddLog();
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            "Starting OnCalloutAccepted".AddLog();
            AcceptScannerAudio = _acceptAudio;
            ResponseInfo = CalloutResponseInfo;
            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);
            SpawnBlip = false;
            "Initial data set".AddLog();
            CreateCopsOnScene(false);
            "Cops created".AddLog();
            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(3, 4), 12f, 12f);
            "Peds created".AddLog();
            GiveWeaponOrArmor(PedList);
            AddPedListWeapons(PedList, PedType.Type.Suspect);
            "Peds given weapons and data stored".AddLog();
            CreatePursuit(PedList);
            "Pursuit created".AddLog();
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            if (!PedCheck(PedList.ToList())) return;
            this.End();
        }
    }
}
