using UserService.Enums;

namespace UserService.DTOs.ShipperDTOs
{
    public class ShipperRequestResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string LicenseNumber { get; set; } = null!;
        public string LicenseFrontUrl { get; set; } = null!;
        public string LicenseBackUrl { get; set; } = null!;

        public string IdFrontUrl { get; set; } = null!;
        public string IdBackUrl { get; set;} = null!;

        public string SelfieUrl { get; set; } = null!;

        public string IdNumber { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public VerificationStatus Status { get; set; }
        public string RejectedReason { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? ReviewedBy { get; set; }
    }
}
