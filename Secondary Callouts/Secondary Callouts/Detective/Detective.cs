using System.Diagnostics;
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
        //public DetectiveDialogue Dialogue;
        public Blip DetectiveBlip;
        public bool IsRunning;

        private ProcessHost _processHost = new ProcessHost();

        public Detective(Vector3 position)
        {
            TargetPosition = position;
            var sp = World.GetNextPositionOnStreet(TargetPosition.Around(100f, 200f));
            DetectiveVeh = new Vehicle(new Model("fbi"), sp);
            DetectivePed = new Ped(new Model(0xedbc7546), sp, 0f);
            DetectivePed.WarpIntoVehicle(DetectiveVeh, -1);
        }

        public void Dispatch(float speed = 40f, VehicleDrivingFlags flags = VehicleDrivingFlags.Emergency)
        {
            DetectivePed.Tasks.DriveToPosition(TargetPosition, speed, flags, 5f);
            IsRunning = true;
            _processHost.Start();
            _processHost.ActivateProcess(DriveFiber);
        }


        private void DriveFiber()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (DetectivePed.TravelDistanceTo(TargetPosition) > 5f)
                {
                    if (sw.Elapsed.TotalSeconds > 360) WarpToPosition();
                    GameFiber.Yield();
                    continue;
                }

                GameFiber.Sleep(0500);
                ExitVehicleAndGoToPos();

                GameFiber.Yield();
            }
        }

        private void ExitVehicleAndGoToPos()
        {
            DetectivePed.Tasks.LeaveVehicle(DetectiveVeh, LeaveVehicleFlags.None);
            while (DetectivePed.IsGettingIntoVehicle)
                GameFiber.Yield();
            DetectivePed.Tasks.FollowNavigationMeshToPosition(TargetPosition, 0f, 4f, 3f);
            while (DetectivePed.Position.DistanceTo(TargetPosition) > 3.5f)
                GameFiber.Yield();

            StartScenario();
        }

        private void StartScenario()
        {
            var scenario = new ScenarioHelper(DetectivePed, ScenarioHelper.Scenario.CODE_HUMAN_MEDIC_TIME_OF_DEATH);
            scenario.StartNonLooped();

            while (scenario.IsRunning) GameFiber.Yield();
            
            "Scenario ended".AddLog();

            AwaitPlayerTalk();
        }

        private void AwaitPlayerTalk()
        {
            while (Game.LocalPlayer.Character.Position.DistanceTo(DetectivePed) > 3f) GameFiber.Yield();

            // Start interrogation
        }

        private void WarpToPosition()
        {
            "Warping to position...".AddLog(true);
            DetectiveVeh.Position = TargetPosition;
            GameFiber.Sleep(0500);
            ExitVehicleAndGoToPos();
        }
    }
}
