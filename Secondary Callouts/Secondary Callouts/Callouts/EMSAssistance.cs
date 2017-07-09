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
    [CalloutInfo("EMS Requires Assistance", CalloutProbability.Medium)]
    public class EMSAssistance : BaseCallout
    {
        private Vehicle _ambulance;
        private List<Ped> _emsList;

        private bool _reactAndFlee, _hasArrived;

        private const string CallName = "EMS Requires Assistance";
        private const string CalloutMsg = "~g~EMS~w~ requires assistance\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~g~EMS~w~ in need of assistance with fight; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "EMS reported in fight; some may be armed with weapons.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} CITIZENS_REPORT ASSAULT_BAT";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} NONLETHAL_WEAPONS RESPOND_CODE3";

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"EMS requires immediate backup near {World.GetStreetName(SpawnPoint)}. Multiple involved parties.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;
            ResponseInfo = CalloutResponseInfo;
            BlipAlpha = 0.75f;
            CalloutEState = EState.Accepted;
            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            _ambulance = new Vehicle(new Model("ambulance"), SpawnPoint)
            {
                IsSirenOn = true,
                IsSirenSilent = true
            };

            _emsList = SpawnPeds("s_m_m_paramedic_01", 2, 2f);
            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(2, 6));

            AddPedList(_emsList, PedType.Type.Service);
            GiveWeaponOrArmor(PedList);
            AddPedListWeapons(PedList, PedType.Type.Suspect);
            
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

                    CalloutEState = EState.EnRoute;
                    if (ComputerPlus_Active) ComputerPlusAPI.SetCalloutStatusToAtScene(ComputerPlus_GUID);
                    
                    SetRelationshipGroups(PedList, "Fiskey111Perps");
                    SetRelationshipGroups(_emsList, "Fiskey111EMS");

                    SetRelationshipsHate(PedList, _emsList);
                    SetRelationshipsHate(_emsList, PedList);
                    SetPlayerRelationships(_emsList, Relationship.Companion);

                    StartFightTask(PedList);
                    StartFightTask(_emsList);
                    break;
                case EState.EnRoute:
                    if (PlayerDistanceFromSpawnPoint > 50f) break;
                    if (!StartedWeaponFireCheck) StartWeaponFireCheck(PedList);

                    if (PlayerDistanceFromSpawnPoint > 40f) break;

                    CalloutEState = EState.Checking;

                    _hasArrived = true;
                    break;
                case EState.Checking:
                    IsNearAnyPed(PedList, _emsList);
                    CheckIfBeingArrested();
                    PedList = SuspectPositionCheck(PedList);
                    if (PedCheck(PedList))
                    {
                        CalloutFinished();
                        GiveCourtCase(PedList.Where(p => p.IsAlive).ToList(), "Assault and battery on a paramedic");
                        this.End();
                    }
                    break;
            }
        }

        public override void End()
        {
            base.End();

            if (_ambulance) _ambulance.Dismiss();
        }

        private void StartFightTask(List<Ped> pedList)
        {
            foreach (var p in pedList)
            {
                if (!p) continue;
                if (IsPursuit && Functions.GetPursuitPeds(PursuitHandler).Contains(p)) continue;
                p.Tasks.FightAgainstClosestHatedTarget(30f);
            }
        }

        private void CheckIfBeingArrested()
        {
            if (_reactAndFlee || PlayerDistanceFromSpawnPoint > 10f) return;
            _reactAndFlee = true;
            foreach (var p in _emsList)
            {
                if (!p) continue;
                p.KeepTasks = true;
                p.BlockPermanentEvents = true;
                p.Tasks.ReactAndFlee(PedList.FirstOrDefault());
            }
            
            var random = Fiskey111Common.Rand.RandomNumber(0, PedList.Count);
            var list = new List<Ped>();
            for (int i = 0; i < random; i++)
            {
                list.Add(PedList[i]);
            }

            $"Total of {list.Count} peds added to list".AddLog();

            CreatePursuit(list);

            foreach (var p in PedList)
            {
                if (!p || !IsPursuit || !IsPedInPursuit(p)) continue;
                p.Tasks.FightAgainst(Game.LocalPlayer.Character);
            }
        }
    }
}
