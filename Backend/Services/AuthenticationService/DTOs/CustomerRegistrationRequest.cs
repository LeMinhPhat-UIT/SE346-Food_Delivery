using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class CustomerRegistrationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;
        
        public string FullName { get; set; } = string.Empty;
    }
}
