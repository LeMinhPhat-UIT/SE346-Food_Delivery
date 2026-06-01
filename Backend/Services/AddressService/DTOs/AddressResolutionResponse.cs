namespace AddressService.DTOs
{
    public class AddressResolutionResponse
    {
        public string ProvinceCode { get; set; } = null!;
        public string ProvinceName { get; set; } = null!;
        public string ProvinceFullName { get; set; } = null!;
        public string WardCode { get; set; } = null!;
        public string WardName { get; set; } = null!;
        public string WardFullName { get; set; } = null!;
        public string? AddressLine { get; set; }
        public string FullAddress { get; set; } = null!;

        public string City { get; set; } = null!;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = null!;
    }
}
