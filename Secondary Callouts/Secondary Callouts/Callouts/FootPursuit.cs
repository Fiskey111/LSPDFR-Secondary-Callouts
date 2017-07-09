using System.Linq;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Foot Pursuit", CalloutProbability.Medium)]
    public class FootPursuit : BaseCallout
    {
        private const string CallName = "Officer Needs Assistance with Foot Pursuit";
        private const string CalloutMsg = "~b~Officer~w~ requires assistance\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officer~w~ in need of assistance with foot pursuit; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Foot pursuit in progress.  Multiple suspects possible.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} EN_ROUTE_CODE3 CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01 NONLETHAL_WEAPONS";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            FalseCall = false;

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);

            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officer requires immediate backup for foot pursuit near {World.GetStreetName(SpawnPoint)}.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            CreateCopsOnScene(false);

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 4), 9f, 12f);

            ResponseInfo = CalloutResponseInfo;

            GiveWeaponOrArmor(PedList);

            AddPedListWeapons(PedList, PedType.Type.Suspect);

            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            SpawnBlip = false;

            CreatePursuit(PedList);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            if (PedCheck(PedList.ToList()))
            {
                CalloutFinished();
                GiveCourtCase(PedList.Where(p => p.IsAlive).ToList(), "Resisting arrest; failure to stop");
                this.End();
            }
        }
    }
}
