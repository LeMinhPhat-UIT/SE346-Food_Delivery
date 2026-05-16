using Messaging.Contracts.Common.Models;
using NotificationService.Enums;

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
