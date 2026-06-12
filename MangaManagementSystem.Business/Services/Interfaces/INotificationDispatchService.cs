using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface INotificationDispatchService
    {
        Task<NotificationDispatchResponse> DispatchToUsersAsync(
            NotificationDispatchRequest request,
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken = default);

        Task<NotificationDispatchResponse> DispatchToRoleAsync(
            NotificationDispatchRequest request,
            string roleName,
            CancellationToken cancellationToken = default);
    }
}
