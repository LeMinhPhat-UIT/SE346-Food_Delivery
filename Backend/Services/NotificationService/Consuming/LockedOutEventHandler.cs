using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Microsoft.Extensions.Options;
using NotificationService.Options;
using System.Net;
using System.Net.Mail;

namespace NotificationService.Consuming
{
    public class LockedOutEventHandler : IEventHandler<LockedOutEvent>
    {
        private readonly ILogger<LockedOutEventHandler> _logger;
        private readonly IOptions<EmailOptions> _emailOptions;

        public LockedOutEventHandler(ILogger<LockedOutEventHandler> logger, IOptions<EmailOptions> emailOptions)
        {
            _logger = logger;
            _emailOptions = emailOptions;
        }

        public async Task Handle(LockedOutEvent @event)
        {
            try
            {
                _logger.LogInformation(
                    "Handling LockedOut event for user {UserId}, sending to email: {Email}",
                    @event.UserId,
                    @event.Email);

                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; margin-top: 20px; }}
        .warning {{ font-size: 18px; font-weight: bold; color: #f44336; text-align: center; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Account Locked</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>Your account has been <strong>temporarily locked</strong> due to too many failed login attempts.</p>

            <div class=""warning"">
                Too many incorrect login attempts detected
            </div>

            <p><strong>Lockout will end at: {@event.LockoutEndDate:yyyy-MM-dd HH:mm:ss} UTC</strong></p>

            <p>If this was you, please wait until the lock expires before trying again.</p>
            <p>If you did not attempt to log in, we recommend that you reset your password immediately to secure your account.</p>

            <p>You can use the ""Forgot Password"" feature to regain access.</p>
                </ div >
        
                < div class=""footer"">
            <p>&copy; 2026 Food Delivery.All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                var mail = new EmailMessage()
                {
                    From = _emailOptions.Value.FromEmail,
                    To = @event.Email,
                    Body = emailBody,
                    Subject = "Your Account Has Been Locked - Food Delivery"
                };

                SmtpClient client = new SmtpClient(_emailOptions.Value.Host, _emailOptions.Value.Port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _emailOptions.Value.FromEmail,
                        _emailOptions.Value.Password
                    ),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000
                };

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(mail.From),
                    Subject = mail.Subject,
                    Body = mail.Body,
                    IsBodyHtml = true
                };
                message.To.Add(mail.To);

                await client.SendMailAsync(message);

                _logger.LogInformation(
                    "LockedOut email sent successfully to {Email} for user {UserId}",
                    @event.Email,
                    @event.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send LockedOut email to {Email}", @event.Email);
                throw;
            }
        }
    }
}
