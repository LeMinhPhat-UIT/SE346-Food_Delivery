using DeliveryService.Commons;

namespace DeliveryService.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Type { get; set; } = null!;// order_update, promotion, system
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
