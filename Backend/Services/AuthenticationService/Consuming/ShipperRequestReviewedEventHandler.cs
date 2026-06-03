using AuthenticationService.Repositories.Interfaces;
using Messaging.Abstractions.Dispatching;
using Messaging.Contracts.Events;

namespace AuthenticationService.Consuming
{
    public class ShipperRequestReviewedEventHandler : IEventHandler<ShipperRequestReviewedEvent>
    {
        private const string ShipperRole = "Shipper";

        private readonly IAuthRepository _authRepository;
        private readonly ILogger<ShipperRequestReviewedEventHandler> _logger;

        public ShipperRequestReviewedEventHandler(
            IAuthRepository authRepository,
            ILogger<ShipperRequestReviewedEventHandler> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task Handle(ShipperRequestReviewedEvent @event)
        {
            if (!@event.IsApproved)
                return;

            var user = await _authRepository.FindByIdAsync(@event.UserId);
            if (user is null)
            {
                _logger.LogWarning("Cannot add Shipper role. User {UserId} was not found", @event.UserId);
                return;
            }

            var roles = await _authRepository.GetRolesAsync(user);
            if (roles.Any(role => string.Equals(role, ShipperRole, StringComparison.OrdinalIgnoreCase)))
                return;

            var result = await _authRepository.AddToRoleAsync(user, ShipperRole);
            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Cannot add Shipper role to user {UserId}: {Errors}",
                    @event.UserId,
                    string.Join("; ", result.Errors.Select(error => error.Description)));
                return;
            }

            _logger.LogInformation("Added Shipper role to user {UserId}", @event.UserId);
        }
    }
}
