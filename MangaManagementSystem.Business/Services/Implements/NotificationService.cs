using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notifRepo;
        private readonly IRepository<UserNotification> _userNotifRepo;

        public NotificationService(IRepository<Notification> notifRepo, IRepository<UserNotification> userNotifRepo)
        {
            _notifRepo = notifRepo;
            _userNotifRepo = userNotifRepo;
        }

        public async Task<IEnumerable<UserNotificationResponse>> GetMyNotificationsAsync(Guid userId)
            => await _userNotifRepo.GetAll()
                .Include(un => un.Notification)
                .Where(un => un.UserId == userId && un.DeletedAt == null && un.Notification.DeletedAt == null)
                .Select(un => new UserNotificationResponse
                {
                    UserNotificationId = un.UserNotificationId,
                    NotificationId = un.NotificationId,
                    Message = un.Notification.Message,
                    IsRead = un.IsRead,
                    ReadAt = un.ReadAt,
                    CreatedAt = un.Notification.CreatedAt
                }).ToListAsync();

        public async Task MarkAsReadAsync(Guid userNotificationId, Guid userId)
        {
            var un = await _userNotifRepo.GetAll()
                .FirstOrDefaultAsync(x => x.UserNotificationId == userNotificationId && x.UserId == userId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("Notification not found.");
            un.IsRead = true;
            un.ReadAt = DateTime.UtcNow;
            _userNotifRepo.Update(un);
            await _userNotifRepo.SaveChangeAsync();
        }

        public async Task<NotificationResponse> BroadcastAsync(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };
            await _notifRepo.AddAsync(notification);

            foreach (var userId in request.TargetUserIds)
                await _userNotifRepo.AddAsync(new UserNotification { UserId = userId, NotificationId = notification.NotificationId, IsRead = false });

            await _notifRepo.SaveChangeAsync();
            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task SoftDeleteAsync(Guid notificationId)
        {
            var n = await _notifRepo.GetAll().FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Notification not found.");
            n.DeletedAt = DateTime.UtcNow;
            _notifRepo.Update(n);
            await _notifRepo.SaveChangeAsync();
        }
    }
}
