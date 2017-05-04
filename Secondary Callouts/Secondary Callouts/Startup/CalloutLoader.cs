using System;
using LSPD_First_Response.Mod.API;
using Secondary_Callouts.Callouts;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Startup
{
    internal class CalloutLoader
    {
        private static int _calloutNumber = 0;

        internal static void RegisterCallouts()
        {
            "Registering callouts...".AddLog(true);
            RegisterCallout(typeof(Fight));
            RegisterCallout(typeof(EMSAssistance));

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
