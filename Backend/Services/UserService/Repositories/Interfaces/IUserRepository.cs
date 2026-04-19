using UserService.Entities;

namespace UserService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<User>> GetAllUserAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task CreateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task UpdateUserAsync(User user);
    }
}
