using Riok.Mapperly.Abstractions;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Mappers
{
    [Mapper]
    public partial class UserMapper
    {
        [MapperIgnoreSource(nameof(User.MerchantId))]
        [MapperIgnoreSource(nameof(User.Merchant))]
        [MapperIgnoreSource(nameof(User.ShipperId))]
        [MapperIgnoreSource(nameof(User.Shipper))]
        [MapperIgnoreSource(nameof(User.Addresses))]
        [MapperIgnoreSource(nameof(User.CreatedAt))]
        [MapperIgnoreSource(nameof(User.UpdatedAt))]
        [MapperIgnoreSource(nameof(User.DeletedAt))]
        public partial IEnumerable<UserProfileResponse> ToUserProfileResponseList(IEnumerable<User> userList);

        [MapperIgnoreSource(nameof(User.MerchantId))]
        [MapperIgnoreSource(nameof(User.Merchant))]
        [MapperIgnoreSource(nameof(User.ShipperId))]
        [MapperIgnoreSource(nameof(User.Shipper))]
        [MapperIgnoreSource(nameof(User.Addresses))]
        [MapperIgnoreSource(nameof(User.CreatedAt))]
        [MapperIgnoreSource(nameof(User.UpdatedAt))]
        [MapperIgnoreSource(nameof(User.DeletedAt))]
        public partial UserProfileResponse ToUserProfileResponse(User user);
    }
}
