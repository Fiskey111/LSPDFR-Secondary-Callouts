using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    [CalloutInfo("Officer Down", CalloutProbability.Low)]
    public class OfficerShot : BaseCallout
    {
        private const string CallName = "Officer Down";
        private const string CalloutMsg = "~r~Officer down~w~\nRespond ~r~Code 3";
        private const string CalloutResponseInfo = "~r~Officer down~w~; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Officer down; Suspect fleeing the scene.\nNo officers on scene, EMS en route";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitName} CRIME_SHOTS_FIRED_AT_AN_OFFICER";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitName} SHOTS_OFFICER_LETHAL_FORCE RESPOND_CODE3 ALL_RESPOND";

        private bool _helpDisplayed, _cprStarted;
        private LtFlash.Common.Processes.ProcessHost _procHost = new LtFlash.Common.Processes.ProcessHost();

        public override bool OnBeforeCalloutDisplayed()
        {
            var isAllowed = false;
#if DEBUG
            isAllowed = true;
#endif
            if (!isAllowed)
            {
                Game.DisplaySubtitle("~r~This isn't ready yet. Try again some other day.~w~ -Fiskey111");
                Game.RawFrameRender += Game_RawFrameRender;
                GameFiber.StartNew(delegate
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    while (sw.Elapsed.Seconds < 5)
                        GameFiber.Yield();
                    Game.RawFrameRender -= Game_RawFrameRender; 
                });
            }

            CalloutName = CallName;
            CalloutMessage = CalloutMsg;

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officer down near {World.GetStreetName(SpawnPoint)}. Suspect at large.";

            return base.OnBeforeCalloutDisplayed();
        }

        private void Game_RawFrameRender(object sender, GraphicsEventArgs e)
        {
            e.Graphics.DrawTexture(Game.CreateTextureFromFile(@"Plugins\LSPDFR\Secondary Callouts\Secret\secret.jpg"), new RectangleF(500f, 200f, 400f, 400f));
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            PedList = SpawnPeds(Fiskey111Common.Rand.RandomNumber(1, 10), 4f);

            CreateCopsOnScene(false, true, false, 1);

            ResponseInfo = CalloutResponseInfo;

            GiveWeaponOrArmor(PedList);

            AddPedListWeapons(PedList, PedType.Type.Suspect);

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

                        StartCPR();
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


        }

        private Animation[] LoadAnimations()
        {
            /*
mini@cpr@char_a@cpr_str cpr_kol 7466
mini@cpr@char_a@cpr_str cpr_kol_idle 7000
mini@cpr@char_a@cpr_str cpr_kol_to_cpr 1566
mini@cpr@char_a@cpr_str cpr_pumpchest 1000
mini@cpr@char_a@cpr_str cpr_success 33600
mini@cpr@char_b@cpr_def cpr_intro 15800
mini@cpr@char_b@cpr_def cpr_pumpchest_idle 7000
mini@cpr@char_b@cpr_str cpr_cpr_to_kol 1666
mini@cpr@char_b@cpr_str cpr_fail 25333
mini@cpr@char_b@cpr_str cpr_kol 7466
mini@cpr@char_b@cpr_str cpr_kol_idle 7000
mini@cpr@char_b@cpr_str cpr_kol_to_cpr 1566
mini@cpr@char_b@cpr_str cpr_pumpchest 1000
mini@cpr@char_b@cpr_str cpr_pumpchest_idle 7000
mini@cpr@char_b@cpr_str cpr_success 33600
             */

            var animations = new[]
            {
                new Animation("A_Intro", "mini@cpr@char_a@cpr_def", "cpr_intro", 15800),
                new Animation("B_Intro", "mini@cpr@char_b@cpr_def", "cpr_intro", 15800),
                new Animation("A_Success", "mini@cpr@char_a@cpr_str", "cpr_success", 33600),
                new Animation("A_Success", "mini@cpr@char_b@cpr_str", "cpr_success", 33600),
            };

            return animations;
        }
    }
}
