﻿using LSPD_First_Response.Mod.API;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Fight in Progress", CalloutProbability.Medium)]
    public class Fight : BaseCallout
    {
        private EFightState _state;

        private const string CallName = "";
        private const string CalloutMsg = "~y~Fight~w~ in progress - respond ~r~Code 3";
        private const string CalloutResponseInfo = "~b~Officers~w~ in need of assistance with fight; respond ~y~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Multiple individuals reported fighting; some may be armed with weapons.\nOfficers on scene";

        private string _startScanner =
            $"ATTN_UNIT_02 {Fiskey111Common.OfficerSettings.UnitName()} CITIZENS_REPORT ASSAULT_BAT";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Fiskey111Common.OfficerSettings.UnitName()} NONLETHAL_WEAPONS RESPOND_CODE3";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            DisplayCalloutMessage(CalloutMsg);

            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Fight reported near {World.GetStreetName(SpawnPoint)}. Multiple involved parties. Officers on scene.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 10));

            CreateCopsOnScene();

            ResponseInfo = CalloutResponseInfo;

            if (PedList.Count > 0)
            {
                foreach (var ped in PedList)
                {
                    if (!ped) continue;
                    if (Fiskey111Common.Rand.RandomNumber(13) == 1) GiveWeaponOrArmor(ped);
                }
            }

            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);
            
            _state = EFightState.EnRoute;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (IsFalseCall) return;

            switch (_state)
            {
                case EFightState.EnRoute:
                    if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) > 100f) break;

                    _state = EFightState.DecisionMade;
                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);

                    CommonMethods.DisplayMenuHelp();

                    SetRelationshipGroups();

                    SetRelationshipsHate();

                    StartPursuit();

                    StartFightTask();
                    break;
                case EFightState.DecisionMade:
                    _state = EFightState.Checking;
                    if (AreaBlip.Exists()) AreaBlip.Delete();
                    break;
                case EFightState.Checking:
                    if (IsPursuit && IsPursuitCompleted())
                        CalloutFinished();
                    else if (PedList.PedCheck())
                        CalloutFinished();
                    break;
            }
        }

        private void GiveWeaponOrArmor(Ped ped)
        {
            if (!ped) return;
            switch (Fiskey111Common.Rand.RandomNumber(1, 4))
            {
                case 1:
                    var gun1 = new Weapon("WEAPON_BAT", SpawnPoint, -1);
                    gun1.GiveTo(ped);
                    break;
                case 2:
                    var gun2 = new Weapon("WEAPON_PISTOL", SpawnPoint, 100);
                    gun2.GiveTo(ped);
                    break;
                case 3:
                    ped.Armor = 100;
                    break;
            }
        }

        private void SetRelationshipGroups()
        {
            for (var i = 1; i < PedList.Count + 1; i++)
            {
                var ped = PedList[i - 1];
                if (!ped) continue;
                ped.RelationshipGroup = $"perp{i}";
            }
        }

        private void SetRelationshipsHate()
        {
            foreach (var perp in PedList)
            {
                if (!perp) continue;
                foreach (var perp2 in PedList)
                    Game.SetRelationshipBetweenRelationshipGroups(perp.RelationshipGroup,
                        perp2.RelationshipGroup, Relationship.Hate);
            }
        }

        private void StartPursuit()
        {
            if (Fiskey111Common.Rand.RandomNumber(1, 5) != 1) return;

            var pedList = new List<Ped>();
            foreach (var p in PedList)
            {
                if (PedList.IndexOf(p) == 0) pedList.Add(p);
                if (Fiskey111Common.Rand.RandomNumber(1, 5) == 1) pedList.Add(p);
            }

            $"Starting pursuit with {pedList.Count} runners".AddLog();

            CreatePursuit(pedList);
        }

        private void StartFightTask()
        {
            foreach (var p in PedList)
            {
                if (!p) continue;
                if (IsPursuit && Functions.GetPursuitPeds(PursuitHandler).Contains(p)) continue;
                p.Tasks.FightAgainstClosestHatedTarget(30f);
            }
        }

        public enum EFightState
        {
            EnRoute,
            OnScene,
            DecisionMade,
            Checking
        }
    }
}
