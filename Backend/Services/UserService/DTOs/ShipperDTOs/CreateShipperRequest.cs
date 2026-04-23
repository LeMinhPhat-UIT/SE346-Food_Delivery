namespace UserService.DTOs.ShipperDTOs
{
    public class CreateShipperRequest
    {
        public string LicenseNumber { get; set; } = null!;
        public string LicenseFrontUrl { get; set; } = null!;
        public string LicenseBackUrl { get; set;} = null!;
        public string IdCardFrontUrl { get; set; } = null!;
        public string IdCardBackUrl { get;set; } = null!;
        public string SelfieUrl { get; set; } = null!;
        public string IdNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }
}
