using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.BaseClass
{
    internal class EntityMethods
    {
        internal static void CheckIfOnVehicle(Ped ped)
        {
            if (!ped) return;
            var groundPos = World.GetGroundZ(ped.Position, false, true);
            while (!groundPos.HasValue)
                GameFiber.Yield();

            var dist = Vector3.Distance(new Vector3(ped.Position.X, ped.Position.Y, groundPos.Value), ped.Position);
            $"Distance from ground position for ped: {dist}".AddLog();
            if (dist < 0.5f) return;

            ped.Position = ped.RightPosition;
            ped.Position = ped.RightPosition;
            ped.Position = new Vector3(ped.Position.X, ped.Position.Y, groundPos.Value);
        }
    }
}
