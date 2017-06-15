using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secondary_Callouts.Detective
{
    public class ConversationLine
    { 
        public string ID { get; set; }
        public ConversationLineData[] Lines;

        public ConversationLine() { }
    }

    public enum ResponseType
    {
        OptionOne,
        OptionTwo,
        OptionThree
    }

    public class ConversationLineData
    {
        public ResponseType CorrectAnswer;
        public string[] Question;
        public string[] Answer;

        public string[] PlayerResponseOne;
        public string[] InterrogeeReactionOne;
        public string[] PlayerResponseTwo;
        public string[] InterrogeeReactionTwo;
        public string[] PlayerResponseThree;
        public string[] InterrogeeReactionThree;

        public ConversationLineData()
        {
        }
    }
}
