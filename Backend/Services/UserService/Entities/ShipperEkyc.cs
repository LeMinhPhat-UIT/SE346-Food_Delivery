using Messaging.Contracts.Common.Models;
using UserService.Enums;

namespace UserService.Entities
{
    public class ShipperEkyc : BaseEntity
    {
        public Guid ShipperId { get; set; }

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

        public Shipper Shipper { get; set; } = null!;
    }
}
