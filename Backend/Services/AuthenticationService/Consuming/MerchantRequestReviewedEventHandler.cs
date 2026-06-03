using AuthenticationService.Repositories.Interfaces;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;

namespace AuthenticationService.Consuming
{
    public class MerchantRequestReviewedEventHandler : IEventHandler<MerchantRequestReviewedEvent>
    {
        private const string MerchantRole = "Merchant";

        private readonly IAuthRepository _authRepository;
        private readonly ILogger<MerchantRequestReviewedEventHandler> _logger;

        public MerchantRequestReviewedEventHandler(
            IAuthRepository authRepository,
            ILogger<MerchantRequestReviewedEventHandler> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Handle(MerchantRequestReviewedEvent @event)
        {
            if (!@event.IsApproved)
                return;

            var user = await _authRepository.FindByIdAsync(@event.UserId);
            if (user is null)
            {
                _logger.LogWarning("Cannot add Merchant role. User {UserId} was not found", @event.UserId);
                return;
            }

            var roles = await _authRepository.GetRolesAsync(user);
            if (roles.Any(role => string.Equals(role, MerchantRole, StringComparison.OrdinalIgnoreCase)))
                return;

            var result = await _authRepository.AddToRoleAsync(user, MerchantRole);
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Cannot add Merchant role to user {UserId}: {Errors}",
                    @event.UserId,
                    string.Join("; ", result.Errors.Select(error => error.Description)));
                return;
            }

            _logger.LogInformation("Added Merchant role to user {UserId}", @event.UserId);
        }
    }
}
