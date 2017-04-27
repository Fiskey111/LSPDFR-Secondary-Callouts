using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.Startup;

namespace Secondary_Callouts
{
    public class Main : Plugin
    {
        public Main() { }

        public override void Finally() { }

        public override void Initialize() => Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

        private void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            var fiber = new GameFiber(StartDuty.StartDutyMethods);
            fiber.Start();
        }
    }
}
