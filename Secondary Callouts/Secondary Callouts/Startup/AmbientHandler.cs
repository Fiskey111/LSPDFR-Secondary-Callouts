using System.Collections.Generic;
using Rage;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Objects;

namespace Secondary_Callouts.Startup
{
    internal class AmbientHandler
    {
        private static GameFiber _ambientController;
        private static List<AmbientEvent> _ambientList;

        internal static void StartAmbientEvents()
        {
            "Starting to load ambient events...".AddLog(true);
            _ambientList = LoadAmbientEvents();
            _ambientController = new GameFiber(AmbientController);
            _ambientController.Start();
            $"{_ambientList.Count} ambient events loaded".AddLog(true);
        }

        private static List<AmbientEvent> LoadAmbientEvents()
        {
            var list = new List<AmbientEvent>();
            //list.Add();
            return list;
        }

        private static void AmbientController()
        {
            while (true)
            {
                GameFiber.Yield();
            }
        }
    }
}
