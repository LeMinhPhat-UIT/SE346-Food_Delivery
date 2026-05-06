using NotificationService.Enums;

namespace NotificationService.DTOs
{
    public class RegisterDeviceRequest
    {
        public string DeviceToken { get; set; } = null!;
        public DeviceType DeviceType { get; set; }
    }
}
