using Riok.Mapperly.Abstractions;
using UserService.DTOs.MerchantDTOs;
using UserService.Entities;

namespace UserService.Mappers
{
    public partial class UserMapper
    {
        [MapperIgnoreTarget(nameof(MerchantRequest.Id))]
        [MapperIgnoreTarget(nameof(MerchantRequest.UserId))]
        [MapperIgnoreTarget(nameof(MerchantRequest.VerificationStatus))]
        [MapperIgnoreTarget(nameof(MerchantRequest.RejectedReason))]
        [MapperIgnoreTarget(nameof(MerchantRequest.VerifiedAt))]
        [MapperIgnoreTarget(nameof(MerchantRequest.CreatedAt))]
        [MapperIgnoreTarget(nameof(MerchantRequest.ReviewedBy))]
        [MapperIgnoreTarget(nameof(MerchantRequest.User))]
        [MapperIgnoreTarget(nameof(MerchantRequest.ReviewedUser))]
        public partial MerchantRequest ToMerchantRequest(CreateMerchantRequest request);

        [MapperIgnoreSource(nameof(MerchantRequest.User))]
        [MapperIgnoreSource(nameof(MerchantRequest.ReviewedUser))]
        public partial IEnumerable<MerchantRequestResponse> ToMerchantRequestResponseList(IEnumerable<MerchantRequest> merchantRequests);

        [MapperIgnoreSource(nameof(MerchantRequest.User))]
        [MapperIgnoreSource(nameof(MerchantRequest.ReviewedUser))]
        public partial MerchantRequestResponse ToMerchantRequestResponse(MerchantRequest merchantRequest);

        [MapperIgnoreTarget(nameof(Merchant.Id))]
        [MapperIgnoreTarget(nameof(Merchant.StoreLogoUrl))]
        [MapperIgnoreTarget(nameof(Merchant.StoreBannerUrl))]
        [MapperIgnoreTarget(nameof(Merchant.IsOpen))]
        [MapperIgnoreTarget(nameof(Merchant.OpeningTime))]
        [MapperIgnoreTarget(nameof(Merchant.ClosingTime))]
        [MapperIgnoreTarget(nameof(Merchant.MinOrderAmount))]
        [MapperIgnoreTarget(nameof(Merchant.AvgPrepTime))]
        [MapperIgnoreTarget(nameof(Merchant.Status))]
        [MapperIgnoreTarget(nameof(Merchant.Addresses))]
        [MapperIgnoreTarget(nameof(Merchant.User))]
        [MapperIgnoreTarget(nameof(Merchant.CreatedAt))]
        [MapperIgnoreTarget(nameof(Merchant.UpdatedAt))]
        [MapperIgnoreTarget(nameof(Merchant.DeletedAt))]
        [MapperIgnoreSource(nameof(MerchantRequest.BusinessLicenseUrl))]
        [MapperIgnoreSource(nameof(MerchantRequest.VerificationStatus))]
        [MapperIgnoreSource(nameof(MerchantRequest.RejectedReason))]
        [MapperIgnoreSource(nameof(MerchantRequest.VerifiedAt))]
        [MapperIgnoreSource(nameof(MerchantRequest.CreatedAt))]
        [MapperIgnoreSource(nameof(MerchantRequest.ReviewedBy))]
        [MapperIgnoreSource(nameof(MerchantRequest.User))]
        [MapperIgnoreSource(nameof(MerchantRequest.ReviewedUser))]
        [MapperIgnoreSource(nameof(MerchantRequest.Id))]
        public partial Merchant ToMerchant(MerchantRequest merchantRequest);

        [MapperIgnoreSource(nameof(Merchant.Addresses))]
        [MapperIgnoreSource(nameof(Merchant.User))]
        [MapperIgnoreSource(nameof(Merchant.DeletedAt))]
        public partial IEnumerable<MerchantResponse> ToMerchantResponseList(IEnumerable<Merchant> merchants);

        [MapperIgnoreSource(nameof(Merchant.Addresses))]
        [MapperIgnoreSource(nameof(Merchant.User))]
        [MapperIgnoreSource(nameof(Merchant.DeletedAt))]
        public partial MerchantResponse ToMerchantResponse(Merchant merchant);
    }
}
