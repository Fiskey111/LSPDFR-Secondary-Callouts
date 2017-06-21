using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Secondary_Callouts;
using Secondary_Callouts.API;
using Secondary_Callouts.BaseClass;
using Secondary_Callouts.Detective;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace SecondaryCallouts
{
    public abstract class BaseCallout : Callout
    {
        public Vector3 SpawnPoint;
        public EState CalloutEState;

        public string StartScannerAudio = "";
        public string AcceptScannerAudio = "";
        public string CalloutName = "Officer in Need of Assistance";
        public string ResponseInfo = "";
        public string ShotsFiredScannerAudio = "ATTN_DISPATCH CRIME_GUNFIRE_03 DEADLY_FORCE UNITS_RESPOND_CODE_99";

        public bool FalseCall = false;
        public bool IsFalseCall => _isFalseCall;
        public bool SpawnBlip = true;
        public bool StartedWeaponFireCheck = false;

        public ComputerPlusAPI.ResponseType ComputerPlus_ResponseType = ComputerPlusAPI.ResponseType.Code_3;
        public string ComputerPlus_CallMsg = "Respond to the call";
        public bool ComputerPlus_Active => _computerPlus;
        public Guid ComputerPlus_GUID => _callId;

        public List<Ped> PedList = new List<Ped>();
        public List<Ped> CopPedList = new List<Ped>();
        public List<Blip> BlipList = new List<Blip>();
        public List<Vehicle> CopVehList = new List<Vehicle>();

        public Blip AreaBlip;
        public CalloutStandardization.BlipTypes BlipType = CalloutStandardization.BlipTypes.Support;
        public CalloutStandardization.BlipScale BlipScale = CalloutStandardization.BlipScale.SearchArea;
        public float BlipAlpha = 1.0f;

        public LHandle PursuitHandler;
        public bool IsPursuit;

        public int AudioTime = 6000;

        public List<PedType> FinalPedList = new List<PedType>();

        private bool _computerPlus, _isFalseCall;
        private Guid _callId;
        private Stopwatch _sw = new Stopwatch();
        private bool _isAudioCompleted = false;
        
        public override bool OnBeforeCalloutDisplayed()
        {
            $"Starting callout".AddLog();
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around2D(250f, 350f));

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
            "Callouit has been accepted".AddLog();
            if (_computerPlus) ComputerPlusAPI.SetCalloutStatusToUnitResponding(_callId);

            if (!string.IsNullOrWhiteSpace(AcceptScannerAudio)) StartSecondaryAudio();

//            if (FalseCall && Fiskey111Common.Rand.RandomNumber(1, 15) == 1)
//            {
//                "False Call".AddLog();
//                _isFalseCall = true;
//                FalseCallHandler.callState = FalseCallHandler.CallState.Start;
//            }

            //CalloutName.DisplayNotification(ResponseInfo);

            var position = SpawnPoint.Around2D(5f, 20f);

            if (!SpawnBlip) return base.OnCalloutAccepted();

            AreaBlip = CalloutStandardization.CreateStandardizedBlip(position, BlipType, BlipScale);
            AreaBlip.Alpha = BlipAlpha;
            AreaBlip.EnableRoute(AreaBlip.Color);

            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
            if (_computerPlus) ComputerPlusAPI.AssignCallToAIUnit(_callId);
            if (Settings.AiAudio) Functions.PlayScannerAudio("OFFICER_INTRO_01 UNIT_RESPONDING_DISPATCH_04");
        }

        public override void Process()
        {
            base.Process();

            if (Game.IsKeyDown(Keys.End))
            {
                "Forcing callout end".AddLog(true);

                this.CalloutFinished();
            }

            if (!_isFalseCall) return;
            GameFiber.Sleep(0500);
            if (!FalseCallHandler.FalseCall(SpawnPoint, CalloutName)) return;
            CalloutFinished();
        }

        public void CalloutFinished()
        {
            "CalloutFinished()".AddLog();
            if (_computerPlus) ComputerPlusAPI.ConcludeCallout(_callId);
            DisplayEndInformation();
            "Deleting blips".AddLog();
            foreach (var blip in BlipList)
                if (blip) blip.Delete();
            if (AreaBlip.Exists()) AreaBlip.Delete();

            "Dismissing peds".AddLog();
            PedList.Dismiss();
            CopVehList.Dismiss();
            CopPedList.Dismiss();
            "End".AddLog();
            Game.DisplayHelp("To request ~b~detectives~w~, press ~y~Y~w~ in the next five seconds\nOtherwise, ignore this message", 5100);
            var sw = new Stopwatch();
            sw.Start();

            GameFiber.StartNew(delegate
            {
                while (sw.Elapsed.Seconds < 5)
                {
                    GameFiber.Yield();

                    if (!Game.IsKeyDown(Keys.Y)) continue;
                    Game.LogTrivial("Key pressed");
                    RequestDetectives();
                    break;
                }
                Game.LogTrivial("Broken");
            });

            this.End();
        }

        private void DisplayEndInformation()
        {
            "Displaying end info".AddLog();
            CalloutName.DisplayNotification("Callout Overview: " +
                                            $"\n<font size=\"9\">~r~Suspects~w~: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Suspect)}" +
                                            $" Deceased: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Suspect && p.Pedestrian && p.Pedestrian.IsDead)}" +
                                            $" Arrested: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Suspect && p.Pedestrian && p.Arrested)}" +
                                            $" Escaped: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Suspect && p.Pedestrian && p.Escaped)}" +
                                            $"\n~o~Victims~w~: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Victim)}" +
                                            $" Deceased: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Victim && p.Pedestrian && p.Pedestrian.IsDead)}" +
                                            $"\n~b~Officers~w~: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Cop)}" +
                                            $" Deceased: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Cop && p.Pedestrian && p.Pedestrian.IsDead)}" +
                                            $"\n~g~Services~w~: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Service)}" +
                                            $" Deceased: {FinalPedList.Count(p => p.PedestrianType == PedType.Type.Service && p.Pedestrian && p.Pedestrian.IsDead)}<\font>");

            Functions.PlayScannerAudio("ATTENTION_ALL_UNITS_01 CODE_04_PATROL");
            CalloutName.DisplayNotification($"~g~Code 4~w~, good work officer!");
        }


        // SUPPORTING METHODS

        public void GiveBlipInfo(CalloutStandardization.BlipTypes blipType, float alpha = 1.0f, CalloutStandardization.BlipScale blipScale = CalloutStandardization.BlipScale.SearchArea)
        {
            BlipType = blipType;
            BlipScale = blipScale;
            BlipAlpha = alpha;
        }

        public List<Ped> SpawnPeds(int number = 1, float around1 = 5f, float around2 = 9f)
        {
            var list = SpawnMethods.SpawnPeds(SpawnPoint, null, number, around1, around2, 0f);

            $"Total peds created: {list.Count}".AddLog();
            return list;
        }

        public List<Ped> SpawnPeds(string model, int number = 1, float around1 = 5f, float around2 = 9f, float heading = 0f)
        {
            var list = SpawnMethods.SpawnPeds(SpawnPoint, model, number, around1, around2, heading);

            $"Total peds created: {list.Count}".AddLog();
            return list;
        }

        public void CreateCopsOnScene(bool withVehs, bool kill = false, bool isBusy = false, int number = 0)
        {
            var position = SpawnPoint.Around2D(1f, 3f);

            if (number == 0) number = Fiskey111Common.Rand.RandomNumber(1, 3);

            CopPedList = SpawnMethods.SpawnCops(position, number, isBusy, kill, out List<PedType> outList);

            FinalPedList.AddRange(outList);

            var vehs = GetVehs();
            if (withVehs)
            {
                var veh = new Vehicle(new Model(vehs[Fiskey111Common.Rand.RandomNumber(vehs.Length)]), SpawnPoint);
                if (Fiskey111Common.Rand.RandomNumber(2) == 1) veh.IsSirenOn = true;
                CopVehList.Add(veh);
            }
            $"Cops created: {CopPedList.Count}".AddLog();
        }

        public void CreatePursuit(List<Ped> peds)
        {
            $"Creating a pursuit with {peds.Count} peds".AddLog();
            IsPursuit = true;
            PursuitHandler = Functions.CreatePursuit();

            foreach (var ped in peds)
            {
                if (!ped) continue;
                Functions.AddPedToPursuit(PursuitHandler, ped);
            }

            Functions.SetPursuitIsActiveForPlayer(PursuitHandler, true);
            Functions.SetPursuitCopsCanJoin(PursuitHandler, true);
            Functions.SetPursuitDisableAI(PursuitHandler, false);

            $"Peds added to pursuit: {Functions.GetPursuitPeds(PursuitHandler).Length}".AddLog();
        }

        public void GiveWeaponOrArmor(Ped ped, bool forceWeapons = false)
        {
            if (!ped) return;

            var random = forceWeapons
                ? Fiskey111Common.Rand.RandomNumber(1, 6)
                : Fiskey111Common.Rand.RandomNumber(1, Settings.GunFireChance);

            WeaponAsset weapon;
            switch (random)
            {
                case 1:
                    weapon = new WeaponAsset((uint) WeaponHash.Bat);
                    break;
                case 2:
                    weapon = new WeaponAsset((uint)WeaponHash.Pistol);
                    break;
                case 3:
                    weapon = new WeaponAsset((uint)WeaponHash.PumpShotgun);
                    break;
                case 4:
                    weapon = new WeaponAsset((uint)WeaponHash.AssaultRifle);
                    break;
                case 5:
                    ped.Armor = Fiskey111Common.Rand.RandomNumber(25, 101);
                    break;
            }
            if (random > 4) return;
            var gun = new Weapon(weapon, SpawnPoint, 400);
            gun.GiveTo(ped);
        }

        public void GiveWeaponOrArmor(List<Ped> pedList)
        {
            if (pedList.Count < 1) return;

            foreach (var ped in pedList)
            {
                if (!ped) continue;
                GiveWeaponOrArmor(ped);
            }
        }

        public void GiveFightTasks(IEnumerable<Ped> pedList, float radius = 40f)
        {
            foreach (var perp in pedList)
            {
                if (!perp) continue;

                perp.KeepTasks = true;
                perp.BlockPermanentEvents = true;
                perp.Tasks.FightAgainstClosestHatedTarget(radius);
            }
        }

        public bool IsPursuitCompleted() => IsPursuit && Functions.IsPursuitStillRunning(PursuitHandler);
        public bool IsPedInPursuit(Ped ped) => IsPursuit && Functions.GetPursuitPeds(PursuitHandler).Any(p => p == ped);

        public List<Ped> SuspectPositionCheck(List<Ped> pedList)
        {
            var list = pedList.ToList();
            
            if (list.Count < 1) return list;

            for (var index = list.Count - 1; index >= 0; index--)
            {
                var ped = list[index];
                if (!ped) continue;
                if (IsPursuit && IsPedInPursuit(ped)) continue;
                if (ped.Position.DistanceTo(Game.LocalPlayer.Character) < 60f || Functions.IsPedGettingArrested(ped)) continue;
                $"Ped considered escaped because distance = {Vector3.Distance(ped.Position, Game.LocalPlayer.Character)}".AddLog();
                CalloutName.DisplayNotification("~r~Suspect escaped~w~");
                if (FinalPedList.Any(p => p.Pedestrian == ped)) FinalPedList.First(p => p.Pedestrian == ped).Escaped = true;
                list.Remove(ped);
            }
            return list;
        }

        public void StartWeaponFireCheck(List<Ped> pedList)
        {
            if (pedList.Count < 1) return;

            StartedWeaponFireCheck = true;

            GameFiber.StartNew(delegate
            {
                bool hasAudioStarted = false;
                while (!hasAudioStarted)
                {
                    GameFiber.Yield();
                    if (!pedList.Any(p => p && p.IsShooting)) continue;

                    if (Fiskey111Common.Rand.RandomNumber(3) == 1)
                    {
                        while (!_isAudioCompleted) GameFiber.Yield();
                        Functions.PlayScannerAudio(ShotsFiredScannerAudio);
                    }
                    hasAudioStarted = true;
                }
            });
        }

        public string[] GetVehs()
        {
            var zone = Functions.GetZoneAtPosition(SpawnPoint);
            $"Getting vehs at zone {zone.GameName}".AddLog();
            switch (zone.County)
            {
                case EWorldZoneCounty.LosSantos:
                    var lossantos = new[]
                    {
                        "POLICE",
                        "POLICE2",
                        "POLICE3",
                        "POLICE4",
                    };
                    return lossantos;
                default:
                    var county = new[]
                    {
                        "SHERIFF",
                        "SHERIFF2"
                    };
                    return county;
            }
        }

        public void SendBackup(Vector3 sp, EBackupResponseType responseType = EBackupResponseType.Code3, EBackupUnitType backupType = EBackupUnitType.LocalUnit, bool random = false)
        {
            var backupTypes = new[]
            {
                EBackupUnitType.AirUnit, EBackupUnitType.LocalUnit, EBackupUnitType.NooseTeam,
                EBackupUnitType.StateUnit, EBackupUnitType.SwatTeam
            };

            var backup = backupType;
            if (random) backup = (EBackupUnitType)backupTypes.GetValue(Fiskey111Common.Rand.RandomNumber(backupTypes.Length));

            Functions.RequestBackup(sp, EBackupResponseType.Code3, backup);
        }          

        public float PlayerDistanceFromSpawnPoint => Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint);

        public float PlayerDistanceFromPosition(Vector3 pos) => Game.LocalPlayer.Character.Position.DistanceTo(pos);

        public void DisplayAdditionalInformation(string text) => CalloutName.DisplayNotification(text);

        private void StartSecondaryAudio()
        {
            _sw.Stop();
            var timeelapsed = Convert.ToInt32(_sw.Elapsed.TotalSeconds);
            $"Elapsed time: {timeelapsed}s".AddLog();
            GameFiber.StartNew(delegate
            {
                GameFiber.Sleep(AudioTime - timeelapsed);
                Functions.PlayScannerAudio(AcceptScannerAudio);
                GameFiber.Sleep(8000);
                "Audio completed".AddLog();
                _isAudioCompleted = true;
            });
        }

        public void AddPedList(List<Ped> pedList, PedType.Type type)
        {
            foreach (var ped in pedList)
            {
                if (!ped) continue;
                FinalPedList.Add(new PedType(ped, type));
            }
        }

        public void AddPedListWeapons(List<Ped> pedList, PedType.Type type)
        {
            foreach (var ped in pedList)
            {
                if (!ped) continue;
                FinalPedList.Add(new PedType(ped, type, ped.Inventory.EquippedWeaponObject));
            }
        }

        public void AddPedType(Ped ped, PedType.Type type)
        {
            if (!ped) return;
            FinalPedList.Add(new PedType(ped, type));
        }

        public void AddPedType(Ped ped, PedType.Type type, Weapon weapon)
        {
            if (!ped) return;
            FinalPedList.Add(new PedType(ped, type, weapon));
        }

        public bool PedCheck(List<Ped> peds)
        {
            if (peds.Count < 1) return false;

            foreach (var ped in peds)
            {
                if (!ped) return false;
                if (Functions.IsPedArrested(ped) || ped.IsDead || FinalPedList.Any(p => p.Pedestrian && p.Pedestrian == ped && p.Escaped)) continue;

                return false;
            }
            return true;
        }

        public void SetRelationshipGroups(IEnumerable<Ped> pedList, string relGroup)
        {
            foreach (var ped in pedList)
            {
                if (!ped) continue;

                ped.RelationshipGroup = relGroup;
            }
        }

        public void SetRelationshipsHate(IEnumerable<Ped> pedList1, IEnumerable<Ped> pedList2, Relationship groupRelationship = Relationship.Hate) => Game.SetRelationshipBetweenRelationshipGroups(pedList1.FirstOrDefault().RelationshipGroup, pedList2.FirstOrDefault().RelationshipGroup, groupRelationship);
        public void SetPlayerRelationships(IEnumerable<Ped> pedList2, Relationship groupRelationship = Relationship.Hate) => Game.SetRelationshipBetweenRelationshipGroups(Game.LocalPlayer.Character.RelationshipGroup, pedList2.FirstOrDefault().RelationshipGroup, groupRelationship);


        public void GiveCourtCase(List<Ped> pedList, string crimes)
        {
            if (!PluginCheck.IsLSPDFRPlusRunning()) return;

            LSPDFRPlusAPI.AddCourtCase(pedList, crimes);
        }

        public void RequestDetectives()
        {
            Game.LogTrivial("Requesting detectives");
            var detective = new Detective(SpawnPoint, DetectiveQuestions.GetOptions());
            detective.Dispatch();
        }

        public enum EState { Accepted, EnRoute, DecisionMade, OnScene, Checking }
    }

    internal static class DetectiveQuestions
    {
        internal static MultipleOptionLine[] GetOptions(int suspects = 20, int numberDead = 20)
        {
            return new[]
            {
                HowManySuspects(suspects),
                HowManyDead(numberDead),
                WhyDidYouArrive(),
                HowDidThesePeopleDie()
            };
        }

        internal static MultipleOptionLine HowManySuspects(int numberOfSuspects)
        {
            var array = new List<Option>();
            for (var i = 0; i < numberOfSuspects; i++)
            {
                array.Add(new Option($"{i} suspects", "[~b~Detective~w~]: Alright, thank you.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: There were {i} suspects."));
            }

            return new MultipleOptionLine("[~b~Detective~w~]: How many suspects were there?", array);
        }

        internal static MultipleOptionLine HowManyDead(int numberDead)
        {
            var array = new List<Option>();
            for (var i = 0; i < numberDead; i++)
            {
                array.Add(new Option($"{i} dead", $"[~b~Detective~w~]: {i} died, good to know.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: A total of {i} died."));
            }

            return new MultipleOptionLine("[~b~Detective~w~]: How many individuals are dead?", array);
        }

        internal static MultipleOptionLine WhyDidYouArrive()
        {
            var array = new List<Option>
            {
                new Option($"Callout", "[~b~Detective~w~]: Sounds good; I'll confirm that with dispatch.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: I arrived because I was dispatched here"),
                new Option($"Patrolling", "[~b~Detective~w~]: Okay, well I'll check your GPS to confirm that.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: I was patrolling in the area nearby"),
                new Option($"No reason", "[~b~Detective~w~]: That's interesting, we'll talk more at the station", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: I don't really have a reason, I just arrived")
            };
            
            return new MultipleOptionLine("[~b~Detective~w~]: What made you arrive on scene?", array);
        }

        internal static MultipleOptionLine HowDidThesePeopleDie()
        {
            var array = new List<Option>
            {
                new Option($"Nobody died", "[~b~Detective~w~]: Phew, thank goodness.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: Nobody is dead at the scene"),
                new Option($"Gunshot wounds", "[~b~Detective~w~]: Alright, good to know", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: They died from gunshot wounds"),
                new Option($"Physical altercation", "[~b~Detective~w~]: Interesting to know", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: They died from physical trauma like being beaten"),
                new Option($"Taser", "[~b~Detective~w~]: Did you have your taser set to over 9000? Jeez.", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: They died from taser deployment"),
                new Option($"Other", "[~b~Detective~w~]: Okay, that's interesting...", $"[~y~{Fiskey111Common.OfficerSettings.OfficerName()}~w~]: They died from another method")
            };

            return new MultipleOptionLine("[~b~Detective~w~]: How did these people die?", array);
        }
    }
}
