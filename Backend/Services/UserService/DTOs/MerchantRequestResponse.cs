using UserService.Enums;

namespace UserService.DTOs
{
    public class MerchantRequestResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string StoreName { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;

        public string BusinessLicense { get; set; } = null!;
        public string BusinessLicenseUrl { get; set; } = null!;

        public string TaxId { get; set; } = null!;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string RejectedReason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
    }
}
