using LSPD_First_Response.Mod.API;
using System;
using System.Linq;

namespace Secondary_Callouts.API
{
    internal class PluginCheck
    {
        internal static bool IsComputerPlusRunning() => IsLspdfrPluginRunning("ComputerPlus", new Version("1.3.5.0"));
        internal static bool IsBetterEMSRunning() => IsLspdfrPluginRunning("BetterEMS", new Version("3.0.6298.2858"));

        private static bool IsLspdfrPluginRunning(string plugin, Version minversion = null) => Functions.GetAllUserPlugins().Select(assembly => assembly.GetName()).Where(an => an.Name.ToLower() == plugin.ToLower()).Any(an => minversion == null || an.Version.CompareTo(minversion) >= 0);
    }
}
