using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts
{
    internal class FalseCallHandler
    {
        public static CallState callState;
        private static bool _wait;
        private static Ped _p;

        public static bool FalseCall(Vector3 position, string calloutName)
        {
            if (Vector3.Distance(Game.LocalPlayer.Character.Position, position) < 30f &&  callState == CallState.Start)
            {
                callState = CallState.Middle;
                calloutName.DisplayNotification("Caller hung up, investigate the area");
            }
            if (Vector3.Distance(Game.LocalPlayer.Character.Position, position) < 10f && callState == CallState.Middle)
            {
                callState = CallState.Wait;
                GameFiber.StartNew(delegate
                {
                    GameFiber.Sleep(Fiskey111Common.Rand.RandomNumber(9000));
                    GameFiber.Sleep(Fiskey111Common.Rand.RandomNumber(4000, 9000));
                    GameFiber.Sleep(Fiskey111Common.Rand.RandomNumber(4000, 9000));
                    GameFiber.Sleep(Fiskey111Common.Rand.RandomNumber(4000, 9000));
                    calloutName.DisplayNotification("Update: Call possibly false\nInvestigate the area");
                });
            }
            if (callState == CallState.Wait && !_wait)
            {
                _wait = true;
                if (Fiskey111Common.Rand.RandomNumber(1, 10) == 1)
                {
                    _p = new Ped(position);
                    if (Fiskey111Common.Rand.RandomNumber(1, 4) == 1)
                    {
                        Weapon g = new Weapon("WEAPON_PISTOL", position, 100);
                        g.GiveTo(_p);
                    }
                    _p.KeepTasks = true;
                    _p.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                    callState = CallState.End;
                }
                else
                {
                    GameFiber.Sleep(Fiskey111Common.Rand.RandomNumber(3000, 9000));
                    calloutName.DisplayNotification("Call determined to be a false report");
                    return true;
                }
            }
            if (callState == CallState.End && (_p.IsDead || Functions.IsPedArrested(_p)))
                return true;

            GameFiber.Yield();
            return false;
        }

        public enum CallState { Start, Middle, Wait, End }
    }
}
