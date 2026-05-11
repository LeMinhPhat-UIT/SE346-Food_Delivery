namespace DeliveryService.DTOs
{
    public class UpdateLocationRequest
    {
        public Guid OrderId { get; set; }
        public Guid ShipperId { get; set; }
        //public double Speed { get; set; }
        //public double Heading { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
