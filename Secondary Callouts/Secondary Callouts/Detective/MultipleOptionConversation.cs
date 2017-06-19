using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Fiskey111Common;
using LtFlash.Common.Processes;
using Rage;
using Secondary_Callouts.ExtensionMethods;

namespace Secondary_Callouts.Detective
{
    public class MultipleOptionConversation
    {
        public MultipleOptionLine[] Conversation { get; }
        public Ped DetectivePed { get; }
        public Ped Player => Game.LocalPlayer.Character;
        public float StartDistance { get; set; }
        public bool IsRunning { get; private set; }
        public bool HasEnded { get; private set; }

        private ProcessHost _procHost = new ProcessHost();
        private int _conversationLine;
        private const string FontName = "Arial";
        private const float FontSize = 16f;
        private SizeF _stringSizeF;

        private Keys[] _keyArray =
        {
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
            Keys.D0,
        };

        private readonly Animations[] _animation =
        {
            new Animations("gestures@m@car@van@casual@ps", "gesture_hand_left_three"),
            new Animations("gestures@m@car@van@casual@ps", "gesture_hand_right_three"),
            new Animations("gestures@m@car@van@casual@ps", "gesture_why"),
            new Animations("gestures@m@sitting@generic@casual", "gesture_bring_it_on"),
            new Animations("gestures@m@standing@casual", "gesture_easy_now"),
        };

        public MultipleOptionConversation(IEnumerable<MultipleOptionLine> lines, Ped det, float startDist = 3.0f)
        {
            Conversation = lines.ToArray();
            DetectivePed = det;
            StartDistance = startDist;
        }

        public void Start()
        {
            IsRunning = true;
            if (!DetectivePed)
            {
                Game.DisplayNotification("Detective error, see log");
                Game.LogTrivial("Detective not found");
                return;
            }
            DetectivePed.IsInvincible = true;
            DetectivePed.IsPersistent = true;
            DetectivePed.BlockPermanentEvents = true;

            _procHost.Start();
            _procHost.ActivateProcess(QuestionDisplay);
        }

        // MAIN LOGIC

        private void QuestionDisplay()
        {
            if (Player.DistanceTo(DetectivePed) > StartDistance) return;

            DisplayLine(Conversation[_conversationLine].Question, DetectivePed);
            _procHost.SwapProcesses(QuestionDisplay, UserInput);
        }

        private void UserInput()
        {
            _stringSizeF = GetStringInfo();

            var selectedOption = Conversation[_conversationLine];
            var options = selectedOption.Options.Select(option => option.OptionValue).Cast<dynamic>().ToList();
            var optionQuestion = Conversation[_conversationLine].Question;

            DetectiveMenu.StartMenu(options, optionQuestion, Convert.ToInt32(_stringSizeF.Width), DetectiveMenu.InteractionType.Detective, UserResponseSelected);
            $"Displaying question {optionQuestion}".AddLog();
            _procHost.DeactivateProcess(UserInput);
        }

        private void DetectiveResponse(string response = "")
        {
            ("DetectiveResponse for conversationline " + _conversationLine).AddLog();

            DisplayLine(response, DetectivePed);
            _conversationLine++;

            if (_conversationLine == Conversation.Length - 1) _procHost.ActivateProcess(End);
            else _procHost.ActivateProcess(QuestionDisplay);           
        }

        private void End()
        {
            Game.LogTrivial("End()");
            HasEnded = true;
            IsRunning = false;
            DetectivePed.IsInvincible = false;
            DetectivePed.IsPersistent = false;
            DetectivePed.BlockPermanentEvents = false;
            _procHost.Stop();
        }

        // SUPPORTING METHODS

        internal void UserResponseSelected(string optionText)
        {
            var option = Conversation[_conversationLine].Options.FirstOrDefault(c => c.OptionValue == optionText);
            if (option == null) return;
            DisplayLine(option.OptionSubtitle, Player);

            DetectiveResponse(option.DetectiveResponse);
        }

        private SizeF GetStringInfo()
        {
            var stringValues = Conversation[_conversationLine].Options.Select((t, i) => $"\n~y~Option {i + 1}: ~w~" + t.OptionValue).ToArray();

            var longestString = stringValues.OrderByDescending(s => s.Length).FirstOrDefault();

            using (var graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
                return graphics.MeasureString(longestString, new Font(FontName, FontSize, FontStyle.Regular, GraphicsUnit.Point));
        }

        //        private void Game_RawFrameRender(object sender, GraphicsEventArgs e)
        //        {
        //            var stringValues = Conversation[_conversationLine].Options.Select((t, i) => $"\n{i + 1}:" + t.OptionValue).ToArray();
        //            var stringValue = string.Concat(stringValues);
        //            e.Graphics.DrawRectangle(new RectangleF(7f, 7f, _stringSizeF.Width, _stringSizeF.Height * stringValues.Length / 2 + 5f), Color.Black);
        //            e.Graphics.DrawText(stringValue, FontName, FontSize, new PointF(10f, 0f), Color.White, new RectangleF(7f, 7f, _stringSizeF.Width, _stringSizeF.Height * stringValues.Length / 2));
        //        }

        private void PlayFacialAnim(Ped p)
        {
            p.Tasks.PlayAnimation("mp_facial", "mic_chatter", 3f, AnimationFlags.SecondaryTask);
        }

        public static int CountWords(string s) => Regex.Matches(s, @"[\S]+").Count;

        private static int GetDisplayTime(int wordCount) => wordCount / 2 * 1000 < 3500 ? 4000 : wordCount / 2 * 1000;

        private void DisplayLine(string line, Ped ped)
        {
            $"Displaying line {line}".AddLog();
            var sleepTime = GetDisplayTime(CountWords(line));
            Game.DisplaySubtitle(line, sleepTime - 50);
            PedPlayRndAnim(ped);
            PlayFacialAnim(ped);
            GameFiber.Sleep(sleepTime);
            ped.Tasks.Clear();
        }

        private void PedPlayRndAnim(Ped p)
        {
            var anim = _animation[MathHelper.GetRandomInteger(_animation.Length - 1)];
            p.Tasks.PlayAnimation(anim.Dictionary, anim.AnimationName, anim.BlendInSpeed, anim.AnimationFlag);
        }
    }
}
