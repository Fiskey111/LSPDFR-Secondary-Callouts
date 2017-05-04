using Rage;

namespace Secondary_Callouts.ExtensionMethods
{
    internal static class Logging
    {
        internal static void AddLog(this string log, bool release = false)
        {
#if DEBUG
            if (!release) Game.LogTrivial($"[SECONDARY CALLOUTS]: {log}");
#endif
            if (release) Game.LogTrivial($"[SECONDARY CALLOUTS]: {log}");
        }

    }
}
