using Microsoft.Extensions.Options;
using NotificationService.Options;
using NotificationService.Senders.Interfaces;
using System.Net.Mail;

namespace NotificationService.Senders.Implements
{
    public class EmailSenderProvider : ISenderProvider
    {
        private EmailOptions _emailOptions;

        public EmailSenderProvider(IOptions<EmailOptions> option)
        {
            _emailOptions = option.Value;
        }

        public async Task SendAsync(object message)
        {
            var sendingMessage = (EmailMessage)message;

            SmtpClient client = new SmtpClient(_emailOptions.Host, _emailOptions.Port);
            MailMessage mail = new MailMessage(sendingMessage.From, sendingMessage.To, sendingMessage.Subject, sendingMessage.Body);

            await client.SendMailAsync(mail);
        }
    }
}
