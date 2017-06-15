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

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Foot Pursuit", CalloutProbability.Medium)]
    public class FootPursuit : BaseCallout
    {
        private EState _state;
        
        private const string CallName = "";
        private const string CalloutMsg = "~b~Officer~w~ requires assistance - respond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officer~w~ in need of assistance with foot pursuit; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Foot pursuit in progress.  Multiple suspects possible.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Fiskey111Common.OfficerSettings.UnitName()} WE_HAVE CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Fiskey111Common.OfficerSettings.UnitName()} EN_ROUTE_CODE3 CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01 NONLETHAL_WEAPONS";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            DisplayCalloutMessage(CalloutMsg);

            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officer requires immediate backup for foot pursuit near {World.GetStreetName(SpawnPoint)}.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            CreateCopsOnScene();

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 4));

            ResponseInfo = CalloutResponseInfo;

            GiveWeaponOrArmor(PedList);

            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            SpawnBlip = false;

            AssignCopsToPursuit(PedList);

            _state = EState.Accepted;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            switch (_state)
            {
                case EState.Accepted:
                    if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) > 100f) break;

                    _state = EState.Checking;
                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);
                    break;
                case EState.Checking:
                    if (IsPursuit && IsPursuitCompleted())
                        CalloutFinished();
                    else if (PedList.PedCheck())
                        CalloutFinished();
                    break;
            }
        }
        
        public enum EState { Accepted, Checking }
    }
}
