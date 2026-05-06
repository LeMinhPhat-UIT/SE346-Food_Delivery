using System.Text.Json.Serialization;

namespace NotificationService.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceType
    {
        Ios, Android, Web
    }
}
