using Messaging.Contracts.Common;
using System.Net.Http.Json;

namespace NotificationService.Integrations
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Guid?> GetUserIdByShipperIdAsync(Guid shipperId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/shippers/{shipperId}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} for shipper {ShipperId}", response.StatusCode, shipperId);
                    return null;
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ShipperResponse>>(cancellationToken);
                if (payload?.Success != true || payload.Data == null)
                    return null;

                return payload.Data.UserId;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to resolve user id for shipper {ShipperId}", shipperId);
                return null;
            }
        }

        private sealed class ShipperResponse
        {
            public Guid UserId { get; set; }
        }
    }
}
