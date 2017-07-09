using System;
using System.Diagnostics;
using LSPD_First_Response.Mod.API;
using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Startup
{
    public class StartDuty
    {
        public static void StartDutyMethods()
        {
            try
            {
                DisplayLoadingMessage();

//                if (!RageProRegistration.RegisterRagePro())
//                {
//                    Game.DisplayNotification(
//                        "Access to Secondary Callouts denied.\nIf you believe this was in error, please contact Fiskey111.");
//                }
                
                CalloutLoader.RegisterCallouts();
                
                AmbientHandler.StartAmbientEvents();
                
                if (Settings.StartingAudio) Functions.PlayScannerAudio($"ATTN_UNIT {Settings.UnitCallsign} BEGIN_BEAT");

                Detective.DetectiveMenu.Main();

                DisplayLoadedMessage();

                Game.AddConsoleCommands();
            }
            catch (Exception e)
            {
                e.ToString().AddLog(true);
                throw;
            }
        }

        private static void DisplayLoadingMessage()
        {
            "Starting to load SECONDARY CALLOUTS...".AddLog(true);
            " ".AddLog(true); 
            " ".AddLog(true);
        }

        private static void DisplayLoadedMessage()
        {
            var version = FileVersionInfo.GetVersionInfo(@"Plugins\LSPDFR\Secondary Callouts.dll").FileVersion;

            "SECONDARY CALLOUTS has been loaded successfully!".AddLog(true);
            " ".AddLog(true);
            $"Thank you for downloading version {version}! I hope you enjoy it!".AddLog(true);
            "  -Fiskey111".AddLog(true);
        }
    }
}
