namespace UserService.Integrations
{
    public interface IAuthenticationServiceClient
    {
        Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
