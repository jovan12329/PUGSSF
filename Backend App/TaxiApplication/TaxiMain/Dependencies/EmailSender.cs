using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;
using System.Diagnostics;



namespace TaxiMain.Dependencies
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                TargetName = "STARTTLS/smtp.gmail.com",
                Credentials = new NetworkCredential("drsprodavnica@gmail.com", "quhm uutx gnwn mxlg")
            };

            
                return client.SendMailAsync(
                new MailMessage(from: "drsprodavnica@gmail.com",
                                to: email,
                                subject,
                                message
                                ));

        }
    }
    
}
