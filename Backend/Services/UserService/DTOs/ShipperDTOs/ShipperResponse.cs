using UserService.Enums;

namespace UserService.DTOs.ShipperDTOs
{
    public class ShipperResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string VehiclePlate { get; set; } = null!;
        public ShipperStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
