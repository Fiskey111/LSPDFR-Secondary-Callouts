using System;
using LSPD_First_Response.Mod.API;
using Secondary_Callouts.Callouts;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Startup
{
    internal class CalloutLoader
    {
        private static int _calloutNumber;

        internal static void RegisterCallouts()
        {
            "Registering callouts...".AddLog(true);
            RegisterCallout(typeof(Fight));
            RegisterCallout(typeof(EMSAssistance));
            RegisterCallout(typeof(FootPursuit));
            RegisterCallout(typeof(GangAttack));
            RegisterCallout(typeof(HeavilyArmed));
            RegisterCallout(typeof(KnifeAttack));
            RegisterCallout(typeof(ShotsOnOfficer));
            //RegisterCallout(typeof(OfficerShot));

#if DEBUG
            RegisterCallout(typeof(DEBUG.TestCallout));
#endif
            $"{_calloutNumber} high-quality callouts loaded!".AddLog(true);
        }

        private static void RegisterCallout(Type type)
        {
            $"Registering callout {nameof(type)}".AddLog();
            Functions.RegisterCallout(type);
            _calloutNumber++;
        }
    }
}
