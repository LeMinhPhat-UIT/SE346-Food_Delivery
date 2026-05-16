using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Persistences;
using UserService.Repositories.Interfaces;

namespace UserService.Repositories.Implements
{
    public partial class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user != null && user.DeletedAt == null)
            {
                user.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IQueryable<User>> GetAllUserAsync()
        {
            return _context.Users.Where(u => u.DeletedAt == null);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CreateUserAddressAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<Address?> GetUserAddressByIdAsync(Guid addressId)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.DeletedAt == null);
        }

        public async Task UpdateUserAddressAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteUserAddressAsync(Guid addressId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.DeletedAt == null);

            if (address == null)
                return false;

            address.DeletedAt = DateTime.UtcNow;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IQueryable<Address>?> GetAllUserAddressesAsync()
        {
            return _context.Addresses
                .Where(a => a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt);
        }

        public async Task<IQueryable<Address>?> GetAllUserAddressesByUserIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Addresses
                .Where(a => a.DeletedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();
        }
    }
}
