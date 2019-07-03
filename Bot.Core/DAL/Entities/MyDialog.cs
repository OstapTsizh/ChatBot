using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StuddyBot.Core.DAL.Entities
{
    public class MyDialog
    {
        public int Id { get; set; }

        public string Sender { get; set; }

        public string Message { get; set; }

        public DateTimeOffset Time { get; set; }

        public int DialogsId { get; set; }
        public Dialogs Dialogs { get; set; }
    }
}
