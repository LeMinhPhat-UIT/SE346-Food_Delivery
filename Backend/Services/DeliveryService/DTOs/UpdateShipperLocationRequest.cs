namespace DeliveryService.DTOs
{
    public class UpdateShipperLocationRequest
    {
        public Guid? OrderId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
