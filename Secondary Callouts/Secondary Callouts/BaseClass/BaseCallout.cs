using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LSPD_First_Response.Engine.Scripting;
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
        
        public string StartScannerAudio = "";
        public string AcceptScannerAudio = "";
        public string CalloutName = "Officer in Need of Assistance";
        public string ResponseInfo = "";

        public bool FalseCall = true;
        public bool IsFalseCall => _isFalseCall;

        public ComputerPlusAPI.ResponseType ComputerPlus_ResponseType = ComputerPlusAPI.ResponseType.Code_3;
        public string ComputerPlus_CallMsg = "Respond to the call";
        public bool ComputerPlus_Active => _computerPlus;
        public Guid ComputerPlus_GUID => _callId;

        public List<Ped> PedList = new List<Ped>();
        public List<Ped> CopPedList = new List<Ped>();
        public List<Blip> BlipList = new List<Blip>();

        public Blip AreaBlip;
        public CalloutStandardization.BlipTypes BlipType;
        public CalloutStandardization.BlipScale BlipScale;

        public LHandle PursuitHandler;
        public bool IsPursuit;

        private bool _computerPlus, _isFalseCall;
        private Guid _callId;
        private Stopwatch _sw = new Stopwatch();

        public override bool OnBeforeCalloutDisplayed()
        {
            $"Starting callout".AddLog();
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.AroundBetween(250f, 350f));

            this.ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 150f);
            
            this.CalloutPosition = SpawnPoint;
            
            Functions.PlayScannerAudio(StartScannerAudio);

            if (PluginCheck.IsComputerPlusRunning())
            {
                $"Computer+ found".AddLog();
                _computerPlus = true;
                _callId = ComputerPlusAPI.CreateCallout(CalloutName, SpawnPoint,
                (int)ComputerPlus_ResponseType, ComputerPlus_CallMsg);
                ComputerPlusAPI.UpdateCalloutStatus(_callId, 2);
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

            var position = SpawnPoint.Around(5f, 20f);

            AreaBlip = CalloutStandardization.CreateStandardizedBlip(position, BlipType, BlipScale);

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
                CalloutFinished();
            }
        }

        public void CalloutFinished()
        {
            if (_computerPlus) ComputerPlusAPI.ConcludeCallout(_callId);
            DisplayEndInformation();
            this.End();
        }

        public override void End()
        {
            base.End();
            foreach (var blip in BlipList)
                if (blip) blip.Delete();
            if (AreaBlip.Exists()) AreaBlip.Delete();
            PedList.Dismiss();
        }

        public void DisplayCalloutMessage(string message, string title = null) => (title ?? "~b~Officers~w~ in need of assistance").DisplayNotification(message);

        public List<Ped> SpawnPeds(int number = 1, float around1 = 3f, float around2 = 6f)
        {
            var list = new List<Ped>();
            for (var l = 1; l < number; l++)
                list.Add(new Ped(SpawnPoint.Around(around1, around2)));
            $"Total peds created: {list.Count}".AddLog();
            return list;
        }

        public List<Ped> SpawnPeds(string model, int number = 1, float around1 = 3f, float around2 = 5f)
        {
            var list = new List<Ped>();
            for (var l = 1; l < number; l++)
                list.Add(new Ped(model, SpawnPoint.Around(around1, around2), 0f));
            $"Total peds created: {list.Count}".AddLog();
            return list;
        }

        public void CreateCopsOnScene(bool kill = false, bool isBusy = false)
        {
            $"Creating cops kill: {kill} busy: {isBusy}".AddLog();
            var array = GetPeds();
            var position = SpawnPoint.Around(3f, 5f);

            for (var i = 0; i < Fiskey111Common.Rand.RandomNumber(2); i++)
            {
                var cop = new Ped(new Model(array[Fiskey111Common.Rand.RandomNumber(array.Length)]), position, 0f);
                Functions.SetCopAsBusy(cop, isBusy);
                if (kill) cop.Kill();
                CopPedList.Add(cop);
            }
        }

        public void CreatePursuit(List<Ped> peds)
        {
            $"Creating a pursuit with {peds.Count} peds".AddLog();
            IsPursuit = true;
            PursuitHandler = Functions.CreatePursuit();

            foreach (var ped in peds)
                Functions.AddPedToPursuit(PursuitHandler, ped);

            Functions.SetPursuitIsActiveForPlayer(PursuitHandler, true);
            Functions.SetPursuitCopsCanJoin(PursuitHandler, true);
        }

        public bool IsPursuitCompleted() => IsPursuit && Functions.IsPursuitStillRunning(PursuitHandler);

        public string[] GetPeds()
        {
            var zone = Functions.GetZoneAtPosition(SpawnPoint);
            $"Getting peds at zone {zone.GameName}".AddLog();
            switch (zone.County)
            {
                case EWorldZoneCounty.LosSantos:
                    var lossantos = new[]
                    {
                        "s_m_y_cop_01",
                        "s_f_y_cop_01"
                    };
                    return lossantos;
                default:
                    var county = new[]
                    {
                        "s_m_y_sheriff_01",
                        "s_f_y_sheriff_01"
                    };
                    return county;
            }
        }

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
            $"Displaying end info".AddLog();
            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 CODE_04_PATROL");
            CalloutName.DisplayNotification("~g~Code 4~w~, good work officer!");
        }
    }
}
