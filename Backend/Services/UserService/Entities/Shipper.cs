using UserService.Commons;
using UserService.Enums;

namespace UserService.Entities
{
    public class Shipper : BaseAuditableEntity
    {
        public Guid UserId { get; set; }

        public string VehiclePlate { get; set; } = null!;

        public ShipperStatus Status { get; set; } = ShipperStatus.Pending;

        public Guid RequestId { get; set; }
        public ShipperRequest Request { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
