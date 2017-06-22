using System;
using System.Collections.Generic;
using System.Linq;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;
using static Fiskey111Common.Rand;
using static Secondary_Callouts.ExtensionMethods.Entities.PedComponent;
using static Secondary_Callouts.ExtensionMethods.Entities.PropID;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Heavily Armed Person", CalloutProbability.VeryLow)]
    public class HeavilyArmed : BaseCallout
    {
        private const string CallName = "Heavily Armed Individual(s)";
        private const string LSPDFRmsg = "~b~Officers~w~ require assistance with ~y~shots fired";
        private const string CalloutMsg = "~r~Shots fired~w~ on ~b~Officers~w~ by individual(s) with ~y~heavy armor and weapons~w~\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officers~w~ in need of assistance with shots fired; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Heavily armed individual firing shots.  Multiple suspects possible.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} CODE99_IMMEDIATE UNITS_REPORTING CRIME_GUNFIRE";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} RESPOND_CODE3 SHOTS_OFFICER_LETHAL_FORCE SUSPECT_IS HEAVILY_ARMED";

        private string _mgScanner = "ATTN_DISPATCH 1099_ALL_RESPOND SUSPECT_IS CARRYING_MGDISPATCHING_SWAT";


        private string[] _validModelArray = new[]
        {
            "player_two",
            "mp_m_freemode_01",
            "mp_m_freemode_01"
        };


        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = LSPDFRmsg;

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officers requires immediate backup for shots fired near {World.GetStreetName(SpawnPoint)}.\nHeavily armed individuals.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            CreateCopsOnScene(true);

            PedList = SpawnPeds(RandomNumber(1, 3), 12f, 13f);

            GiveHeavyWeapons(PedList);

            AddPedListWeapons(PedList, PedType.Type.Suspect);

            DisplayAdditionalInformation(CalloutMsg);

            for (var i = 0; i < RandomNumber(2); i++) SpawnSpecialPed();

            ResponseInfo = CalloutResponseInfo;
            
            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            CalloutEState = EState.Accepted;
            
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            switch (CalloutEState)
            {
                case EState.Accepted:
                    if (PlayerDistanceFromSpawnPoint > 100f) break;

                    CalloutEState = EState.OnScene;

                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);

                    SetRelationshipGroups(PedList, "FiskeyPerps");
                    
                    SetRelationships(PedList, CopPedList);
                    SetRelationships(PedList, PedList.FirstOrDefault().GetNearbyPeds(5));

                    SetPlayerRelationships(CopPedList, Relationship.Companion);
                    SetPlayerRelationships(PedList);

                    GiveFightTasks(PedList);

                    LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(_mgScanner);

                    for (var i = 0; i < RandomNumber(4); i++)
                        SendBackup(SpawnPoint);

                    break;
                case EState.OnScene:
                    if (PlayerDistanceFromSpawnPoint > 45f) break;

                    CalloutEState = EState.Checking;

                    foreach (var ped in PedList)
                    {
                        if (!ped) continue;
                        ped.Tasks.TakeCoverFrom(CopPedList.FirstOrDefault(), -1, true);
                    }

                    break;
                case EState.Checking:
                    IsNearAnyPed(PedList, CopPedList);
                    PedList = SuspectPositionCheck(PedList);
                    if (PedCheck(PedList.ToList()))
                    {
                        CalloutFinished();
                        this.End();
                    }
                    break;
            }
        }

        private void SpawnSpecialPed()
        {
            if (RandomNumber(3) != 0) return;

            var ped = new Ped(new Model(_validModelArray[RandomNumber(_validModelArray.Length)]),
                PedList[0].Position.Around2D(1f), 0f);

            GivePedArmorComponents(ped);
            GiveHeavyWeapons(ped);

            PedList.Add(ped);
        }

        private void GiveHeavyWeapons(Ped ped)
        {
            WeaponAsset weapon;
            switch (RandomNumber(1, 5))
            {
                case 1:
                    weapon = new WeaponAsset((uint)WeaponHash.Minigun);
                    break;
                case 2:
                    weapon = new WeaponAsset((uint)WeaponHash.AssaultShotgun);
                    break;
                case 3:
                    weapon = new WeaponAsset((uint)WeaponHash.CombatMG);
                    break;
                default:
                    weapon = new WeaponAsset((uint)WeaponHash.AssaultRifle);
                    break;
            }
            var gun1 = new Weapon(weapon, SpawnPoint, 500);
            gun1.GiveTo(ped);
            ped.MaxHealth = RandomNumber(200, 751);
            ped.Armor = RandomNumber(150, 401);
            Game.LogTrivial($"Armor: {ped.Armor}; Health: {ped.MaxHealth}");
        }

        private void GiveHeavyWeapons(List<Ped> pedList)
        {
            if (pedList.Count < 1) return;

            foreach (var ped in pedList)
            {
                WeaponAsset weapon;
                switch (RandomNumber(1, 5))
                {
                    case 1:
                        weapon = new WeaponAsset((uint) WeaponHash.APPistol);
                        break;
                    case 2:
                        weapon = new WeaponAsset((uint) WeaponHash.AssaultShotgun);
                        break;
                    case 3:
                        weapon = new WeaponAsset((uint) WeaponHash.CombatMG);
                        break;
                    default:
                        weapon = new WeaponAsset((uint) WeaponHash.AssaultRifle);
                        break;
                }
                var gun1 = new Weapon(weapon, SpawnPoint, 500);
                gun1.GiveTo(ped);
                ped.Armor = RandomNumber(50, 101);
                Game.LogTrivial($"Armor: {ped.Armor}; Health: {ped.MaxHealth}");
            }
        }

        private void GivePedArmorComponents(Ped ped)
        {
            if (!ped) return;
            try
            {
                if (string.Equals(ped.Model.Name, _validModelArray[0], StringComparison.CurrentCultureIgnoreCase))
                {
                    ped.GivePedVariation(Torso, 2);
                    ped.GivePedVariation(Legs, 2);
                    ped.GivePedVariation(Hands, 1);
                    ped.GivePedVariation(Foot, 2);
                    ped.GivePedVariation(Acc1, 2);
                    ped.GivePedVariation(Acc2, 2);
                    ped.GivePedProp(Helmets, 24);
                }
                else if (string.Equals(ped.Model.Name, _validModelArray[1], StringComparison.CurrentCultureIgnoreCase))
                {
                    ped.GivePedVariation(Torso, 131);
                    ped.GivePedVariation(Legs, 84);
                    ped.GivePedVariation(Hands, 0);
                    ped.GivePedVariation(Foot, 54);
                    ped.GivePedVariation(Acc1, 97);
                    ped.GivePedVariation(AuxTorso, 186);
                    ped.GivePedProp(Helmets, 39);
                    ped.GivePedProp(Glasses, 16);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial(ex.ToString());
            }
        }

        private void SetRelationships(IEnumerable<Ped> pedList1, IEnumerable<Ped> pedList2,
            Relationship groupRelationship = Relationship.Hate)
        {
            var list1 = pedList1 as Ped[] ?? pedList1.ToArray();
            var list2 = pedList2 as Ped[] ?? pedList2.ToArray();
            if (list1.Length < 1 || list2.Length < 1) return;

            var startPed = list1.FirstOrDefault();
            if (startPed == null) return;

            foreach (var targetPed in list2)
            {
                if (!targetPed || targetPed.RelationshipGroup == startPed.RelationshipGroup) continue;

                Game.SetRelationshipBetweenRelationshipGroups(startPed.RelationshipGroup, targetPed.RelationshipGroup,
                    groupRelationship);
            }
        }
    }
}
