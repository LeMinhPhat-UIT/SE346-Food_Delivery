using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using Microsoft.Extensions.Options;
using NotificationService.Options;
using System.Net;
using System.Net.Mail;

namespace NotificationService.Consuming
{
    public class OtpEmailEventHandler : IEventHandler<OtpSendRequestedEvent>
    {
        private readonly ILogger<OtpEmailEventHandler> _logger;
        private readonly IOptions<EmailOptions> _emailOptions;

        public OtpEmailEventHandler(ILogger<OtpEmailEventHandler> logger, IOptions<EmailOptions> emailOptions)
        {
            _logger = logger;
            _emailOptions = emailOptions;
        }

        public async Task Handle(OtpSendRequestedEvent @event)
        {
            try
            {
                _logger.LogInformation(
                    "Handling OTP event for user {UserId}, sending to email: {Email}",
                    @event.UserId,
                    @event.Email);

                var isPasswordReset = string.Equals(@event.OtpType, "password-reset", StringComparison.OrdinalIgnoreCase);
                var header = isPasswordReset ? "Password Reset" : "Email Verification";
                var intro = isPasswordReset
                    ? "Please use the following OTP code to reset your Food Delivery password:"
                    : "Thank you for registering with Food Delivery! Please use the following OTP code to verify your email address:";
                var subject = isPasswordReset
                    ? "Your Password Reset Code - Food Delivery"
                    : "Your Email Verification Code - Food Delivery";

                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 5px; margin-top: 20px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #4CAF50; text-align: center; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{header}</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>{intro}</p>
            <div class=""otp-code"">{@event.Otp}</div>
            <p><strong>This code will expire at: {@event.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC</strong></p>
            <p>If you didn't request this code, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Food Delivery. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                var mail = new EmailMessage()
                {
                    From = _emailOptions.Value.FromEmail,
                    To = @event.Email,
                    Body = emailBody,
                    Subject = subject
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
                    "OTP email sent successfully to {Email} for user {UserId}",
                    @event.Email,
                    @event.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", @event.Email);
                throw;
            }
        }
    }
}
