using RedisGeoUnit = StackExchange.Redis.GeoUnit;

namespace DeliveryService.Options
{
    public class DeliveryOption
    {
        public double DeliveryRadius { get; set; }
        public double FindingShipperRadius { get; set; }
        public string GeoUnit { get; set; } = "Kilometers";
        public RedisGeoUnit RedisGeoUnit => DeliveryGeoUnitParser.Parse(GeoUnit);
        public decimal BaseDeliveryFee { get; set; } = 10000m;
        public decimal FeePerKm { get; set; } = 5000m;
        public decimal MinimumDeliveryFee { get; set; } = 10000m;
        public string Currency { get; set; } = "VND";
        public int MaxShippersPerBatch { get; set; } = 5;
        public int AssignmentOfferTimeoutSeconds { get; set; } = 20;
        public int AllowedLocationStalenessSeconds { get; set; } = 30;
        public int AssignmentOfferExpirationScanSeconds { get; set; } = 5;

        public bool TryGetRedisGeoUnit(out RedisGeoUnit geoUnit)
        {
            return DeliveryGeoUnitParser.TryParse(GeoUnit, out geoUnit);
        }
    }

    internal static class DeliveryGeoUnitParser
    {
        public const string SupportedValuesMessage =
            "DeliveryOptions:GeoUnit must be one of: Meters (m), Kilometers (km), Miles (mi), Feet (ft).";

        private static readonly IReadOnlyDictionary<string, RedisGeoUnit> Aliases =
            new Dictionary<string, RedisGeoUnit>(StringComparer.OrdinalIgnoreCase)
            {
                ["m"] = RedisGeoUnit.Meters,
                ["meter"] = RedisGeoUnit.Meters,
                ["meters"] = RedisGeoUnit.Meters,
                ["km"] = RedisGeoUnit.Kilometers,
                ["kilometer"] = RedisGeoUnit.Kilometers,
                ["kilometers"] = RedisGeoUnit.Kilometers,
                ["mi"] = RedisGeoUnit.Miles,
                ["mile"] = RedisGeoUnit.Miles,
                ["miles"] = RedisGeoUnit.Miles,
                ["ft"] = RedisGeoUnit.Feet,
                ["foot"] = RedisGeoUnit.Feet,
                ["feet"] = RedisGeoUnit.Feet
            };

        public static RedisGeoUnit Parse(string? value)
        {
            if (TryParse(value, out var geoUnit))
                return geoUnit;

            throw new InvalidOperationException(SupportedValuesMessage);
        }

        public static bool TryParse(string? value, out RedisGeoUnit geoUnit)
        {
            var normalized = value?.Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                geoUnit = default;
                return false;
            }

            if (Aliases.TryGetValue(normalized, out geoUnit))
                return true;

            if (Enum.TryParse(normalized, ignoreCase: true, out geoUnit) && Enum.IsDefined(geoUnit))
                return true;

            geoUnit = default;
            return false;
        }
    }
}
