using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<UserNotificationResponse>> GetMyNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid userNotificationId, Guid userId);
        Task<NotificationResponse> BroadcastAsync(CreateNotificationRequest request);
        Task SoftDeleteAsync(Guid notificationId);
    }
}
