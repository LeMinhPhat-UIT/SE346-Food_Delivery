using NotificationService.Senders.Interfaces;

namespace NotificationService.Senders.Implements
{
    public class OtpSender : ISender
    {
        public OtpSender(ISenderProvider provider) : base(provider) { }

        public override async Task SendAsync(object content)
        {
            await _provider.SendAsync(content);
        }
    }
}
