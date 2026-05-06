using Messaging.Contracts.Common.Models;
using UserService.Enums;

namespace UserService.Entities
{
    public class MerchantRequest : BaseEntity
    {
        public Guid UserId { get; set; }

        public string StoreName { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;

        public string BusinessLicense { get; set; } = null!;
        public string BusinessLicenseUrl { get; set; } = null!;

        public string TaxId { get; set; } = null!;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string RejectedReason { get; set; } = null!;

        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid ReviewedBy { get; set; }

        public User User { get; set; } = null!;
        public User ReviewedUser { get; set; } = null!;
    }
}
