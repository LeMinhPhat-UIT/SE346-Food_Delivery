using AuthenticationService.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationService.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(Guid userId);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure);
        Task<IdentityResult> AccessFailedAsync(ApplicationUser user);
        Task<bool> IsLockedOutAsync(ApplicationUser user);
        Task<IdentityResult> ResetAccessFailedCountAsync(ApplicationUser user);
        Task<IList<Claim>> GetClaimsAsync(ApplicationUser user);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
        Task UpdateRefreshTokenAsync(RefreshToken token);
    }
}
