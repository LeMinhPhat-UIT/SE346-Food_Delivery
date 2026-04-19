using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;
using UserService.Enums;
using UserService.Repositories.Interfaces;

namespace UserService.Consuming
{
    public class OtpVerifiedEventHandler : IEventHandler<OtpVerifiedEvent>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OtpVerifiedEventHandler> _logger;

        public OtpVerifiedEventHandler(IUserRepository userRepository, ILogger<OtpVerifiedEventHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task Handle(OtpVerifiedEvent @event)
        {
            var user = await _userRepository.GetUserByIdAsync(@event.UserId);
            if (user == null)
            {
                _logger.LogWarning("Received OtpVerifiedEvent but user {UserId} was not found", @event.UserId);
                return;
            }

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation("User {UserId} status updated to Active after OTP verification", @event.UserId);
        }
    }
}
