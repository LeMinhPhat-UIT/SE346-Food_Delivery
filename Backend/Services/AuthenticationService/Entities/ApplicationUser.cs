using AuthenticationService.Enums;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsOtpVerified { get; set; } = false;
        public AuthStatus Status { get; set; } = AuthStatus.PendingVerification;
        public string? Otp { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<RefreshToken> RefreshTokens { get; set; } = null!;
    }
}
