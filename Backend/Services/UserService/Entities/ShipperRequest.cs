using Messaging.Contracts.Common.Models;
using UserService.Enums;

namespace UserService.Entities
{
    public class ShipperRequest : BaseEntity
    {
        public Guid UserId { get; set; }

        public string LicenseNumber { get; set; } = null!;
        public string LicenseFrontUrl { get; set; } = null!;
        public string LicenseBackUrl { get; set; } = null!;

        public string IdCardFrontUrl { get; set; } = null!;
        public string IdCardBackUrl { get; set; } = null!;
        public string SelfieUrl { get; set; } = null!;

        public string IdNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string RejectedReason { get; set; } = null!;

        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? ReviewedBy { get; set; }

        public User User { get; set; } = null!;
        public User? ReviewedUser { get; set; }
    }
}
