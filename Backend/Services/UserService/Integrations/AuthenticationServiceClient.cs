using Messaging.Contracts.Common;
using System.Net.Http.Json;

namespace UserService.Integrations
{
    public class AuthenticationServiceClient : IAuthenticationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticationServiceClient> _logger;

        public AuthenticationServiceClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticationServiceClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/auth/users/{userId}/roles");
                if (_httpContextAccessor.HttpContext?.Request.Headers.Authorization is { Count: > 0 } authorization)
                    request.Headers.TryAddWithoutValidation("Authorization", authorization.ToString());

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AuthenticationService returned {StatusCode} for user roles {UserId}", response.StatusCode, userId);
                    return Array.Empty<string>();
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<UserRolesResponse>>(cancellationToken);
                if (payload?.Success != true || payload.Data == null)
                    return Array.Empty<string>();

                return payload.Data.Roles.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to resolve roles for user {UserId}", userId);
                return Array.Empty<string>();
            }
        }

        private sealed class UserRolesResponse
        {
            public Guid UserId { get; set; }
            public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
        }
    }
}
