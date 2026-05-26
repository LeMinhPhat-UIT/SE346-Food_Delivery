using DeliveryService.Entities;
using DeliveryService.Exceptions;
using DeliveryService.Options;
using DeliveryService.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeliveryService.Services.Implements
{
    public class OpenRouteServiceClient : IOpenRouteServiceClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;
        private readonly IOptions<OpenRouteServiceOptions> _options;
        private readonly ILogger<OpenRouteServiceClient> _logger;

        public OpenRouteServiceClient(
            HttpClient httpClient,
            IOptions<OpenRouteServiceOptions> options,
            ILogger<OpenRouteServiceClient> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public async Task<RouteEstimate> EstimateRouteAsync(
            decimal pickupLat,
            decimal pickupLng,
            decimal deliveryLat,
            decimal deliveryLng,
            CancellationToken cancellationToken = default)
        {
            var options = _options.Value;
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new OpenRouteServiceException("OpenRouteService API key is missing.");

            var profile = string.IsNullOrWhiteSpace(options.Profile)
                ? "driving-car"
                : options.Profile.Trim();

            var payload = new MatrixRequest
            {
                Locations = new[]
                {
                    new[] { pickupLng, pickupLat },
                    new[] { deliveryLng, deliveryLat }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"v2/matrix/{Uri.EscapeDataString(profile)}")
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            request.Headers.TryAddWithoutValidation("Authorization", options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenRouteService matrix request failed with status {StatusCode}: {ResponseBody}",
                    (int)response.StatusCode,
                    body);

                throw new OpenRouteServiceException("OpenRouteService route estimation failed.");
            }

            MatrixResponse? matrixResponse;
            try
            {
                matrixResponse = JsonSerializer.Deserialize<MatrixResponse>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new OpenRouteServiceException("OpenRouteService returned an invalid route estimate response.", ex);
            }

            var distanceKm = matrixResponse?.Distances?.ElementAtOrDefault(0)?.ElementAtOrDefault(0);
            var durationSeconds = matrixResponse?.Durations?.ElementAtOrDefault(0)?.ElementAtOrDefault(0);

            if (!distanceKm.HasValue || !durationSeconds.HasValue)
                throw new OpenRouteServiceException("OpenRouteService did not return distance and duration for this route.");

            return new RouteEstimate
            {
                DistanceKm = Math.Max(0m, distanceKm.Value),
                DurationSeconds = Math.Max(0d, durationSeconds.Value)
            };
        }

        private sealed class MatrixRequest
        {
            [JsonPropertyName("locations")]
            public decimal[][] Locations { get; init; } = Array.Empty<decimal[]>();

            [JsonPropertyName("sources")]
            public string[] Sources { get; init; } = new[] { "0" };

            [JsonPropertyName("destinations")]
            public string[] Destinations { get; init; } = new[] { "1" };

            [JsonPropertyName("metrics")]
            public string[] Metrics { get; init; } = new[] { "distance", "duration" };

            [JsonPropertyName("units")]
            public string Units { get; init; } = "km";
        }

        private sealed class MatrixResponse
        {
            [JsonPropertyName("distances")]
            public decimal?[][]? Distances { get; init; }

            [JsonPropertyName("durations")]
            public double?[][]? Durations { get; init; }
        }
    }
}
