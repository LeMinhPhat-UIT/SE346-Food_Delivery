using AuthenticationService.Entities;
using AuthenticationService.Persistences;
using AuthenticationService.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthenticationService.Repositories.Implements
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationDbContext _context;

        public AuthRepository(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, AuthenticationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> FindByIdAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            user.UpdatedAt = DateTime.UtcNow;
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> AccessFailedAsync(ApplicationUser user)
        {
            return await _userManager.AccessFailedAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> IsLockedOutAsync(ApplicationUser user)
        {
            return await _userManager.IsLockedOutAsync(user);
        }

        public async Task<IdentityResult> ResetAccessFailedCountAsync(ApplicationUser user)
        {
            return await _userManager.ResetAccessFailedCountAsync(user);
        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            return await _userManager.GetClaimsAsync(user);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            await _context.SaveChangesAsync();
        }
    }
}
