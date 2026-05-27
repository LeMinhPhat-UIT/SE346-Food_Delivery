namespace DeliveryService.Integrations
{
    public interface IUserServiceClient
    {
        Task<Guid?> GetShipperIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
