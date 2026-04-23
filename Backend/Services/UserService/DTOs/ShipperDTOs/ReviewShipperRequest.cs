using UserService.Enums;

namespace UserService.DTOs.ShipperDTOs
{
    public class ReviewShipperRequest
    {
        public VerificationStatus VerificationStatus { get; set; }
        public string? RejectedReason { get; set; }
    }
}
