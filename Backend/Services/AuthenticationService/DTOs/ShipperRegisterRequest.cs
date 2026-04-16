namespace AuthenticationService.DTOs
{
    public class ShipperRegisterRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;
        public string IdCardNumber { get; set; } = null!;
    }
}
