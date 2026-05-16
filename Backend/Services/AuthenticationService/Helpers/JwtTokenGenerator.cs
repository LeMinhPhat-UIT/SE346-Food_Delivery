using AuthenticationService.Entities;
using AuthenticationService.Options;
using AuthenticationService.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationService.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IAuthRepository _authRepository;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions, IAuthRepository authRepository)
        {
            _jwtOptions = jwtOptions;
            _authRepository = authRepository;
        }

        public async Task<string> AccessTokenGenerate(ApplicationUser user)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(await _authRepository.GetClaimsAsync(user));

            var roles = await _authRepository.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            string key = _jwtOptions.Value.Key;
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer: _jwtOptions.Value.Issuer,
                    audience: _jwtOptions.Value.Audience,
                    expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenMinutes),
                    signingCredentials: creds,
                    claims: claims
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string RefreshTokenGenerate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
