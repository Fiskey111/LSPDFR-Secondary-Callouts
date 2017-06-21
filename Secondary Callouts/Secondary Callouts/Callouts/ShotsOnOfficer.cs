using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Shots Fired on Officer", CalloutProbability.Medium)]
    public class ShotsOnOfficer : BaseCallout
    {
        private const string CallName = "Shots Fired on Officer";
        private const string CalloutMsg = "~r~Shots fired~w~ on ~b~officer~w~\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officers~w~ in need of assistance with shots fired; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Multiple individuals reported firing on officers; some may be armed with heavy weapons.\nOfficers on scene";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitName} CRIME_SHOTS_FIRED_AT_AN_OFFICER";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitName} SHOTS_OFFICER_LETHAL_FORCE RESPOND_CODE3 ALL_RESPOND";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            if (Settings.ShotsFiredCallAudio) _startScanner = $"SHOTS BEEP_LONG BEEP_LONG BEEP_LONG REQUEST_BACKUP STATIC ATTN_UNIT_02 {Settings.UnitName} CRIME_SHOTS_FIRED_AT_AN_OFFICER";

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            StartScannerAudio = _startScanner;

            AudioTime = 10000;

            ComputerPlus_CallMsg = $"Shots fired on officers reported near {World.GetStreetName(SpawnPoint)}. Officers on scene.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            GameFiber.StartNew(delegate
            {
                AcceptScannerAudio = _acceptAudio;

                PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(3, 10), 4f);

                CreateCopsOnScene(true);

                ResponseInfo = CalloutResponseInfo;

                GiveWeapons(PedList);

                AddPedListWeapons(PedList, PedType.Type.Suspect);

                if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

                CalloutEState = EState.EnRoute;
            });

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            switch (CalloutEState)
            {
                case EState.EnRoute:
                    if (PlayerDistanceFromSpawnPoint > 100f) break;

                    CalloutEState = EState.DecisionMade;
                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);

                    SetRelationshipGroups(PedList, "Fiskey111Perps");

                    SetRelationshipsHate(PedList, CopPedList);

                    SetPlayerRelationships(PedList);
                    SetPlayerRelationships(CopPedList, Relationship.Companion);

                    GiveFightTask(PedList);

                    break;
                case EState.DecisionMade:
                    if (PlayerDistanceFromSpawnPoint > 30f) break;

                    CalloutEState = EState.Checking;
                    if (AreaBlip.Exists()) AreaBlip.Delete();

                    break;
                case EState.Checking:
                    if (PedCheck(SuspectPositionCheck(PedList).ToList()))
                        CalloutFinished();

                    break;
            }
        }
        
        private void GiveWeapons(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;

            foreach (var ped in enumerable)
            {
                switch (Fiskey111Common.Rand.RandomNumber(1, 5))
                {
                    case 1:
                        var gun1 = new Weapon(new WeaponAsset((uint)WeaponHash.MicroSMG), SpawnPoint, 100);
                        gun1.GiveTo(ped);
                        break;
                    case 2:
                        var gun2 = new Weapon(new WeaponAsset((uint)WeaponHash.Pistol), SpawnPoint, 100);
                        gun2.GiveTo(ped);
                        break;
                    case 3:
                        var gun3 = new Weapon(new WeaponAsset((uint)WeaponHash.PumpShotgun), SpawnPoint, 100);
                        gun3.GiveTo(ped);
                        break;
                    default:
                        ped.Armor = 100;
                        var gun4 = new Weapon(new WeaponAsset((uint)WeaponHash.AssaultRifle), SpawnPoint, 100);
                        gun4.GiveTo(ped);
                        break;
                }
            }
        }

        private void GiveFightTask(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;

            foreach (var ped in enumerable)
            {
                if (!ped) continue;

                ped.Tasks.FightAgainstClosestHatedTarget(30f);
            }
        }
    }
}
