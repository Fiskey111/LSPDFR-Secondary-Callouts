using Rage;

namespace Secondary_Callouts.ExtensionMethods
{
    public static class Notifications
    {
        public static void DisplayNotification(this string subtitle, string body) => Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Dispatch:", subtitle, body);
    }
}
