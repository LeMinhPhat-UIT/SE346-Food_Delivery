namespace AuthenticationService.DTOs
{
    public class CustomerRegisterRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
