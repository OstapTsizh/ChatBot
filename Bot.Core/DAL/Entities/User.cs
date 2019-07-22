using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Entities
{
    public class User
    {
        public string Id { get; set; }

        public ICollection<Dialogs> Dialogs { get; set; }

        public ICollection<UserCourse> UserCourses { get; set; }

        public string ConversationReference { get; set; }
    }
}
