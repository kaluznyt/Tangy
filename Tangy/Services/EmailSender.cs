using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace Tangy
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient client = new SmtpClient("localhost");
            client.UseDefaultCredentials = true;
            //client.Credentials = new NetworkCredential("abc", "cde");

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("whoever@me.com");
            mailMessage.To.Add(email);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlMessage;
            mailMessage.Subject = subject;

            client.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}