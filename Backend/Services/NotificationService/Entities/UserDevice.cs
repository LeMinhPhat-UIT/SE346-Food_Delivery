using NotificationService.Enums;
using Messaging.Contracts.Common.Models;

namespace NotificationService.Entities
{
    public class UserDevice : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string DeviceToken { get; set; } = null!;
        public DeviceType DeviceType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
