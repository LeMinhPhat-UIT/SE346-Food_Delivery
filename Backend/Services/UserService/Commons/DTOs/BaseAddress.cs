namespace UserService.Commons.DTOs
{
    public abstract class BaseAddress
    {
        public string AddressLine { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
