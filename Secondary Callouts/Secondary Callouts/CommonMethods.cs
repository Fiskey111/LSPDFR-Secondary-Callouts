using System.Windows.Forms;
using Rage;
using Rage.Native;

namespace Secondary_Callouts
{
    internal class CommonMethods
    {
/*        internal static void DisplayMenuHelp()
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
        }*/

        internal static bool IsOnScene(ISpatial target, ISpatial checkPed, float distance = 5f) => Vector3.Distance(target.Position, checkPed.Position) < distance;
        internal static bool IsOnScene(Vector3 target, ISpatial checkPed, float distance = 5f) => Vector3.Distance(target, checkPed.Position) < distance;
        internal static bool IsOnScene(Vector3 target, Vector3 check, float distance = 5f) => Vector3.Distance(target, check) < distance;


        internal static int DisplayNotification(string title, string subtitle, string text)
        {
            NativeFunction.Natives.x202709F4C58A0424("STRING");
            NativeFunction.Natives.x6C188BE134E074AA(text);
            NativeFunction.Natives.x2B7E9A4EAAA93C89("CHAR_CALL911", "CHAR_CALL911", false, 4, title, subtitle);
            return NativeFunction.Natives.x2ED7843F8F801023<int>(false, true);
        }
    }
}
