using UserService.Commons.DTOs;

namespace UserService.DTOs.User
{
    public class UserAddressResponse : BaseAddress
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Label { get; set; }
        public string? RecipientName { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
