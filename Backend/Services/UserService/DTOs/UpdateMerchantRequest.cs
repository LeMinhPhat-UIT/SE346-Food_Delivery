using UserService.Enums;

namespace UserService.DTOs
{
    public class UpdateMerchantRequest
    {
        public string? StoreName { get; set; }
        public string? StoreDescription { get; set; }
        public string? StoreLogoUrl { get; set; }
        public string? StoreBannerUrl { get; set; }
        public string? BusinessLicense { get; set; }
        public string? TaxId { get; set; }

        public bool? IsOpen { get; set; }
        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public int? AvgPrepTime { get; set; }

        public MerchantStatus? Status { get; set; }
    }
}
