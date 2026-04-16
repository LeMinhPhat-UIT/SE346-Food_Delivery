namespace NotificationService
{
    public class EmailMessage
    {
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string From { get; set; } = null!;
        public string To { get; set; } = null!;
    }
}
