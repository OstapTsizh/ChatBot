using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Entities
{
    public class User
    {
        /// <summary>
        /// The user's Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A collection of user's dialogs.
        /// </summary>
        public ICollection<Dialogs> Dialogs { get; set; }

        /// <summary>
        /// A collection of user's courses he is subscribed to.
        /// </summary>
        public ICollection<UserCourse> UserCourses { get; set; }

        /// <summary>
        /// A reference to the user's conversation.
        /// </summary>
        public string ConversationReference { get; set; }

        /// <summary>
        /// The user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The user's language.
        /// </summary>
        public string Language { get; set; }
    }
}
