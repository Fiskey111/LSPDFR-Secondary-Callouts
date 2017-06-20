using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secondary_Callouts.Objects
{
    public class Animation
    {
        public string ShortName { get; }
        public string Dictionary { get; }
        public string AnimationName { get; }
        public float BlendInSpeed { get; } = 8f;
        public Rage.AnimationFlags Flags { get; } = Rage.AnimationFlags.None;
        public int AnimationTime { get; }

        public Animation(string shortName, string dict, string name, float blend, Rage.AnimationFlags flags)
        {
            ShortName = shortName;
            Dictionary = dict;
            AnimationName = name;
            BlendInSpeed = blend;
            Flags = flags;
        }

        public Animation(string shortName, string dict, string name, int time)
        {
            ShortName = shortName;
            Dictionary = dict;
            AnimationName = name;
            AnimationTime = time;
        }

        public void Play(Rage.Ped ped)
        {
            ped.Tasks.PlayAnimation(Dictionary, AnimationName, BlendInSpeed, Flags);
        }

        public void PlayAndWait(Rage.Ped ped, int timeOut = -1)
        {
            ped.Tasks.PlayAnimation(Dictionary, AnimationName, BlendInSpeed, Flags).WaitForCompletion(timeOut);
        }
    }
}
