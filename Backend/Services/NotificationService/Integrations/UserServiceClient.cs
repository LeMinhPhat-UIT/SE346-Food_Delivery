using Messaging.Contracts.Common;
using System.Net.Http.Json;

namespace NotificationService.Integrations
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<Guid?> GetShipperIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/shippers/by-user/{userId}");
                if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization is { Count: > 0 } authorization)
                {
                    request.Headers.TryAddWithoutValidation("Authorization", authorization.ToString());
                }
                else if (_httpContextAccessor.HttpContext?.Request.Query["access_token"] is { Count: > 0 } accessToken)
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UserService returned {StatusCode} for user shipper profile {UserId}", response.StatusCode, userId);
                    return null;
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ShipperResponse>>(cancellationToken);
                if (payload?.Success != true || payload.Data == null)
                    return null;

                return payload.Data.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to resolve shipper id for user {UserId}", userId);
                return null;
            }
        }

        private sealed class ShipperResponse
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
        }
    }
}
