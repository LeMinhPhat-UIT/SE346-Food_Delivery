using UserService.Commons.Entities;

namespace UserService.Entities
{
    public class MerchantAddress : BaseAuditableEntity
    {
        public Guid MerchantId { get; set; }

        public string AddressLine { get; set; } = null!;
        public string Ward { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;

        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }

        public Merchant Merchant { get; set; } = null!;
    }
}
