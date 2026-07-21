using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Requests.Notifications;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Notifications;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class NotificationDispatchService : INotificationDispatchService
    {
        private readonly IRepository<Notification> _notificationRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRealtimeNotifier _realtimeNotifier;
        private readonly ILogger<NotificationDispatchService> _logger;

        public NotificationDispatchService(
            IRepository<Notification> notificationRepo,
            IRepository<User> userRepo,
            IRealtimeNotifier realtimeNotifier,
            ILogger<NotificationDispatchService> logger)
        {
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
            _realtimeNotifier = realtimeNotifier;
            _logger = logger;
        }

        public async Task<NotificationDispatchResponse> DispatchToUsersAsync(
            NotificationDispatchRequest request,
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(userIds);
            ValidateRequest(request);

            var requestedUserIds = userIds
                .Distinct()
                .ToList();

            if (requestedUserIds.Count == 0)
            {
                return CreateNoRecipientsResponse("No recipient user IDs were provided.", 0);
            }

            var queryableUserIds = requestedUserIds
                .Where(userId => userId != Guid.Empty)
                .ToList();

            var users = await _userRepo.GetAll()
                .Where(user => queryableUserIds.Contains(user.UserId))
                .Select(user => new
                {
                    user.UserId,
                    IsActive = user.DeletedAt == null
                })
                .ToListAsync(cancellationToken);

            var resolvedUserIds = users.Select(user => user.UserId).ToHashSet();
            var activeUserIds = users
                .Where(user => user.IsActive)
                .Select(user => user.UserId)
                .ToList();

            var missingUserIds = requestedUserIds
                .Where(userId => !resolvedUserIds.Contains(userId))
                .ToList();

            var inactiveUserIds = users
                .Where(user => !user.IsActive)
                .Select(user => user.UserId)
                .ToList();

            if (activeUserIds.Count == 0)
            {
                return CreateNoRecipientsResponse(
                    "No active notification recipients were resolved.",
                    requestedUserIds.Count,
                    missingUserIds,
                    inactiveUserIds);
            }

            return await PersistDispatchAsync(
                request,
                activeUserIds,
                requestedUserIds.Count,
                missingUserIds,
                inactiveUserIds,
                cancellationToken);
        }

        public async Task<NotificationDispatchResponse> DispatchToRoleAsync(
            NotificationDispatchRequest request,
            string roleName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidateRequest(request);

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role name is required.", nameof(roleName));
            }

            var activeUserIds = await _userRepo.GetAll()
                .Include(user => user.Role)
                .Where(user =>
                    user.DeletedAt == null
                    && user.Role.DeletedAt == null
                    && user.Role.RoleName == roleName)
                .Select(user => user.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (activeUserIds.Count == 0)
            {
                return CreateNoRecipientsResponse($"No active users were found for role '{roleName}'.", 0);
            }

            return await PersistDispatchAsync(
                request,
                activeUserIds,
                activeUserIds.Count,
                Enumerable.Empty<Guid>(),
                Enumerable.Empty<Guid>(),
                cancellationToken);
        }

        private async Task<NotificationDispatchResponse> PersistDispatchAsync(
            NotificationDispatchRequest request,
            List<Guid> recipientUserIds,
            int requestedRecipientCount,
            IEnumerable<Guid> missingUserIds,
            IEnumerable<Guid> inactiveUserIds,
            CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var userId in recipientUserIds)
            {
                notification.UserNotifications.Add(new UserNotification
                {
                    UserId = userId,
                    Notification = notification,
                    IsRead = false
                });
            }

            await _notificationRepo.AddAsync(notification, cancellationToken);
            await _notificationRepo.SaveChangeAsync(cancellationToken);
            await TryPushRealtimeAsync(notification, recipientUserIds, cancellationToken);

            var skippedMissingUserIds = missingUserIds.ToList();
            var skippedInactiveUserIds = inactiveUserIds.ToList();

            return new NotificationDispatchResponse
            {
                NotificationId = notification.NotificationId,
                Status = skippedMissingUserIds.Count > 0 || skippedInactiveUserIds.Count > 0
                    ? NotificationDispatchStatus.Partial
                    : NotificationDispatchStatus.Sent,
                DeliveredUserIds = recipientUserIds,
                SkippedMissingUserIds = skippedMissingUserIds,
                SkippedInactiveUserIds = skippedInactiveUserIds,
                RequestedRecipientCount = requestedRecipientCount,
                DeliveredRecipientCount = recipientUserIds.Count,
                Message = "Notification dispatched."
            };
        }

        private static void ValidateRequest(NotificationDispatchRequest request)
        {
            ValidateRequiredText(request.Message, nameof(request.Message), 1000);
        }

        private async Task TryPushRealtimeAsync(
            Notification notification,
            IReadOnlyCollection<Guid> recipientUserIds,
            CancellationToken cancellationToken)
        {
            if (recipientUserIds.Count == 0)
            {
                return;
            }

            var payload = new RealtimeNotificationPayload
            {
                NotificationId = notification.NotificationId,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt
            };

            try
            {
                await _realtimeNotifier.NotifyUsersAsync(recipientUserIds, payload);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Notification {NotificationId} was persisted, but realtime push failed.",
                    notification.NotificationId);
            }
        }

        private static void ValidateRequiredText(string? value, string fieldName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{fieldName} is required.", fieldName);
            }

            if (value.Length > maxLength)
            {
                throw new ArgumentException($"{fieldName} must be {maxLength} characters or fewer.", fieldName);
            }
        }

        private static NotificationDispatchResponse CreateNoRecipientsResponse(
            string message,
            int requestedRecipientCount,
            IEnumerable<Guid>? missingUserIds = null,
            IEnumerable<Guid>? inactiveUserIds = null)
        {
            return new NotificationDispatchResponse
            {
                Status = NotificationDispatchStatus.NoRecipients,
                RequestedRecipientCount = requestedRecipientCount,
                DeliveredRecipientCount = 0,
                SkippedMissingUserIds = missingUserIds?.ToList() ?? new List<Guid>(),
                SkippedInactiveUserIds = inactiveUserIds?.ToList() ?? new List<Guid>(),
                Message = message
            };
        }
    }
}
