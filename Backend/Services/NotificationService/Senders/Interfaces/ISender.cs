namespace NotificationService.Senders.Interfaces
{
    public abstract class ISender
    {
        protected readonly ISenderProvider _provider;

        public ISender(ISenderProvider provider)
        {
            _provider = provider;
        }

        public abstract Task SendAsync(object content);
    }
}
