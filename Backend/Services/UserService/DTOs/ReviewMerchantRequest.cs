using UserService.Enums;

namespace UserService.DTOs
{
    public class ReviewMerchantRequest
    {
        public VerificationStatus VerificationStatus { get; set; }
        public string? RejectedReason { get; set; }
    }
}
