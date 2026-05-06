namespace NotificationService.DTOs
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; } = null!;
    }
}
