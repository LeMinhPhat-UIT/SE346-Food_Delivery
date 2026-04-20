using UserService.Enums;

namespace UserService.DTOs
{
    public class MerchantResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string StoreName { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;

        public string StoreLogoUrl { get; set; } = null!;
        public string StoreBannerUrl { get; set; } = null!;

        public string BusinessLicense { get; set; } = null!;
        public string TaxId { get; set; } = null!;

        public bool IsOpen { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public int? AvgPrepTime { get; set; }

        public MerchantStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
