namespace AddressService.DTOs
{
    public class AddressResolutionRequest
    {
        public string ProvinceCode { get; set; } = string.Empty;
        public string WardCode { get; set; } = string.Empty;
        public string? AddressLine { get; set; }
    }
}
