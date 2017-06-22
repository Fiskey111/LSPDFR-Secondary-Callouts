using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using SecondaryCallouts;
using Secondary_Callouts.API;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;
using static Fiskey111Common.Rand;

namespace Secondary_Callouts.Callouts
{
    [CalloutInfo("Gang Attack", CalloutProbability.Medium)]
    public class GangAttack : BaseCallout
    {
        private List<Ped> _ballasList = new List<Ped>();
        private List<Ped> _lostList = new List<Ped>();
        private List<Vehicle> _vehList = new List<Vehicle>();

        private const string CallName = "Gang War in Progress";
        private const string LSPDFRMsg = "~b~Officers~w~ require assistance for ~y~shots fired";
        private const string CalloutMsg = "~r~Shots fired~w~ on ~b~officers~w~ by ~p~gang~w~ members\nRespond ~r~Code 3~w~";
        private const string CalloutResponseInfo = "~b~Officers~w~ in need of assistance with shots fired; respond ~r~Code 3~w~ and assist";
        private const string ComputerPlusUpdate =
            "Gang members firing shots.  Multiple suspects.";

        private string _startScanner =
            $"ATTN_UNIT_02 {Settings.UnitCallsign} CODE99_IMMEDIATE UNITS_REPORTING GANG_RELATED";
        private string _acceptAudio =
            $"OFFICER_INTRO_01 COPY_DISPATCH OUTRO_01 DISPATCH_INTRO_01 REPORT_RESPONSE_COPY_02 {Settings.UnitCallsign} RESPOND_CODE3 SHOTS_OFFICER_LETHAL_FORCE SUSPECTS_MEMBERS_OF THE_BALLAS AND THE_LOST";

        private string[] _ballasModelArray = new []
        {
            "g_f_y_ballas_01",
            "g_m_y_ballaeast_01",
            "g_m_y_ballaorig_01",
            "g_m_y_ballasout_01"
        };

        private string[] _lostModelArray = new[]
        {
            "g_f_y_lost_01",
            "g_m_y_lost_01",
            "g_m_y_lost_02",
            "g_m_y_lost_03"
        };

        private string[] _lostVehicles = new[]
        {
            "DAEMON",
            "GBURRITO"
        };

        private string[] _ballasVehicles = new[]
        {
            "BALLER",
            "CAVALCADE"
        };


        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = CallName;
            CalloutMessage = LSPDFRMsg;

            GiveBlipInfo(CalloutStandardization.BlipTypes.Officers, 0.75f);
            StartScannerAudio = _startScanner;

            ComputerPlus_CallMsg = $"Officers requires immediate backup for shots fired near {World.GetStreetName(SpawnPoint)}.\nGang activity presumed.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            AcceptScannerAudio = _acceptAudio;

            CreateCopsOnScene(true);

            var ballasSpawn = SpawnPoint.Around2D(6f);
            var lostSpawn = ballasSpawn.Around2D(18f);

            for (var l = 1; l < RandomNumber(5, 10); l++)
            {
                var model = _ballasModelArray[RandomNumber(_ballasModelArray.Length)];
                $"Creating ped model {model}".AddLog();
                _ballasList.Add(new Ped(model, ballasSpawn.Around2D(1f, 2f), 0f));
            }

            for (var l = 0; l < RandomNumber(2); l++)
            {
                _vehList.Add(new Vehicle(new Model(_ballasVehicles[RandomNumber(_ballasVehicles.Length)]), ballasSpawn));
                _vehList[l].PrimaryColor = Color.MediumPurple;
            }

            for (var l = 1; l < RandomNumber(5, 10); l++)
            {
                var model = _lostModelArray[RandomNumber(_lostModelArray.Length)];
                $"Creating ped model {model}".AddLog();
                _lostList.Add(new Ped(model, lostSpawn.Around2D(1f, 2f), 0f));
            }

            for (var l = 0; l < RandomNumber(2); l++)
            {

                _vehList.Add(new Vehicle(new Model(_lostVehicles[RandomNumber(_lostVehicles.Length)]), ballasSpawn));
            }

            DisplayAdditionalInformation(CalloutMsg);
            ResponseInfo = CalloutResponseInfo;

            GiveWeapons(_ballasList);
            GiveWeapons(_lostList);


            AddPedListWeapons(_ballasList, PedType.Type.Suspect);
            AddPedListWeapons(_lostList, PedType.Type.Suspect);

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

                    SetRelationshipGroups(_ballasList, "FiskeyBallas");
                    SetRelationshipGroups(_lostList, "FiskeyLost");

                    SetRelationshipsHate(_ballasList, _lostList);
                    SetRelationshipsHate(_lostList, _ballasList);

                    SetRelationshipsHate(CopPedList, _ballasList);
                    SetRelationshipsHate(CopPedList, _lostList);
                    SetPlayerRelationships(_ballasList);
                    SetPlayerRelationships(_lostList);

                    GiveFightTasks(_ballasList);
                    GiveFightTasks(_lostList);
                    GiveFightTasks(CopPedList);

                    "Swapping to OnScene".AddLog();

                    break;
                case EState.OnScene:
                    if (PlayerDistanceFromSpawnPoint > 45f) break;

                    if (RandomNumber(10) == 1) RequestBackup(RandomNumber(2) == 1 ? GangType.Ballas : GangType.Lost);

                    CalloutEState = EState.Checking;

                    break;
                case EState.Checking:
                    IsNearAnyPed(_ballasList, _lostList);
                    if (PedCheck(_ballasList) && PedCheck(_lostList))
                    {
                        CalloutFinished();
                        this.End();
                    }
                    break;
            }
        }
        
        private void GiveWeapons(IEnumerable<Ped> pedList)
        {
            var enumerable = pedList as Ped[] ?? pedList.ToArray();
            if (enumerable.Length < 1) return;

            foreach (var ped in enumerable)
            {
                switch (RandomNumber(1, 5))
                {
                    case 1:
                        var gun1 = new Weapon(new WeaponAsset((uint)WeaponHash.MicroSMG), SpawnPoint, 100);
                        gun1.GiveTo(ped);
                        break;
                    case 2:
                        var gun2 = new Weapon(new WeaponAsset((uint)WeaponHash.Pistol), SpawnPoint, 100);
                        gun2.GiveTo(ped);
                        break;
                    case 3:
                        var gun3 = new Weapon(new WeaponAsset((uint)WeaponHash.PumpShotgun), SpawnPoint, 100);
                        gun3.GiveTo(ped);
                        break;
                    default:
                        ped.Armor = 100;
                        var gun4 = new Weapon(new WeaponAsset((uint)WeaponHash.AssaultRifle), SpawnPoint, 100);
                        gun4.GiveTo(ped);
                        break;
                }
            }
        }

        private void RequestBackup(GangType type)
        {
            var array = type == GangType.Ballas ? _ballasModelArray : _lostModelArray;

            var sp = World.GetNextPositionOnStreet(SpawnPoint.Around2D(60f, 80f));
            var list = new List<Ped>();

            for (int i = 0; i < RandomNumber(1, 4); i++)
            {
                var ped = new Ped(array[RandomNumber(array.Length)], sp.Around2D(1f), 0f);

                if (type == GangType.Ballas) _ballasList.Add(ped);
                else _lostList.Add(ped);
                list.Add(ped);
            }

            GiveWeapons(list);

            AddPedListWeapons(list, PedType.Type.Suspect);

            var model = type == GangType.Ballas ? _ballasVehicles : _lostVehicles;
            var length = type == GangType.Ballas ? _ballasVehicles.Length : _lostVehicles.Length;

            _vehList.Add(new Vehicle(model[RandomNumber(length)], sp));

            var seat = -1;
            foreach (var p in list)
            {
                p.WarpIntoVehicle(_vehList.Last(), seat);
                seat++;
            }

            var driver = _vehList.Last().Driver;
            driver.KeepTasks = true;
            driver.Tasks.DriveToPosition(SpawnPoint, 60f, VehicleDrivingFlags.Emergency, 10f);
            var relGroup = type == GangType.Ballas
                ? _ballasList.FirstOrDefault().RelationshipGroup
                : _lostList.FirstOrDefault().RelationshipGroup;
            SetRelationshipGroups(list, relGroup.Name);

            $"Backup requested: {list.Count} peds, vehicle {_vehList.Last().Model.Name}".AddLog();
            CallName.DisplayNotification("Backup is arriving for one of the ~r~gangs~w~.\nRemain alert.");
        }

        private enum GangType { Ballas, Lost }
   }
}
