using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Fiskey111Common;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;
using static Secondary_Callouts.Objects.Animation.EMSAnimation;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Officer Down", CalloutProbability.Low)]
    public class OfficerShot : BaseCallout
    {
        private const string CallName = "Officer Down";
        private const string CalloutMsg = "~r~Officer down~w~\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~r~Officer down~w~; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Officer down; Suspect fleeing the scene.\nNo officers on scene, EMS en route";

        private readonly string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} CRIME_SHOTS_FIRED_AT_AN_OFFICER";
        private readonly string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} SHOTS_OFFICER_LETHAL_FORCE RESPOND_CODE3 ALL_RESPOND";

        private bool _helpDisplayed;
        private bool _cprStarted;
        private readonly LtFlash.Common.Processes.ProcessHost _procHost = new LtFlash.Common.Processes.ProcessHost();

        private static Ped Player => Game.LocalPlayer.Character;

        private readonly Animation[] _playerAnims = new[]
        {
            new Animation(Start, "mini@cpr@char_a@cpr_def", "cpr_intro", 16000, AnimationFlags.StayInEndFrame),
            new Animation(Pump, "mini@cpr@char_a@cpr_str", "cpr_pumpchest", 750, AnimationFlags.StayInEndFrame),
            new Animation(Mouth_to_CPR, "mini@cpr@char_a@cpr_str", "cpr_cpr_to_kol", 1500, AnimationFlags.StayInEndFrame),
            new Animation(CPR_to_Mouth, "mini@cpr@char_a@cpr_str", "cpr_kol_to_cpr", 1500, AnimationFlags.StayInEndFrame),
            new Animation(MTM, "mini@cpr@char_a@cpr_str", "cpr_kol", 8000, AnimationFlags.StayInEndFrame),
            new Animation(Success, "mini@cpr@char_a@cpr_str", "cpr_success", 32000, AnimationFlags.None),
            new Animation(Fail, "mini@cpr@char_a@cpr_str", "cpr_fail", 26000, AnimationFlags.None)
        };

        private readonly Animation[] _targetAnims = new[]
        {
            new Animation(Start, "mini@cpr@char_b@cpr_def", "cpr_intro", 16000, AnimationFlags.StayInEndFrame),
            new Animation(Pump, "mini@cpr@char_b@cpr_str", "cpr_pumpchest", 750, AnimationFlags.StayInEndFrame),
            new Animation(Mouth_to_CPR, "mini@cpr@char_b@cpr_str", "cpr_cpr_to_kol", 1500, AnimationFlags.StayInEndFrame),
            new Animation(CPR_to_Mouth, "mini@cpr@char_b@cpr_str", "cpr_kol_to_cpr", 1500, AnimationFlags.StayInEndFrame),
            new Animation(MTM, "mini@cpr@char_b@cpr_str", "cpr_kol", 8000, AnimationFlags.StayInEndFrame),
            new Animation(Success, "mini@cpr@char_b@cpr_str", "cpr_success", 32000, AnimationFlags.None),
            new Animation(Fail, "mini@cpr@char_b@cpr_str", "cpr_fail", 26000, AnimationFlags.StayInEndFrame)
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = CalloutMsg;
            ComputerPlus_CallMsg = $"Officer down near {World.GetStreetName(SpawnPoint)}. Suspect at large.";
            StartScannerAudio = _startScanner;

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;
            ResponseInfo = CalloutResponseInfo;
            if (ComputerPlus_Active) ComputerPlusAPI.AddUpdateToCallout(ComputerPlus_GUID, ComputerPlusUpdate);

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 10), 4f);
            CreateCopsOnScene(false, true, false, 1);

            GiveWeaponOrArmor(PedList);
            AddPedListWeapons(PedList, PedType.Type.Suspect);

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

                    SetPedAsPursuitOrFight(PedList);

                    break;
                case EState.DecisionMade:
                    if (PlayerDistanceFromSpawnPoint > 30f) break;

                    CalloutEState = EState.Checking;
                    if (AreaBlip.Exists()) AreaBlip.Delete();

                    break;
                case EState.Checking:
                    if (IsNearOfficer)
                    {
                        if (_cprStarted) return;

                        if (!_helpDisplayed)
                        {
                            _helpDisplayed = true;
                            Game.DisplayHelp("Press ~y~Y~w~ to perform CPR on the ~y~officer~w~ while you wait for ~g~EMS~w~");
                        }
                        if (!Game.IsKeyDown(System.Windows.Forms.Keys.Y)) return;
                        
                        _procHost.ActivateProcess(StartCPR);
                        _procHost.Start();
                    }

                        break;
            }
        }

        private void SetPedAsPursuitOrFight(List<Ped> pedList)
        {
            if (pedList.Count < 1) return;
            var pursuitList = new List<Ped>();
            foreach (var ped in pedList)
            {
                if (!ped) return;

                if (Fiskey111Common.Rand.RandomNumber(4) == 1)
                {
                    ped.RelationshipGroup = "Fiskey111Perp";
                    Game.SetRelationshipBetweenRelationshipGroups(ped.RelationshipGroup, Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
                    ped.Tasks.FightAgainstClosestHatedTarget(60f);
                }
                else
                    pursuitList.Add(ped);
            }

            if (pursuitList.Count > 0) CreatePursuit(pursuitList);
        }

        private bool IsNearOfficer => Game.LocalPlayer.Character.Position.DistanceTo(CopPedList.FirstOrDefault().LeftPosition) < 1f;

        private void StartCPR()
        {
            "Starting CPR".AddLog();
            _cprStarted = true;
            var cop = CopPedList[0];

            Player.Position = cop.LeftPosition;
            Player.Face(cop);

            var startPlayer = _playerAnims.GetAnimation(Start);
            var startCop = _targetAnims.GetAnimation(Start);


            cop.PlayAnimation(startCop);
            Player.PlayAnimationWait(startPlayer);

            _procHost.SwapProcesses(StartCPR, AwaitKeypress);
        }

        private void AwaitKeypress()
        {
            Game.DisplayHelp("Press ~y~Y~w~ to perform a compression" +
                             "\nPress ~y~U~w~ to perform mouth-to-mouth", true);

            while (!Game.IsKeyDown(Keys.Y) || Game.IsKeyDown(Keys.U))
                GameFiber.Yield();

            if (Game.IsKeyDown(Keys.Y))
            {
                _procHost.SwapProcesses(StartCPR, CPRPump);
            }
            else
            {
                _procHost.SwapProcesses(StartCPR, CPRMouth);
            }
        }

        private void CPRPump()
        {
            Game.HideHelp();

            var pumpPlayer = _playerAnims.GetAnimation(Pump);
            var pumpCop = _targetAnims.GetAnimation(Pump);
            var cop = CopPedList[0];

            cop.PlayAnimation(pumpCop);
            Player.PlayAnimationWait(pumpPlayer);

            if (Rand.RandomNumber(4) == 1) _procHost.SwapProcesses(CPRPump, SuccessFail);
            _procHost.SwapProcesses(CPRPump, AwaitKeypress);
        }

        private void CPRMouth()
        {
            Game.HideHelp();

            var startPlayer = _playerAnims.GetAnimation(CPR_to_Mouth);
            var startCop = _targetAnims.GetAnimation(CPR_to_Mouth);
            var mtmPlayer = _playerAnims.GetAnimation(MTM);
            var mtmCop = _targetAnims.GetAnimation(MTM);
            var stopPlayer = _playerAnims.GetAnimation(Mouth_to_CPR);
            var stopCop = _targetAnims.GetAnimation(Mouth_to_CPR);
            var cop = CopPedList[0];

            cop.PlayAnimation(startCop);
            Player.PlayAnimationWait(startPlayer);

            cop.PlayAnimation(mtmCop);
            Player.PlayAnimationWait(mtmPlayer);

            cop.PlayAnimation(stopCop);
            Player.PlayAnimationWait(stopPlayer);

            if (Rand.RandomNumber(4) == 1) _procHost.SwapProcesses(CPRMouth, SuccessFail);
            _procHost.SwapProcesses(CPRMouth, AwaitKeypress);

        }

        private void SuccessFail()
        {
            Game.HideHelp();

            var successPlayer = _playerAnims.GetAnimation(Success);
            var successCop = _targetAnims.GetAnimation(Success);
            var failPlayer = _playerAnims.GetAnimation(Fail);
            var failCop = _targetAnims.GetAnimation(Fail);
            var cop = CopPedList[0];

            if (MathHelper.GetChance(2))
            {
                cop.PlayAnimation(successCop);
                cop.Resurrect();
                cop.Health = 1;
                cop.IsInvincible = true;
                Player.PlayAnimationWait(successPlayer);
                Player.PlayAmbientSpeech("CHAT_STATE");
                Game.DisplayHelp("Take the ~b~officer~w~ to the nearest ~g~hospital~w~");
                NativeFunction.Natives.CAN_SHUFFLE_SEAT<bool>(Player.LastVehicle, false);
                cop.Tasks.EnterVehicle(Player.LastVehicle, 0);
                _stopwatch.Start();
                _procHost.SwapProcesses(SuccessFail, WaitForEnterVehicle);
            }
            else
            {
                cop.PlayAnimation(failCop);
                Player.PlayAnimationWait(failPlayer);
                Player.PlayAmbientSpeech("GENERIC_SHOCKED_HIGH");
                _procHost.SwapProcesses(SuccessFail, WaitForPursuitEnd);
            }
        }

        private void WaitForPursuitEnd()
        {
            if (!PedCheck(PedList)) return;

            this.End();
            _procHost.Stop();
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Vector3 _closestHospital;
        private void WaitForEnterVehicle()
        {
            if (!CopPedList[0].IsInAnyVehicle(false) || _stopwatch.Elapsed.Seconds < 15) return;

            if (_stopwatch.Elapsed.Seconds >= 15)
            {
                CopPedList[0].WarpIntoVehicle(Player.LastVehicle, 0);
                _stopwatch.Reset();
            }
            
            if (!Player.IsInAnyVehicle(false)) return;
            
            _procHost.SwapProcesses(WaitForEnterVehicle, SetWaypoint);
        }

        private void SetWaypoint()
        {
            _closestHospital = GetClosestHospital();
            var blip = new Blip(_closestHospital, 10f);
            blip.SetStandardColor(CalloutStandardization.BlipTypes.Support);
            BlipList.Add(blip);

            Game.DisplayHelp("Drive to the nearest ~g~hospital~w~");

            _procHost.SwapProcesses(SetWaypoint, AwaitArrival);
        }

        private void AwaitArrival()
        {
            if (PlayerDistanceFromPosition(_closestHospital) > 10f) return;

            Game.DisplayHelp("Stop and press ~y~Y~w~ to admit the officer");

            while (Game.IsKeyDown(Keys.Y)) GameFiber.Yield();

            _procHost.SwapProcesses(AwaitArrival, AdmitOfficer);
        }

        private void AdmitOfficer()
        {
            if (Player.CurrentVehicle.Speed > 0.1f && !Game.IsKeyDown(Keys.Y)) return;

            Game.DisplayNotification("Admitting ~b~officer~w~...");
            var cop = CopPedList[0];
            if (!cop) this.End();
            cop.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
            cop.Tasks.GoStraightToPosition(_closestHospital, 1f, 0f, 0f, -1);

            while (cop.IsInAnyVehicle(true)) GameFiber.Yield();

            if (PedCheck(PedList))
            {
                this.End();
                _procHost.Stop();
            }

            Game.DisplayHelp("The ~~b~officer~w~ will be admitted now; go get that ~r~suspect~w~!");

            _procHost.SwapProcesses(AdmitOfficer, WaitForPursuitEnd);
        }

        private Vector3 GetClosestHospital()
        {
            var hospitals = new[]
            {
                new Vector3(),
                new Vector3()
            };

            return hospitals.OrderBy(hosp => hosp.TravelDistanceTo(Game.LocalPlayer.Character.Position)).FirstOrDefault();
        }
    }
}
