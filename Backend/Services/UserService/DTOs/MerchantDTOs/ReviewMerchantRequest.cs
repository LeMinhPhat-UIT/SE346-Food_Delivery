using UserService.Enums;

namespace UserService.DTOs.MerchantDTOs
{
    public class ReviewMerchantRequest
    {
        public VerificationStatus VerificationStatus { get; set; }
        public string? RejectedReason { get; set; }
    }
}
