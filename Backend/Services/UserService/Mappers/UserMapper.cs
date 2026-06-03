using Riok.Mapperly.Abstractions;
using UserService.DTOs.User;
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
        [MapperIgnoreSource(nameof(User.ShipperRequests))]
        [MapperIgnoreSource(nameof(User.MerchantRequests))]
        [MapperIgnoreSource(nameof(User.CreatedAt))]
        [MapperIgnoreSource(nameof(User.UpdatedAt))]
        [MapperIgnoreSource(nameof(User.DeletedAt))]
        [MapperIgnoreTarget(nameof(UserProfileResponse.Roles))]
        [MapProperty(nameof(User.AvatarUrl), nameof(UserProfileResponse.AvatarFileKey))]
        public partial IEnumerable<UserProfileResponse> ToUserProfileResponseList(IEnumerable<User> userList);

        [MapperIgnoreSource(nameof(User.MerchantId))]
        [MapperIgnoreSource(nameof(User.Merchant))]
        [MapperIgnoreSource(nameof(User.ShipperId))]
        [MapperIgnoreSource(nameof(User.Shipper))]
        [MapperIgnoreSource(nameof(User.Addresses))]
        [MapperIgnoreSource(nameof(User.ShipperRequests))]
        [MapperIgnoreSource(nameof(User.MerchantRequests))]
        [MapperIgnoreSource(nameof(User.CreatedAt))]
        [MapperIgnoreSource(nameof(User.UpdatedAt))]
        [MapperIgnoreSource(nameof(User.DeletedAt))]
        [MapperIgnoreTarget(nameof(UserProfileResponse.Roles))]
        [MapProperty(nameof(User.AvatarUrl), nameof(UserProfileResponse.AvatarFileKey))]
        public partial UserProfileResponse ToUserProfileResponse(User user);

        [MapperIgnoreTarget(nameof(Address.UserId))]
        [MapperIgnoreTarget(nameof(Address.User))]
        [MapperIgnoreTarget(nameof(Address.Id))]
        [MapperIgnoreTarget(nameof(Address.CreatedAt))]
        [MapperIgnoreTarget(nameof(Address.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Address.DeletedAt))]
        [MapperIgnoreTarget(nameof(Address.District))]
        public partial Address ToAddress(CreateUserAddressRequest request);

        [MapperIgnoreSource(nameof(Address.User))]
        [MapperIgnoreSource(nameof(Address.UpdatedAt))]
        [MapperIgnoreSource(nameof(Address.DeletedAt))]
        public partial UserAddressResponse ToUserAddressResponse(Address address);

        [MapperIgnoreSource(nameof(Address.User))]
        [MapperIgnoreSource(nameof(Address.UpdatedAt))]
        [MapperIgnoreSource(nameof(Address.DeletedAt))]
        public partial IEnumerable<UserAddressResponse> ToUserAddressResponseList(IEnumerable<Address> addresses);
    }
}
