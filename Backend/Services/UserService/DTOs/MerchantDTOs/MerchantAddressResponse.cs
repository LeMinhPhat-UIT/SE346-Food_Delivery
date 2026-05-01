using UserService.Commons.DTOs;

namespace UserService.DTOs.MerchantDTOs
{
    public class MerchantAddressResponse : BaseAddress
    {
        public Guid Id { get; set; }
        public Guid MerchantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
