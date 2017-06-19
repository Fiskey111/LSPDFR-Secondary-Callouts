using LSPD_First_Response.Mod.API;
using Rage;

namespace Secondary_Callouts.Objects
{
    public class PedType
    {
        public Ped Pedestrian { get; }
        public Type PedestrianType { get; }
        public bool Escaped { get; set; }
        public Weapon WeaponGiven { get; private set; }
        public bool Arrested => Functions.IsPedArrested(Pedestrian);

        public PedType(Ped ped, Type type)
        {
            Pedestrian = ped;
            PedestrianType = type;
        }

        public PedType(Ped ped, Type type, Weapon weapon)
        {
            Pedestrian = ped;
            PedestrianType = type;
            WeaponGiven = weapon;
        }

        public void GiveWeapon(Weapon weapon) => WeaponGiven = weapon;

        public enum Type { Cop, Victim, Suspect, Service }
    }
}
