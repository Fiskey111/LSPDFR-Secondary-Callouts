using System;
using System.Collections.Generic;
using System.Diagnostics;
using ComputerPlus;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Secondary_Callouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;

namespace SecondaryCallouts
{
    public abstract class BaseCallout : Callout
    {
        public Vector3 SpawnPoint;

        public string CallMsg = "";
        public string StartScannerAudio = "";
        public string AcceptScannerAudio = "";
        public string CalloutName = "";
        public string ResponseInfo = "";

        public bool FalseCall = true;
        public bool IsFalseCall => _isFalseCall;

        public EResponseType ComputerPlus_ResponseType = EResponseType.Code_3;
        public string ComputerPlus_CallMsg = "Respond to the call";
        public bool ComputerPlus_Active => _computerPlus;
        public Guid ComputerPlus_GUID => _callId;

        public List<Ped> PedList = new List<Ped>();
        public Blip AreaBlip;

        public LHandle PursuitHandler;
        public bool IsPursuit;

        private bool _computerPlus, _isFalseCall;
        private Guid _callId;
        private Stopwatch _sw = new Stopwatch();

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.AroundBetween(250f, 350f));

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 150f);
            
            this.CalloutMessage = CallMsg;
            this.CalloutPosition = SpawnPoint;
            
            Functions.PlayScannerAudio(StartScannerAudio);

            if (PluginCheck.IsComputerPlusRunning())
            {
                _computerPlus = true;
                _callId = ComputerPlusAPI.CreateCallout(CalloutName, SpawnPoint,
                (int)ComputerPlus_ResponseType, ComputerPlus_CallMsg);
                ComputerPlusAPI.UpdateCalloutStatus(_callId, (int)ECallStatus.Dispatched);
            }

            _sw.Start();
            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            if (_computerPlus) ComputerPlusAPI.SetCalloutStatusToUnitResponding(_callId);

            if (!string.IsNullOrWhiteSpace(AcceptScannerAudio)) StartSecondaryAudio();

            if (FalseCall && Fiskey111Common.Rand.RandomNumber(1, 15) == 1)
            {
                "False Call".AddLog();
                _isFalseCall = true;
                FalseCallHandler.callState = FalseCallHandler.CallState.Start;
            }

            CalloutName.DisplayNotification(ResponseInfo);

            AreaBlip.EnableRoute(AreaBlip.Color);

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (_computerPlus) ComputerPlusAPI.AssignCallToAIUnit(_callId);
            Functions.PlayScannerAudio("OFFICER_INTRO_01 UNIT_RESPONDING_DISPATCH_04");
        }

        public override void Process()
        {
            base.Process();
            if (_isFalseCall)
            {
                GameFiber.Sleep(0500);
                if (!FalseCallHandler.FalseCall(SpawnPoint, "Fight in Progress")) return;
                this.End();
            }
        }

        public override void End()
        {
            base.End();
            DisplayEndInformation();
            if (_computerPlus) ComputerPlusAPI.ConcludeCallout(_callId);
            if (AreaBlip.Exists()) AreaBlip.Delete();
            PedList.Dismiss();
        }

        public void SpawnPeds(int number = 1, float around1 = 3f, float around2 = 6f)
        {
            for (var l = 1; l < number; l++)
                PedList.Add(new Ped(SpawnPoint.Around(around1, around2)));
            $"Total peds created: {PedList.Count}".AddLog();
        }

        public void CreatePursuit(List<Ped> peds)
        {
            IsPursuit = true;
            PursuitHandler = Functions.CreatePursuit();

            foreach (var ped in peds)
                Functions.AddPedToPursuit(PursuitHandler, ped);

            Functions.SetPursuitIsActiveForPlayer(PursuitHandler, true);
            Functions.SetPursuitCopsCanJoin(PursuitHandler, true);
        }

        public bool IsPursuitCompleted() => IsPursuit && Functions.IsPursuitStillRunning(PursuitHandler);

        private void StartSecondaryAudio()
        {
            _sw.Stop();
            var timeelapsed = Convert.ToInt32(_sw.Elapsed.TotalSeconds);
            $"Elapsed time: {timeelapsed}s".AddLog();
            GameFiber.StartNew(delegate
            {
                GameFiber.Sleep(6000 - timeelapsed);
                Functions.PlayScannerAudio(AcceptScannerAudio);
            });
        }

        private void DisplayEndInformation()
        {
            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 CODE_04_PATROL");
            CalloutName.DisplayNotification("~g~Code 4~w~, good work officer!");
        }
    }
}
