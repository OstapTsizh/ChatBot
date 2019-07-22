using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace DecisionMakers
{
    public class Mailer
    {
        readonly MailMessage _mail = new MailMessage();
        readonly SmtpClient _smtpClient = new SmtpClient("smtp.gmail.com");

        public void MailMethod()
        {
            _mail.From = new MailAddress("mnadorozhhniak@gmail.com");
            _mail.To.Add("mrmisha999@gmail.com");
            _mail.Subject = "Studdy Bot";

            _smtpClient.Port = 587;
            _smtpClient.UseDefaultCredentials = true;
            _smtpClient.EnableSsl = true;

            _smtpClient.SendMailAsync(_mail);
            Console.WriteLine("Massage send");
        }
    }
}
