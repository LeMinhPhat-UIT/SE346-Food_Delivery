using AuthenticationService.DTOs;
using Riok.Mapperly.Abstractions;

namespace AuthenticationService.Mappers
{
    [Mapper]
    public partial class TokenRequestMapper
    {
        public RevokeTokenRequest ToRevokeTokenRequest(RefreshTokenRequest request)
        {
            return new RevokeTokenRequest()
            {
                DeviceName = request.DeviceName,
                RefreshToken = request.RefreshToken,
            };
        }
    }
}
