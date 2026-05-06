namespace DeliveryService.DTOs
{
    public class ToggleAvailabilityRequest
    {
        public bool IsGoOnline { get; set; }

        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }
    }
}
