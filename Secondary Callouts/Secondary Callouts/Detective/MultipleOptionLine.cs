using System.Collections.Generic;
using System.Linq;

namespace Secondary_Callouts.Detective
{
    public class MultipleOptionLine
    {
        public string Question { get; }
        public Option[] Options { get; }

        public MultipleOptionLine(string ques, IEnumerable<Option> options)
        {
            Question = ques;
            Options = options.ToArray();
        }
    }

    public class Option
    {
        public string OptionValue { get; }
        public string OptionSubtitle { get; }
        public string DetectiveResponse { get; }

        public Option(string displayText, string response, string subtitle = null)
        {
            OptionValue = displayText;
            OptionSubtitle = string.IsNullOrEmpty(subtitle) ? displayText : subtitle;
            DetectiveResponse = response;
        }
    }
}
