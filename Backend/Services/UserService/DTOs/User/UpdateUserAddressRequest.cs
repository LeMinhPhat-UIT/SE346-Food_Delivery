namespace UserService.DTOs.User
{
    public class UpdateUserAddressRequest
    {
        public string AddressLine { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? City { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
        public string? Label { get; set; }
        public string? RecipientName { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
    }
}
