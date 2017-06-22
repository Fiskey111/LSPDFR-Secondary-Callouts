using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Attributes;
using Rage.ConsoleCommands;

namespace Secondary_Callouts
{
    public static class Commands
    {
        [ConsoleCommand]
        public static void Command_ForceCallout()
        {
            Game.Console.Print("Forcing test callout");
            Functions.StartCallout("TestCallout");
        }
    }
}
