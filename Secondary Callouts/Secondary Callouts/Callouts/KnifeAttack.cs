using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Knife Attack in Progress", CalloutProbability.Medium)]
    public class KnifeAttack : BaseCallout
    {
        private List<Ped> _targetList = new List<Ped>();

        private const string CallName = "Knife Attack in Progress";
        private const string CalloutMsg = "~y~Knife Attack~w~ in progress\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~y~Civilians~w~ report individual with knife; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Individual with a knife reported attacking individuals.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} CITIZENS_REPORT PERSON_WITH_KNIFE";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} CRIME_AMBULANCE_REQUESTED_03 RESPOND_CODE3 PROCEED_CAUTION";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            StartScannerAudio = _startScanner;

            GiveBlipInfo(CalloutStandardization.BlipTypes.AreaSearch, 0.75f);
            ComputerPlus_CallMsg = $"Individual with knife reported near {World.GetStreetName(SpawnPoint)}. Multiple involved parties.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 3));
            _targetList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 4));

            ResponseInfo = CalloutResponseInfo;

            GiveWeaponOrArmor(PedList);

            AddPedListWeapons(PedList, PedType.Type.Suspect);
            AddPedList(_targetList, PedType.Type.Victim);

            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            CalloutEState = EState.EnRoute;

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
                    
                    GiveKnifeOrWeapon(PedList);

                    SetRelationshipGroups(PedList, "Fiskey111Perps");
                    SetRelationshipGroups(_targetList, "Fiskey111Targets");

                    SetRelationshipsHate(PedList, _targetList);
                    SetRelationshipsHate(_targetList, PedList);
                    SetPlayerRelationships(_targetList, Relationship.Companion);
                    
                    StartFightTask(PedList);

                    GiveFleeTask(_targetList);
                    break;
                case EState.DecisionMade:
                    if (PlayerDistanceFromSpawnPoint > 50f) break;
                    if (!StartedWeaponFireCheck)
                    {
                        StartWeaponFireCheck(PedList.ToList());
                        StartWeaponFireCheck(_targetList.ToList());
                    }
                    
                    if (PlayerDistanceFromSpawnPoint > 30f) break;
                    StartPursuit();

                    CalloutEState = EState.Checking;

                    break;
                case EState.Checking:
                    IsNearAnyPed(_targetList, PedList);
                    PedList = SuspectPositionCheck(PedList);
                    _targetList = SuspectPositionCheck(_targetList);
                    if (PedCheck(PedList.ToList()))
                    {
                        CalloutFinished();
                        this.End();
                    }

                    break;
            }
        }

        private void GiveKnifeOrWeapon(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;

            foreach (var ped in enumerable)
            {
                WeaponAsset weapon;
                switch (Fiskey111Common.Rand.RandomNumber(1, 5))
                {
                    case 1:
                        weapon = new WeaponAsset((uint)WeaponHash.Pistol);
                        break;
                    case 2:
                        weapon = new WeaponAsset((uint)WeaponHash.SawnOffShotgun);
                        break;
                    case 3:
                        weapon = new WeaponAsset((uint)WeaponHash.APPistol);
                        break;
                    default:
                        weapon = new WeaponAsset((uint)WeaponHash.Knife);
                        break;
                }

                var gun1 = new Weapon(weapon, SpawnPoint, 100);
                gun1.GiveTo(ped);
            }
        }
        private void StartPursuit()
        {
            if (Fiskey111Common.Rand.RandomNumber(1, 5) != 1) return;

            var pedList = new List<Ped>();
            foreach (var p in PedList)
            {
                if (PedList.IndexOf(p) == 0)
                {
                    pedList.Add(p);
                    continue;
                }
                if (Fiskey111Common.Rand.RandomNumber(1, 5) == 1) pedList.Add(p);
            }

            $"Starting pursuit with {pedList.Count} runners".AddLog();

            CreatePursuit(pedList);
        }

        private void StartFightTask(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;
            foreach (var p in enumerable)
            {
                if (!p) continue;
                if (IsPursuit && Functions.GetPursuitPeds(PursuitHandler).Contains(p)) continue;
                p.Tasks.FightAgainstClosestHatedTarget(30f);
            }
        }

        private void GiveFleeTask(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;
            foreach (var p in enumerable)
            {
                if (!p || Fiskey111Common.Rand.RandomNumber(3) == 1) continue;
                "Fleeing ped".AddLog();
                p.KeepTasks = true;
                p.Tasks.ReactAndFlee(PedList.FirstOrDefault());

            }
        }
    }
}
