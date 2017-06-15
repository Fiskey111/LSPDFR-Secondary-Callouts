using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiskey111Common;
using Rage;

namespace Secondary_Callouts.Detective
{
    internal class Conversation
    {
        public bool HasEnded { get; }
        public float DistanceToStart { get; }
        public Keys KeysInteract { get; } = Keys.Y;

        public Keys KeyOptionOne { get; } = Keys.D1;
        public Keys KeyOptionTwo { get; } = Keys.D2;
        public Keys KeyOptionThree { get; } = Keys.D3;

        private Ped _detective;
        private GameFiber _fiber;
        private ConversationLineData[] _lines;

        private const string Help = "Pick one of the following:\n~y~ 1 ~s~  Truth\n~y~ 2 ~s~  Doubt\n~y~ 3 ~s~  Lie";
        private const string MSG_PRESS_TO_TALK = "Press ~y~{0}~s~ to continue the talk.";

        private static readonly Animations[] Animation =
        {
            new Animations("gestures@m@car@van@casual@ps", "gesture_hand_left_three"),
            new Animations("gestures@m@car@van@casual@ps", "gesture_hand_right_three"),
            new Animations("gestures@m@car@van@casual@ps", "gesture_why"),
            new Animations("gestures@m@sitting@generic@casual", "gesture_bring_it_on"),
            new Animations("gestures@m@standing@casual", "gesture_easy_now"),
        };

        internal Conversation(Ped detective, ConversationLineData[] dialogue, float startDistance = 2f)
        {
            _detective = detective;
            _lines = dialogue;
            DistanceToStart = startDistance;
        }

        internal void Start()
        {
            _fiber = new GameFiber(TalkFiber);
            _fiber.Start();
        }

        private void TalkFiber()
        {
            
        }
    }
}
