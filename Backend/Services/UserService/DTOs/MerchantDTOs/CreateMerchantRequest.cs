namespace UserService.DTOs.MerchantDTOs
{
    public class CreateMerchantRequest
    {
        public string StoreName { get; set; } = null!;
        public string StoreDescription { get; set; } = null!;
        public string BusinessLicense { get; set; } = null!;
        public string BusinessLicenseUrl { get; set; } = null!;
        public string TaxId { get; set; } = null!;
    }
}
