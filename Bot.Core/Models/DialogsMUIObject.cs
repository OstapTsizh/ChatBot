using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.Models
{
    public class DialogsMUIObject
    {
        public Dictionary<string, string> AddQuestionDictionary { get; set; }
        public Dictionary<string, string> CancelAndRestartDictionary { get; set; }
        public Dictionary<string, string> ChooseOptionDictionary { get; set; }
        public Dictionary<string, string> CoursesDictionary { get; set; }
        public Dictionary<string, string> EmailDictionary { get; set; }
        public Dictionary<string, string> FinishDictionary { get; set; }
        public Dictionary<string, string> LocationDictionary { get; set; }
        public Dictionary<string, string> MailingDictionary { get; set; }
        public Dictionary<string, string> MainDictionary { get; set; }
        public Dictionary<string, string> MainMenuDictionary { get; set; }
        public Dictionary<string, string> PlannedEventsDictionary { get; set; }
        public Dictionary<string, string> SubscriptionDictionary { get; set; }
        public Dictionary<string, string> QAsDictionary { get; set; }
    }
}
