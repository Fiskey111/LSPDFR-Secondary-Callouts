using Rage;

namespace Secondary_Callouts.ExtensionMethods
{
    public static class Notifications
    {
        public static void DisplayNotification(this string subtitle, string body) => Game.DisplayNotification("CHAR_CALL911", "CHAR_CALL911", "Dispatch:", subtitle, body);
    }
}
