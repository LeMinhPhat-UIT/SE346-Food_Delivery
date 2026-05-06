using Messaging.Contracts.Common.Models;

namespace UserService.Entities
{
    public class Address : BaseAuditableEntity
    {
        public Guid UserId { get; set; }

        public string Label { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public string AddressLine { get; set; } = null!;
        public string Ward { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;

        public decimal? Lat { get; set; }
        public decimal? Lng { get; set; }

        public bool IsDefault { get; set; } = false;

        public User User { get; set; } = null!;
    }
}
