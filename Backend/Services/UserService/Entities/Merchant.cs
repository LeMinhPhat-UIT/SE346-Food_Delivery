using UserService.Commons.Entities;
using UserService.Enums;

namespace UserService.Entities
{
    public class Merchant : BaseAuditableEntity
    {
        public Guid UserId { get; set; }

        public string StoreName { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;

        public string StoreLogoUrl { get; set; } = null!;
        public string StoreBannerUrl { get; set; } = null!;

        public string BusinessLicense { get; set; } = null!;
        public string TaxId { get; set; } = null!;

        public bool IsOpen { get; set; } = true;

        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public int? AvgPrepTime { get; set; }

        public MerchantStatus Status { get; set; } = MerchantStatus.Pending;

        public ICollection<MerchantAddress> Addresses { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
