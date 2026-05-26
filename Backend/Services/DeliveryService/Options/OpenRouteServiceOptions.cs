namespace DeliveryService.Options
{
    public class OpenRouteServiceOptions
    {
        public const string SectionName = "OpenRouteService";

        public string ApiKey { get; set; } = string.Empty;
        public string Url { get; set; } = "https://api.openrouteservice.org";
        public string Profile { get; set; } = "driving-car";
        public int TimeoutSeconds { get; set; } = 10;
    }
}
