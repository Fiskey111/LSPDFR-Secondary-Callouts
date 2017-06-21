using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LtFlash.Common.Processes;
using Rage;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.Detective
{
    public class Detective
    {
        public Ped DetectivePed;
        public Vehicle DetectiveVeh;
        public Vector3 TargetPosition;
        public MultipleOptionLine[] QuestionList;
        public Blip DetectiveBlip;
        public bool IsRunning;
        public MultipleOptionConversation Conversation;

        private ProcessHost _processHost = new ProcessHost();
        private ScenarioHelper _scenario;

        public Detective(Vector3 position, IEnumerable<MultipleOptionLine> options)
        {
            TargetPosition = World.GetNextPositionOnStreet(position.Around2D(2f));
            var sp = World.GetNextPositionOnStreet(TargetPosition.Around2D(100f, 200f));
            DetectiveVeh = new Vehicle(new Model("fbi"), sp);
            DetectiveVeh.IsPersistent = true;
            DetectivePed = new Ped(new Model(0xedbc7546), sp, 0f);
            DetectivePed.WarpIntoVehicle(DetectiveVeh, -1);
            DetectivePed.MakeMissionPed();
            DetectivePed.BlockPermanentEvents = true;
            DetectivePed.KeepTasks = true;
            QuestionList = options.ToArray();
            Conversation = new MultipleOptionConversation(QuestionList, DetectivePed);
        }

        public void Dispatch(float speed = 20f, VehicleDrivingFlags flags = VehicleDrivingFlags.Emergency)
        {
            DetectiveBlip = new Blip(DetectivePed);
            DetectiveBlip.SetStandardColor(CalloutStandardization.BlipTypes.Officers);
            DetectiveBlip.SetBlipScalePed();
            DetectivePed.Tasks.DriveToPosition(TargetPosition, speed, flags, 5f);
            IsRunning = true;
            "Detective Dispatched".DisplayNotification("A ~b~detective~w~ has been ~g~dispatched~w~ to your location");
            Game.DisplayHelp("Wait where you are until the ~b~detective~w~ arrives.");
            _processHost.Start();
            _processHost.ActivateProcess(DriveFiber);
        }


        private void DriveFiber()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (DetectivePed.DistanceTo(TargetPosition) > 5.5f)
            {
                GameFiber.Yield();
#if DEBUG
                Game.DisplaySubtitle(DetectivePed.DistanceTo(TargetPosition).ToString());
#endif
                if (sw.Elapsed.TotalSeconds > 45) WarpToPosition();
                GameFiber.Yield();
            }
            _processHost.SwapProcesses(DriveFiber, ExitVehicleAndGoToPos);
        }

        private void ExitVehicleAndGoToPos()
        {
            if (DetectiveBlip) DetectiveBlip.Delete();
            DetectivePed.Tasks.LeaveVehicle(DetectiveVeh, LeaveVehicleFlags.None);
            while (DetectivePed.IsGettingIntoVehicle)
                GameFiber.Yield();

            DetectivePed.Tasks.GoStraightToPosition(TargetPosition.Around2D(1f, 2f), 3f, 0f, 0f, 10000);
            while (DetectivePed.Position.DistanceTo(TargetPosition) > 3.5f)
                GameFiber.Yield();

            _processHost.SwapProcesses(ExitVehicleAndGoToPos, StartScenario);
        }

        private void StartScenario()
        {
            _scenario = new ScenarioHelper(DetectivePed, ScenarioHelper.Scenario.CODE_HUMAN_POLICE_INVESTIGATE);
            _scenario.StartLooped();
         
            GameFiber.Sleep(1500);
            var sw = new Stopwatch();
            sw.Start();
            var ran = Fiskey111Common.Rand.RandomNumber(10, 25);
            while (sw.Elapsed.Seconds < ran) GameFiber.Yield();
            _scenario.Stop();

            DetectivePed.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3f, 10f, 4f);
            while (DetectivePed.Position.DistanceTo(Game.LocalPlayer.Character) > 3.5f)
                GameFiber.Yield();
            
            "Scenario ended".AddLog();

            _processHost.SwapProcesses(StartScenario, AwaitPlayerTalk);
        }

        private void AwaitPlayerTalk()
        {
            Conversation.Start();
            
            _processHost.SwapProcesses(AwaitPlayerTalk, AwaitFinish);
        }

        private void AwaitFinish()
        {
            while (!Conversation.HasEnded)
                GameFiber.Yield();

            _scenario = new ScenarioHelper(DetectivePed, ScenarioHelper.Scenario.CODE_HUMAN_POLICE_INVESTIGATE);
            _scenario.StartLooped();

            while (Game.LocalPlayer.Character.Position.DistanceTo(DetectivePed) < 30f) GameFiber.Yield();
            
            this.End();
        }

        private void End()
        {
            _processHost.Stop();

            if (DetectivePed) DetectivePed.Dismiss();
            if (DetectiveVeh) DetectiveVeh.Dismiss();
            if (DetectiveBlip) DetectiveBlip.Delete();
        }

        private void WarpToPosition()
        {
            "Warping to position...".AddLog(true);
            DetectiveVeh.Position = TargetPosition;
            _processHost.SwapProcesses(DriveFiber, ExitVehicleAndGoToPos);
        }
    }
}
