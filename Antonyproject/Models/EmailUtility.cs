using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Antonyproject.Models
{
    public class EmailUtility
    {
        public static void sendMail(String fromEmail, String eMessage, String Subject)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential("anthonyrhodes777@gmail.com", "51891coolingbro");

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            MailAddress from = new MailAddress(fromEmail);
            MailAddress to = new MailAddress("anthonyrhodes777@gmail.com");
            MailMessage message = new MailMessage(from, to);
            message.Subject = Subject;
            message.Body = eMessage;

            message.BodyEncoding = System.Text.Encoding.UTF8;
            client.Send(message);

            message.Dispose();

        }
    }
}