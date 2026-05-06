using NotificationService.Enums;

namespace NotificationService.DTOs
{
    public class UserDeviceResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceToken { get; set; } = null!;
        public DeviceType DeviceType { get; set; }
        public bool IsActive { get; set; }
    }
}
