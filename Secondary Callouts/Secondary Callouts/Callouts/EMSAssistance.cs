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
            $"ATTN_UNIT_02 {Settings.UnitName} CITIZENS_REPORT ASSAULT_BAT";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitName} NONLETHAL_WEAPONS RESPOND_CODE3";

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

            _ambulance = new Vehicle(new Model("ambulance"), SpawnPoint)
            {
                IsSirenOn = true,
                IsSirenSilent = true
            };

            _emsList = SpawnPeds("s_m_m_paramedic_01", 2, 2f);

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(2, 6));

            AddPedList(_emsList, PedType.Type.Service);

            ResponseInfo = CalloutResponseInfo;

            BlipAlpha = 0.75f;

            GiveWeaponOrArmor(PedList);

            AddPedListWeapons(PedList, PedType.Type.Suspect);

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
                    if (!StartedWeaponFireCheck) StartWeaponFireCheck(PedList.ToList());

                    if (PlayerDistanceFromSpawnPoint > 40f) break;

                    CalloutEState = EState.Checking;
                    if (AreaBlip.Exists()) AreaBlip.Delete();

                    CreateBlips();

                    StartPursuit();

                    _hasArrived = true;
                    break;
                case EState.Checking:
                    CheckIfBeingArrested();
                    if (_hasArrived) PedList = SuspectPositionCheck(PedList.ToList());

                    if (IsPursuit && IsPursuitCompleted() && PedCheck(PedList.ToList()))
                        CalloutFinished();
                    else if (PedCheck(PedList.ToList()))
                        CalloutFinished();
                    break;
            }
        }

        public override void End()
        {
            base.End();

            _ambulance.Dismiss();
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
            foreach (var p in pedList)
            {
                if (!p) continue;
                if (IsPursuit && Functions.GetPursuitPeds(PursuitHandler).Contains(p)) continue;
                p.Tasks.FightAgainstClosestHatedTarget(30f);
            }
        }

        private void CreateBlips()
        {
            foreach (var emt in _emsList)
                if (emt) BlipList.Add(CalloutStandardization.CreateStandardizedBlip(emt, CalloutStandardization.BlipTypes.Support));
        }

        private void CheckIfBeingArrested()
        {
            if (_reactAndFlee) return;

            if (PlayerDistanceFromSpawnPoint > 10f) return;

            foreach (var p in _emsList)
            {
                if (!p) continue;
                p.KeepTasks = true;
                p.BlockPermanentEvents = true;
                p.Tasks.ReactAndFlee(PedList.FirstOrDefault());
            }

            CreatePursuit(PedList);

            foreach (var p in PedList)
            {
                if (!p || !IsPursuit || !IsPedInPursuit(p)) continue;
                p.Tasks.FightAgainst(Game.LocalPlayer.Character);
            }
            
            _reactAndFlee = true;
        }
    }
}
