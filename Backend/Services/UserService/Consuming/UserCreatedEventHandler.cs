using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using UserService.Entities;
using UserService.Enums;
using UserService.Repositories.Interfaces;

namespace UserService.Consuming
{
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(IUserRepository userRepository, ILogger<UserCreatedEventHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task Handle(UserCreatedEvent @event)
        {
            var existingUser = await _userRepository.GetUserByIdAsync(@event.UserId);
            if (existingUser != null)
            {
                _logger.LogInformation("User {UserId} already exists in UserService", @event.UserId);
                return;
            }

            var user = new User
            {
                Id = @event.UserId,
                FullName = string.IsNullOrWhiteSpace(@event.FullName) ? string.Empty : @event.FullName,
                AvatarUrl = string.Empty,
                Status = UserStatus.PendingVerification,
                CreatedAt = DateTime.UtcNow,
                Addresses = new List<Address>()
            };

            await _userRepository.CreateUserAsync(user);
            _logger.LogInformation("Created pending user profile for user {UserId}", @event.UserId);
        }
    }
}
