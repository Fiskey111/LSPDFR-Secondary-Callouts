using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.ExtensionMethods;
using Secondary_Callouts.Startup;

namespace Secondary_Callouts
{
    public class Main : Plugin
    {
        public Main() { }

        public override void Finally() { }

        public override void Initialize() => Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

        public void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                "Loading...".AddLog();
                var fiber = new GameFiber(StartDuty.StartDutyMethods);
                fiber.Start();
            }
        }
    }
}
