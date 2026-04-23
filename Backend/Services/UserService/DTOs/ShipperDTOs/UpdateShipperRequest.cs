using UserService.Enums;

namespace UserService.DTOs.ShipperDTOs
{
    public class UpdateShipperRequest
    {
        public string? VehiclePlate { get; set; } = null!;
        public ShipperStatus? Status { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
