using UserService.Commons;
using UserService.Enums;

namespace UserService.Entities
{
    public class User : BaseAuditableEntity
    {
        public string FullName { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;

        public UserStatus Status { get; set; } = UserStatus.PendingVerification;

        public Guid? MerchantId { get; set; }
        public Merchant? Merchant { get; set; }

        public Guid? ShipperId { get; set; }
        public Shipper? Shipper { get; set; }

        public ICollection<Address> Addresses { get; set; } = null!;
        public ICollection<ShipperRequest> ShipperRequests { get; set; } = null!;
        public ICollection<MerchantRequest> MerchantRequests { get; set; } = null!;
    }
}
