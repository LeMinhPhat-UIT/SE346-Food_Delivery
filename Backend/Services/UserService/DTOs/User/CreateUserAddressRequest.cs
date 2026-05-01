using UserService.Commons.DTOs;

namespace UserService.DTOs.User
{
    public class CreateUserAddressRequest : BaseAddress
    {
        public string? Label { get; set; }
        public string? RecipientName { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
    }
}
