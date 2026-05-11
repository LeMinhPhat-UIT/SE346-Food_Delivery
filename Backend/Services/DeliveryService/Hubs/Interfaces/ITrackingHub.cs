using DeliveryService.DTOs;

namespace DeliveryService.Hubs.Interfaces
{
    public interface ITrackingHub
    {
        Task ReceiveLocation(UpdateLocationRequest request);
        //Task UpdateLocation(UpdateLocationRequest request);
    }
}
