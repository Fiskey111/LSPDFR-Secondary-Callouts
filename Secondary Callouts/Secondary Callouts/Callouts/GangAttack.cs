using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Gang Attack", CalloutProbability.Medium)]
    public class GangAttack : BaseCallout
    {
        private List<Ped> _ballasList = new List<Ped>();
        private List<Ped> _lostList = new List<Ped>();

        private const string CallName = "";
        private const string CalloutMsg = "~b~Officers~w~ require assistance\nShots fired by gang members - respond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officers~w~ in need of assistance with shots fired; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Gang members firing shots.  Multiple suspects.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Fiskey111Common.OfficerSettings.UnitName()} CODE99_IMMEDIATE UNITS_REPORTING GANG_RELATED";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Fiskey111Common.OfficerSettings.UnitName()} RESPOND_CODE3 SHOTS_OFFICER_LETHAL_FORCE SUSPECTS_MEMBERS_OF THE_BALLAS AND THE_LOST";

        private string[] _ballasModelArray = new []
        {
            "g_f_y_ballas_01",
            "g_m_y_ballaeast_01",
            "g_m_y_ballaorig_01",
            "g_m_y_ballascout_01"
        };

        private string[] _lostModelArray = new[]
        {
            "g_f_y_lost_01",
            "g_m_y_lost_01",
            "g_m_y_lost_02",
            "g_m_y_lost_03"
        };


        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            DisplayCalloutMessage(CalloutMsg);

            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officers requires immediate backup for shots fired near {World.GetStreetName(SpawnPoint)}.\nGang activity presumed.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            CreateCopsOnScene();

            var ballasSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(5f));
            var lostSpawn = World.GetNextPositionOnStreet(ballasSpawn.Around(5f));

            for (var l = 1; l < Fiskey111Common.Rand.RandomNumber(5, 10); l++)
                _ballasList.Add(new Ped(_ballasModelArray[Fiskey111Common.Rand.RandomNumber(_ballasModelArray.Length)], ballasSpawn.Around(2f, 4f), 0f));

            for (var l = 1; l < Fiskey111Common.Rand.RandomNumber(5, 10); l++)
                _lostList.Add(new Ped(_lostModelArray[Fiskey111Common.Rand.RandomNumber(_lostModelArray.Length)], lostSpawn.Around(2f, 4f), 0f));

            ResponseInfo = CalloutResponseInfo;

            GiveWeapons(PedList);

            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);
            
            State = EState.Accepted;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            switch (State)
            {
                case EState.Accepted:
                    if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) > 100f) break;

                    State = EState.Checking;

                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);

                    SetRelationshipGroups(_ballasList, "FiskeyBallas");
                    SetRelationshipGroups(_lostList, "FiskeyLost");

                    SetRelationshipsHate(_ballasList, _lostList);

                    SetRelationshipsHate(CopPedList, _ballasList);
                    SetRelationshipsHate(CopPedList, _lostList);
                    SetPlayerRelationships(_ballasList);
                    SetPlayerRelationships(_lostList);

                    GiveFightTasks(_ballasList);
                    GiveFightTasks(_lostList);
                    break;
                case EState.Checking:
                    if (IsPursuit && IsPursuitCompleted())
                        CalloutFinished();
                    else if (PedList.PedCheck())
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

        private void SetRelationshipGroups(IEnumerable<Ped> pedList, string relGroup)
        {
            foreach (var ped in pedList)
            {
                if (!ped) continue;

                ped.RelationshipGroup = relGroup;
            }
        }

        private void SetRelationshipsHate(IEnumerable<Ped> pedList1, IEnumerable<Ped> pedList2, Relationship groupRelationship = Relationship.Hate) => Game.SetRelationshipBetweenRelationshipGroups(pedList1.FirstOrDefault().RelationshipGroup, pedList2.FirstOrDefault().RelationshipGroup, groupRelationship);
        private void SetPlayerRelationships(IEnumerable<Ped> pedList2, Relationship groupRelationship = Relationship.Hate) => Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, pedList2.FirstOrDefault().RelationshipGroup, groupRelationship);
    }
}
