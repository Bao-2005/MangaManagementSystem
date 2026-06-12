using AutoMapper;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class EscalationService : IEscalationService
    {
        private const string OpenStatus = "Open";
        private const string InReviewStatus = "InReview";
        private const string ResolvedStatus = "Resolved";
        private const string DuplicateEscalationIndexName = "IX_Escalations_Type_EntityType_EntityId";

        private static readonly string[] AllowedEntityTypes =
        {
            "Series",
            "Chapter",
            "Manuscript",
            "PageTask",
            "PageTaskSubmission",
            "BoardDecision"
        };

        private static readonly string[] AllowedPriorities =
        {
            "Low",
            "Normal",
            "High",
            "Critical"
        };

        private readonly IEscalationRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotificationDispatchService _notificationDispatchService;
        private readonly ILogger<EscalationService> _logger;

        public EscalationService(
            IEscalationRepository repo,
            IMapper mapper,
            INotificationDispatchService notificationDispatchService,
            ILogger<EscalationService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _notificationDispatchService = notificationDispatchService;
            _logger = logger;
        }

        public async Task<IEnumerable<EscalationResponse>> GetBySeriesAsync(Guid seriesId)
        {
            var entities = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .Where(e => e.SeriesId == seriesId && e.DeletedAt == null)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return _mapper.Map<IEnumerable<EscalationResponse>>(entities);
        }

        public async Task<EscalationResponse> GetByIdAsync(Guid id)
        {
            var e = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            return _mapper.Map<EscalationResponse>(e);
        }

        public async Task<EscalationResponse> CreateAsync(Guid createdByUserId, CreateEscalationRequest request)
        {
            var type = RequireText(request.Type, "Type");
            var entityType = NormalizeAllowedValue(request.EntityType, AllowedEntityTypes, "EntityType");
            var priority = NormalizeAllowedValue(request.Priority, AllowedPriorities, "Priority");
            var reason = RequireText(request.Reason, "Reason");

            if (request.EntityId == Guid.Empty)
                throw new ArgumentException("EntityId is required.");
            if (request.SeriesId == Guid.Empty)
                throw new ArgumentException("SeriesId is required.");

            await EnsureEntityBelongsToSeriesAsync(entityType, request.EntityId, request.SeriesId);

            var duplicateExists = await _repo.GetAll()
                .AnyAsync(e => e.Type == type
                    && e.EntityType == entityType
                    && e.EntityId == request.EntityId
                    && (e.Status == OpenStatus || e.Status == InReviewStatus)
                    && e.DeletedAt == null);
            if (duplicateExists)
                throw new InvalidOperationException("An open escalation already exists for this issue.");

            var esc = new Escalation
            {
                Type = type,
                EntityType = entityType,
                EntityId = request.EntityId,
                SeriesId = request.SeriesId,
                Priority = priority,
                Reason = reason,
                Status = OpenStatus,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(esc);
            try
            {
                await _repo.SaveChangeAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateEscalationConflict(ex))
            {
                throw new InvalidOperationException("An open escalation already exists for this issue.", ex);
            }

            await NotifyResolversAsync(esc);
            return await GetByIdAsync(esc.EscalationId);
        }

        public async Task<EscalationResponse> ResolveAsync(Guid id, Guid resolverUserId, UpdateEscalationRequest request)
        {
            var e = await _repo.GetAll()
                .Include(e => e.Creator)
                .Include(e => e.Resolver)
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");

            if (e.Status != OpenStatus && e.Status != InReviewStatus)
                throw new InvalidOperationException("Only open or in-review escalations can be resolved.");

            if (!string.IsNullOrWhiteSpace(request.Status)
                && !request.Status.Equals(ResolvedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Resolve endpoint only accepts status 'Resolved'.");
            }

            var resolution = RequireText(request.Resolution, "Resolution");
            e.Status = ResolvedStatus;
            e.Resolution = resolution;
            e.ResolvedBy = resolverUserId;
            e.ResolvedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
            return await GetByIdAsync(id);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var e = await _repo.GetAll()
                .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Escalation not found.");
            e.DeletedAt = DateTime.UtcNow;
            _repo.Update(e);
            await _repo.SaveChangeAsync();
        }

        private async Task EnsureEntityBelongsToSeriesAsync(string entityType, Guid entityId, Guid seriesId)
        {
            var seriesExists = await _repo.SeriesExistsAsync(seriesId);
            if (!seriesExists)
                throw new KeyNotFoundException("Series not found.");

            var belongsToSeries = await _repo.EntityBelongsToSeriesAsync(entityType, entityId, seriesId);
            if (!belongsToSeries)
                throw new ArgumentException($"{entityType} was not found in the specified series.");
        }

        private async Task NotifyResolversAsync(Escalation escalation)
        {
            var request = new NotificationDispatchRequest
            {
                Title = "New escalation requires review",
                Message = $"{escalation.EntityType} issue was escalated: {escalation.Reason}",
                Type = "EscalationRaised",
                Link = $"/api/escalations/{escalation.EscalationId}",
                Priority = escalation.Priority
            };

            try
            {
                await _notificationDispatchService.DispatchToRoleAsync(
                    request,
                    UserRole.EditorInChief.ToString());
                await _notificationDispatchService.DispatchToRoleAsync(
                    request,
                    UserRole.Admin.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Escalation {EscalationId} was created, but resolver notification dispatch failed.",
                    escalation.EscalationId);
            }
        }

        private static string RequireText(string? value, string fieldName)
        {
            var normalized = value?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException($"{fieldName} is required.");
            return normalized;
        }

        private static string NormalizeAllowedValue(
            string? value,
            IEnumerable<string> allowedValues,
            string fieldName)
        {
            var normalized = RequireText(value, fieldName);
            var canonical = allowedValues.FirstOrDefault(
                allowed => allowed.Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (canonical == null)
                throw new ArgumentException(
                    $"{fieldName} must be one of: {string.Join(", ", allowedValues)}.");
            return canonical;
        }

        private static bool IsDuplicateEscalationConflict(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                && string.Equals(
                    postgresException.ConstraintName,
                    DuplicateEscalationIndexName,
                    StringComparison.Ordinal);
        }
    }
}
