namespace AuthenticationService.DTOs
{
    public class MerchantRegisterRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string StoreName { get; set; } = null!;
        public string StoreAddress { get; set; } = null!;
        public string BusinessLicenseNumber { get; set; } = null!;
    }
}
