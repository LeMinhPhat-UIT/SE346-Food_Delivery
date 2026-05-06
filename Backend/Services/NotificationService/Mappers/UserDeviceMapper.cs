using NotificationService.DTOs;
using NotificationService.Entities;
using Riok.Mapperly.Abstractions;

namespace NotificationService.Mappers
{
    [Mapper]
    public partial class UserDeviceMapper
    {
        [MapperIgnoreSource(nameof(UserDevice.UpdatedAt))]
        [MapperIgnoreSource(nameof(UserDevice.CreatedAt))]
        [MapperIgnoreSource(nameof(UserDevice.DeletedAt))]
        public partial UserDeviceResponse ToUserDeviceResponse(UserDevice userDevice);

        [MapperIgnoreSource(nameof(UserDevice.UpdatedAt))]
        [MapperIgnoreSource(nameof(UserDevice.CreatedAt))]
        [MapperIgnoreSource(nameof(UserDevice.DeletedAt))]
        public partial IEnumerable<UserDeviceResponse> ToUserDeviceResponseList(IEnumerable<UserDevice> userDevices);
    }
}
