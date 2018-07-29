using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Tangy
{
    public class FakeSmtpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient client = new SmtpClient("localhost");
            client.UseDefaultCredentials = true;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("noreply@test.com");
            mailMessage.To.Add(email);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlMessage;
            mailMessage.Subject = subject;

            client.Send(mailMessage);

            return Task.CompletedTask;
        }
    }
}