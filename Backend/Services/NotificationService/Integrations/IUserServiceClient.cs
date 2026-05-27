namespace NotificationService.Integrations
{
    public interface IUserServiceClient
    {
        Task<Guid?> GetUserIdByShipperIdAsync(Guid shipperId, CancellationToken cancellationToken = default);
        Task<Guid?> GetShipperIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
