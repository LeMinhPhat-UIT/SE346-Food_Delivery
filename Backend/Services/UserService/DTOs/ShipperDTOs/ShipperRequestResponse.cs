using UserService.Enums;

namespace UserService.DTOs.ShipperDTOs
{
    public class ShipperRequestResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string LicenseNumber { get; set; } = null!;
        public string LicenseFrontFileKey { get; set; } = null!;
        public string LicenseBackFileKey { get; set; } = null!;

        public string IdCardFrontFileKey { get; set; } = null!;
        public string IdCardBackFileKey { get; set; } = null!;

        public string SelfieFileKey { get; set; } = null!;

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
