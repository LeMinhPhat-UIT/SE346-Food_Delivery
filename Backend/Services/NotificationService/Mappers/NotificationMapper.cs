using NotificationService.DTOs;
using NotificationService.Entities;
using Riok.Mapperly.Abstractions;

namespace NotificationService.Mappers
{
    [Mapper]
    public partial class NotificationMapper
    {
        [MapperIgnoreSource(nameof(Notification.UserId))]
        [MapperIgnoreSource(nameof(Notification.Type))]
        public partial NotificationResponse ToNotificationResponse(Notification notification);

        [MapperIgnoreSource(nameof(Notification.UserId))]
        [MapperIgnoreSource(nameof(Notification.Type))]
        public partial IEnumerable<NotificationResponse> ToNotificationResponseList(IEnumerable<Notification> notifications);
    }
}
