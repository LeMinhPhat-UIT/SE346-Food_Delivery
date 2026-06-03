using DeliveryService.Entities;
using DeliveryService.Enums;
using DeliveryService.Persistences;
using DeliveryService.Repositories;
using DeliveryService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DeliveryService.Repositories.Implements
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<DeliveryRepository> _logger;

        public DeliveryRepository(DeliveryDbContext context, ILogger<DeliveryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateShipperAvailabilityAsync(ShipperAvailability shipperAvailability)
        {
            await _context.ShipperAvailabilities.AddAsync(shipperAvailability);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<ShipperAvailability>> GetAllShipperAvailabilityAsync()
        {
            IQueryable<ShipperAvailability> query = _context.ShipperAvailabilities
                .AsNoTracking()
                .Where(sa => sa.DeletedAt == null);

            return Task.FromResult(query);
        }

        public async Task<ShipperAvailability?> GetShipperAvailabilityByShipperIdAsync(Guid shipperId)
        {
            return await _context.ShipperAvailabilities.FirstOrDefaultAsync(sa => sa.ShipperId == shipperId && sa.DeletedAt == null);
        }

        public async Task UpdateShipperAvailabilityAsync(ShipperAvailability shipperAvailability)
        {
            _context.ShipperAvailabilities.Update(shipperAvailability);
            await _context.SaveChangesAsync();
        }

        public async Task CreateShipperAssignment(ShipperAssignment shipperAssignment)
        {
            await _context.ShipperAssignments.AddAsync(shipperAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task CreateShipperAssignmentsAsync(IEnumerable<ShipperAssignment> shipperAssignments)
        {
            await _context.ShipperAssignments.AddRangeAsync(shipperAssignments);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TryCreateAssignmentOfferAsync(ShipperAssignment shipperAssignment, DateTime expiresAt)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                var now = DateTime.UtcNow;

                var affectedRows = await TryLockAvailabilityForOfferAsync(shipperAssignment, expiresAt, now);

                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                try
                {
                    shipperAssignment.Status = AssignmentStatus.Offering;
                    shipperAssignment.OfferExpiresAt = expiresAt;

                    await _context.ShipperAssignments.AddAsync(shipperAssignment);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(
                        ex,
                        "Failed to create assignment offer for order {OrderId} and shipper {ShipperId}",
                        shipperAssignment.OrderId,
                        shipperAssignment.ShipperId);
                    throw;
                }
                catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            });
        }

        public async Task<AssignmentAcceptanceResult> AcceptAssignmentOfferAsync(Guid assignmentId, Guid shipperId, DateTime now)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var existingAssignment = await _context.ShipperAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sa => sa.Id == assignmentId);

                if (existingAssignment == null)
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.NotFound);

                if (existingAssignment.ShipperId != shipperId)
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.OfferNotFound, existingAssignment);

                if (existingAssignment.OfferExpiresAt.HasValue && existingAssignment.OfferExpiresAt.Value <= now)
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.OfferExpired, existingAssignment);

                if (await _context.ShipperAssignments.AnyAsync(sa =>
                        sa.OrderId == existingAssignment.OrderId &&
                        sa.Id != assignmentId &&
                        sa.Status == AssignmentStatus.Accepted))
                {
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.AlreadyTaken, existingAssignment);
                }

                try
                {
                    var acceptedRows = await _context.ShipperAssignments
                        .Where(sa => sa.Id == assignmentId &&
                                     sa.ShipperId == shipperId &&
                                     (sa.Status == AssignmentStatus.Offering || sa.Status == AssignmentStatus.Pending) &&
                                     (!sa.OfferExpiresAt.HasValue || sa.OfferExpiresAt.Value > now))
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(sa => sa.Status, AssignmentStatus.Accepted)
                            .SetProperty(sa => sa.AcceptedAt, now)
                            .SetProperty(sa => sa.RespondedAt, now)
                            .SetProperty(sa => sa.RejectReason, (string?)null)
                            .SetProperty(sa => sa.CancelledReason, (string?)null));

                    if (acceptedRows == 0)
                    {
                        await transaction.RollbackAsync();
                        return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.NotAvailable, existingAssignment);
                    }

                    var cancelledAssignments = await _context.ShipperAssignments
                        .AsNoTracking()
                        .Where(sa => sa.OrderId == existingAssignment.OrderId &&
                                     sa.Id != assignmentId &&
                                     (sa.Status == AssignmentStatus.Offering || sa.Status == AssignmentStatus.Pending))
                        .ToListAsync();

                    var cancelledAssignmentIds = cancelledAssignments.Select(sa => sa.Id).ToArray();

                    if (cancelledAssignmentIds.Length > 0)
                    {
                        await _context.ShipperAssignments
                            .Where(sa => cancelledAssignmentIds.Contains(sa.Id))
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(sa => sa.Status, AssignmentStatus.Cancelled)
                                .SetProperty(sa => sa.RespondedAt, now)
                                .SetProperty(sa => sa.CancelledReason, "ACCEPTED_BY_ANOTHER_SHIPPER"));

                        await _context.ShipperAvailabilities
                            .Where(sa => sa.CurrentOfferedAssignmentId.HasValue &&
                                         cancelledAssignmentIds.Contains(sa.CurrentOfferedAssignmentId.Value) &&
                                         sa.Status == ShipperWorkStatus.Offering)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(sa => sa.Status, ShipperWorkStatus.ActiveIdle)
                                .SetProperty(sa => sa.CurrentOrderId, (Guid?)null)
                                .SetProperty(sa => sa.CurrentAssignmentId, (Guid?)null)
                                .SetProperty(sa => sa.CurrentOfferedAssignmentId, (Guid?)null)
                                .SetProperty(sa => sa.OfferingExpiresAt, (DateTime?)null)
                                .SetProperty(sa => sa.LastSeenAt, now));
                    }

                    var acceptedAvailabilityRows = await _context.ShipperAvailabilities
                        .Where(sa => sa.ShipperId == shipperId &&
                                     sa.Status == ShipperWorkStatus.Offering &&
                                     sa.CurrentOfferedAssignmentId == assignmentId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(sa => sa.Status, ShipperWorkStatus.Busy)
                            .SetProperty(sa => sa.CurrentOrderId, existingAssignment.OrderId)
                            .SetProperty(sa => sa.CurrentAssignmentId, assignmentId)
                            .SetProperty(sa => sa.CurrentOfferedAssignmentId, (Guid?)null)
                            .SetProperty(sa => sa.OfferingExpiresAt, (DateTime?)null)
                            .SetProperty(sa => sa.LastSeenAt, now));

                    if (acceptedAvailabilityRows == 0)
                    {
                        await transaction.RollbackAsync();
                        return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.NotAvailable, existingAssignment);
                    }

                    await transaction.CommitAsync();

                    var acceptedAssignment = await _context.ShipperAssignments
                        .AsNoTracking()
                        .FirstAsync(sa => sa.Id == assignmentId);

                    return new AssignmentAcceptanceResult
                    {
                        Outcome = AssignmentAcceptanceOutcome.Accepted,
                        Assignment = acceptedAssignment,
                        CancelledAssignments = cancelledAssignments
                    };
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.AlreadyTaken, existingAssignment);
                }
                catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    await transaction.RollbackAsync();
                    return AssignmentAcceptanceResult.FromOutcome(AssignmentAcceptanceOutcome.AlreadyTaken, existingAssignment);
                }
            });
        }

        public async Task<ShipperAssignment?> RejectAssignmentOfferAsync(Guid assignmentId, Guid shipperId, string reason, DateTime now)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var assignment = await _context.ShipperAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sa => sa.Id == assignmentId && sa.ShipperId == shipperId);

                if (assignment == null)
                    return null;

                var affectedRows = await _context.ShipperAssignments
                    .Where(sa => sa.Id == assignmentId &&
                                 sa.ShipperId == shipperId &&
                                 (sa.Status == AssignmentStatus.Offering || sa.Status == AssignmentStatus.Pending) &&
                                 (!sa.OfferExpiresAt.HasValue || sa.OfferExpiresAt.Value > now))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(sa => sa.Status, AssignmentStatus.Rejected)
                        .SetProperty(sa => sa.RespondedAt, now)
                        .SetProperty(sa => sa.RejectReason, reason)
                        .SetProperty(sa => sa.CancelledReason, (string?)null));

                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await _context.ShipperAvailabilities
                    .Where(sa => sa.ShipperId == shipperId &&
                                 sa.CurrentOfferedAssignmentId == assignmentId &&
                                 sa.Status == ShipperWorkStatus.Offering)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(sa => sa.Status, ShipperWorkStatus.ActiveIdle)
                        .SetProperty(sa => sa.CurrentOrderId, (Guid?)null)
                        .SetProperty(sa => sa.CurrentAssignmentId, (Guid?)null)
                        .SetProperty(sa => sa.CurrentOfferedAssignmentId, (Guid?)null)
                        .SetProperty(sa => sa.OfferingExpiresAt, (DateTime?)null)
                        .SetProperty(sa => sa.LastSeenAt, now));

                await transaction.CommitAsync();

                return await _context.ShipperAssignments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sa => sa.Id == assignmentId);
            });
        }

        public async Task<IReadOnlyList<ShipperAssignment>> ExpireStaleAssignmentOffersAsync(DateTime now, CancellationToken cancellationToken = default)
        {
            var expiringAssignments = await _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => (sa.Status == AssignmentStatus.Offering || sa.Status == AssignmentStatus.Pending) &&
                             sa.OfferExpiresAt.HasValue &&
                             sa.OfferExpiresAt.Value <= now)
                .ToListAsync(cancellationToken);

            if (expiringAssignments.Count == 0)
                return expiringAssignments;

            var assignmentIds = expiringAssignments.Select(sa => sa.Id).ToArray();

            await _context.ShipperAssignments
                .Where(sa => assignmentIds.Contains(sa.Id))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(sa => sa.Status, AssignmentStatus.Expired)
                    .SetProperty(sa => sa.RespondedAt, now)
                    .SetProperty(sa => sa.CancelledReason, "OFFER_EXPIRED"), cancellationToken);

            await _context.ShipperAvailabilities
                .Where(sa => sa.CurrentOfferedAssignmentId.HasValue &&
                             assignmentIds.Contains(sa.CurrentOfferedAssignmentId.Value) &&
                             sa.Status == ShipperWorkStatus.Offering)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(sa => sa.Status, ShipperWorkStatus.ActiveIdle)
                    .SetProperty(sa => sa.CurrentOrderId, (Guid?)null)
                    .SetProperty(sa => sa.CurrentAssignmentId, (Guid?)null)
                    .SetProperty(sa => sa.CurrentOfferedAssignmentId, (Guid?)null)
                    .SetProperty(sa => sa.OfferingExpiresAt, (DateTime?)null)
                    .SetProperty(sa => sa.LastSeenAt, now), cancellationToken);

            return expiringAssignments;
        }

        public async Task<ShipperAssignment?> GetActiveOfferForShipperAsync(Guid shipperId)
        {
            var now = DateTime.UtcNow;

            return await _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.ShipperId == shipperId &&
                             (sa.Status == AssignmentStatus.Offering || sa.Status == AssignmentStatus.Pending) &&
                             (!sa.OfferExpiresAt.HasValue || sa.OfferExpiresAt.Value > now))
                .OrderByDescending(sa => sa.AssignedAt)
                .FirstOrDefaultAsync();
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsAsync()
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.Status != AssignmentStatus.Cancelled)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<ShipperAssignment?> GetShipperAssignmentByIdAsync(Guid assignmentId)
        {
            return await _context.ShipperAssignments.FirstOrDefaultAsync(sa => sa.Id == assignmentId);
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByOrderIdAsync(Guid orderId)
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.OrderId == orderId)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public Task<IQueryable<ShipperAssignment>> GetAllShipperAssignmentsByShipperIdAsync(Guid shipperId)
        {
            IQueryable<ShipperAssignment> query = _context.ShipperAssignments
                .AsNoTracking()
                .Where(sa => sa.ShipperId == shipperId && sa.Status != AssignmentStatus.Cancelled)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<ShipperAssignment?> GetAcceptedShipperAssignmentByOrderIdAsync(Guid orderId)
        {
            return await _context.ShipperAssignments.FirstOrDefaultAsync(sa => sa.OrderId == orderId && sa.Status == AssignmentStatus.Accepted);
        }

        public async Task UpdateShipperAssignment(ShipperAssignment shipperAssignment)
        {
            _context.ShipperAssignments.Update(shipperAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task AddShipperLocationHistoriesAsync(IEnumerable<ShipperLocationHistory> histories, CancellationToken cancellationToken = default)
        {
            await _context.ShipperLocationHistories.AddRangeAsync(histories, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task<IQueryable<ShipperLocationHistory>> GetAllShipperLocationHistoriesByOrderIdAsync(Guid orderId)
        {
            IQueryable<ShipperLocationHistory> query = _context.ShipperLocationHistories
                .AsNoTracking()
                .Where(history => history.OrderId == orderId && history.DeletedAt == null)
                .OrderByDescending(history => history.RecordedAt)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public Task<IQueryable<ShipperLocationHistory>> GetAllShipperLocationHistoriesByShipperIdAsync(Guid shipperId)
        {
            IQueryable<ShipperLocationHistory> query = _context.ShipperLocationHistories
                .AsNoTracking()
                .Where(history => history.ShipperId == shipperId && history.DeletedAt == null)
                .OrderByDescending(history => history.RecordedAt)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task CreateIncidentAsync(Incident incident)
        {
            await _context.Incidents.AddAsync(incident);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<Incident>> GetAllIncidentsAsync()
        {
            IQueryable<Incident> query = _context.Incidents
                .AsNoTracking()
                .Where(i => i.DeletedAt == null)
                .OrderByDescending(i => i.CreatedAt)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public Task<IQueryable<Incident>> GetAllIncidentByReporterId(Guid reporterId)
        {
            IQueryable<Incident> query = _context.Incidents
                .AsNoTracking()
                .Where(i => i.ReportedBy == reporterId && i.DeletedAt == null)
                .OrderByDescending(i => i.CreatedAt)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<Incident?> GetIncidentByIdAsync(Guid incidentId)
        {
            return await _context.Incidents.FirstOrDefaultAsync(i => i.Id == incidentId && i.DeletedAt == null);
        }

        public async Task UpdateIncidentAsync(Incident incident)
        {
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<DeliveryFeePolicy>> GetAllDeliveryFeePoliciesAsync(bool includeInactive = true)
        {
            IQueryable<DeliveryFeePolicy> query = _context.DeliveryFeePolicies
                .AsNoTracking()
                .Include(policy => policy.DistanceTiers)
                .Where(policy => policy.DeletedAt == null);

            if (!includeInactive)
                query = query.Where(policy => policy.IsActive);

            query = query
                .OrderByDescending(policy => policy.CreatedAt)
                .ThenBy(policy => policy.Name)
                .AsQueryable();

            return Task.FromResult(query);
        }

        public async Task<IReadOnlyList<DeliveryFeePolicy>> GetActiveDeliveryFeePoliciesWithTiersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.DeliveryFeePolicies
                .AsNoTracking()
                .Include(policy => policy.DistanceTiers)
                .Where(policy => policy.IsActive && policy.DeletedAt == null)
                .OrderBy(policy => policy.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DeliveryFeePolicy?> GetDeliveryFeePolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return await _context.DeliveryFeePolicies
                .Include(policy => policy.DistanceTiers)
                .FirstOrDefaultAsync(policy => policy.Id == policyId && policy.DeletedAt == null, cancellationToken);
        }

        public async Task CreateDeliveryFeePolicyAsync(DeliveryFeePolicy policy, CancellationToken cancellationToken = default)
        {
            await _context.DeliveryFeePolicies.AddAsync(policy, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasDeliveryFeeQuoteDetailsForPolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return await _context.DeliveryFeeQuoteDetails
                .AnyAsync(detail => detail.PolicyId == policyId, cancellationToken);
        }

        public async Task ReplaceUsedDeliveryFeePolicyAsync(DeliveryFeePolicy usedPolicy, DeliveryFeePolicy replacementPolicy, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.DeliveryFeePolicies.Update(usedPolicy);
            await _context.DeliveryFeePolicies.AddAsync(replacementPolicy, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task UpdateDeliveryFeePolicyAsync(
            DeliveryFeePolicy policy,
            IEnumerable<DeliveryFeeDistanceTier> replacementTiers,
            CancellationToken cancellationToken = default)
        {
            var existingTiers = policy.DistanceTiers.ToList();
            _context.DeliveryFeeDistanceTiers.RemoveRange(existingTiers);

            policy.DistanceTiers = replacementTiers.ToList();
            await _context.DeliveryFeeDistanceTiers.AddRangeAsync(policy.DistanceTiers, cancellationToken);

            _context.DeliveryFeePolicies.Update(policy);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SoftDeleteDeliveryFeePolicyAsync(
            DeliveryFeePolicy policy,
            DateTime deletedAt,
            CancellationToken cancellationToken = default)
        {
            policy.IsActive = false;
            policy.DeletedAt = deletedAt;
            policy.UpdatedAt = deletedAt;

            _context.DeliveryFeePolicies.Update(policy);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateDeliveryFeeQuoteAsync(DeliveryFeeQuote quote, CancellationToken cancellationToken = default)
        {
            await _context.DeliveryFeeQuotes.AddAsync(quote, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<int> TryLockAvailabilityForOfferAsync(ShipperAssignment assignment, DateTime expiresAt, DateTime now)
        {
            return await _context.ShipperAvailabilities
                .Where(sa => sa.ShipperId == assignment.ShipperId &&
                             sa.Status == ShipperWorkStatus.ActiveIdle &&
                             sa.CurrentOrderId == null &&
                             sa.CurrentAssignmentId == null &&
                             sa.CurrentOfferedAssignmentId == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(sa => sa.Status, ShipperWorkStatus.Offering)
                    .SetProperty(sa => sa.CurrentOrderId, (Guid?)null)
                    .SetProperty(sa => sa.CurrentAssignmentId, (Guid?)null)
                    .SetProperty(sa => sa.CurrentOfferedAssignmentId, assignment.Id)
                    .SetProperty(sa => sa.OfferingExpiresAt, expiresAt)
                    .SetProperty(sa => sa.LastSeenAt, now));
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException postgresException &&
                   postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
        }
    }
}
