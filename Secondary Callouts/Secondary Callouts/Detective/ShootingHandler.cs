using System.Diagnostics;
using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Detective
{
    public class ShootingHandler
    {
        private static GameFiber _fiber;
        private static Stopwatch _sw;

        public static bool DidFireWeaponInLast30Seconds { get; private set; }

        public static void StartShootingHandler()
        {
            _fiber = new GameFiber(StartFiber);
            _fiber.Start();
        }

        private static void StartFiber() 
        {
            _sw = new Stopwatch();
            _sw.Start();
            while (true)
            {
                GameFiber.Yield();

                if (_sw.Elapsed.Seconds > 30)
                {
                    _sw.Restart();
                    DidFireWeaponInLast30Seconds = false; 
                }

                if (!Game.LocalPlayer.Character.IsShooting) continue;

                "Player is shooting".AddLog();
                DidFireWeaponInLast30Seconds = true;
                _sw.Restart();
            }
        }
    }
}
