using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Entities
{
   public class Dialogs
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public ICollection<MyDialog> MyDialogs { get; set; }
    }
}
