namespace NotificationService.Integrations
{
    public interface IUserServiceClient
    {
        Task<Guid?> GetUserIdByShipperIdAsync(Guid shipperId, CancellationToken cancellationToken = default);
    }
}
