using Rage;
using Rage.Native;

namespace Secondary_Callouts
{
    /// <summary>
    /// Shoutout to khorio for the colors!
    /// Created from AlconH's topic: http://www.lcpdfr.com/forums/topic/60739-calling-all-devs-standardisation-of-api-plugin-colors/
    /// </summary>
    public static class CalloutStandardization
    {
        /// <summary>
        /// Set the color of the blip
        /// </summary>
        /// <param name="blip">The blip to set the color of</param>
        /// <param name="type">The color ((Blips default to yellow))</param>
        public static void SetStandardColor(this Blip blip, BlipTypes type)
        {
            if (type == BlipTypes.Standard) return;
            if (blip) NativeFunction.Natives.SET_BLIP_COLOUR(blip, (int)type);
        }

        /// <summary>
        /// Description
        /// <para>Enemy = Enemies  [red]</para>
        /// <para>Officers = Cops/Detectives/Commanders  [blue] (not gross system blue)</para>
        /// <para>Support = EMS/Coroner/ETC  [green]</para>
        /// <para>Civilians = Bystanders/Witnesses/broken down/etc  [orange]</para>
        ///  <para>Other = Animals/Obstacles/Rocks/etc  [purple]</para>
        /// </summary>
        public enum BlipTypes { Enemy = 1, Officers = 3, Support = 2, AreaSearch = 5, Civilians = 17, Other = 19, Standard = 9999 }

        public static void SetBlipScalePed(this Blip blip)
        {
            if (blip) blip.Scale = 0.75f;
        }


        public static Blip CreateStandardizedBlip(Vector3 spawn, BlipTypes type = BlipTypes.Enemy, BlipScale scale = BlipScale.Normal)
        {
            Blip blip;

            switch (scale)
            {
                case BlipScale.Ped:
                    blip = new Blip(spawn, 0.75f);
                    blip.SetStandardColor(type);
                    break;
                case BlipScale.SearchArea:
                    blip = new Blip(spawn, 60f);
                    blip.SetStandardColor(type);
                    break;
                default:
                    blip = new Blip(spawn);
                    blip.SetStandardColor(type);
                    break;
            }

            return blip;
        }


        public static Blip CreateStandardizedBlip(Entity entity, BlipTypes type = BlipTypes.Enemy)
        {
            var blip = new Blip(entity);
            blip.SetStandardColor(type);

            return blip;
        }

        public enum BlipScale { Ped, Normal, SearchArea }
    }


    // EXAMPLE USAGE

    public class MyCallout
    {
        public void Foo()
        {
            // Create our blip
            var blip = new Blip(Vector3.Zero);

            // It's for an enemy, so we set the color to the enemy color
            blip.SetStandardColor(CalloutStandardization.BlipTypes.Enemy);

            // It's a ped, so let's set the scale to the correct scale for peds
            blip.SetBlipScalePed();
        }
    }
}
