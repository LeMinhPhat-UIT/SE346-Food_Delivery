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

        [MapperIgnoreTarget(nameof(Address.UserId))]
        [MapperIgnoreTarget(nameof(Address.User))]
        [MapperIgnoreTarget(nameof(Address.Id))]
        public partial Address ToAddress(CreateUserAddressRequest request);

        [MapperIgnoreSource(nameof(Address.UserId))]
        [MapperIgnoreSource(nameof(Address.User))]
        [MapperIgnoreSource(nameof(Address.Id))]
        public partial UserAddressResponse ToUserAddressResponse(Address address);

        [MapperIgnoreSource(nameof(Address.UserId))]
        [MapperIgnoreSource(nameof(Address.User))]
        [MapperIgnoreSource(nameof(Address.Id))]
        public partial IEnumerable<UserAddressResponse> ToUserAddressResponseList(IEnumerable<Address> addresses);
    }
}
