using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs
{
    public class UpdatePhoneNumberRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;
    }
}
