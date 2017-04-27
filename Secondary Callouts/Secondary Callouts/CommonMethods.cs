using System.Windows.Forms;
using Rage;

namespace Secondary_Callouts
{
    internal class CommonMethods
    {
        internal static void DisplayMenuHelp()
        {
            if (Fiskey111Common.OfficerSettings.MenuKeyModifier() == Keys.None)
                Game.DisplayHelp("To open the menu for this callout at any time, press ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKey());
            else
                Game.DisplayHelp("To open the menu for this callout at any time, press ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKeyModifier() + "~w~ + ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKey());
        }

        internal static void NotifyEndHelp()
        {
            if (Fiskey111Common.OfficerSettings.MenuKeyModifier() == Keys.None)
                Game.DisplayHelp("To end this callout at any time, press ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKey());
            else
                Game.DisplayHelp("To end this callout at any time, press ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKeyModifier() + "~w~ + ~y~" +
                                 Fiskey111Common.OfficerSettings.MenuKey());
        }

        internal static bool IsOnScene(ISpatial target, ISpatial checkPed, float distance = 5f) => Vector3.Distance(target.Position, checkPed.Position) < distance;
        internal static bool IsOnScene(Vector3 target, ISpatial checkPed, float distance = 5f) => Vector3.Distance(target, checkPed.Position) < distance;
        internal static bool IsOnScene(Vector3 target, Vector3 check, float distance = 5f) => Vector3.Distance(target, check) < distance;
    }
}
