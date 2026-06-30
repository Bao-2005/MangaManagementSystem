using MangaManagementSystem.Business.DTOs.Requests.Notifications;
using MangaManagementSystem.Business.Services.Interfaces.Notifications;
using MangaManagementSystem.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MangaManagementSystem.WebApi.Notifications
{
    public sealed class SignalRNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;
        public SignalRNotifier (IHubContext<NotificationHub, INotificationClient> hub) => _hub = hub;
        public async Task NotifyUsersAsync(IEnumerable<Guid> userIds, RealtimeNotificationPayload payload)
        {
            var tasks = userIds.Select(id => 
                _hub.Clients.Group($"user-{id}").ReceiveNotification(payload));
            await Task.WhenAll(tasks);
        }

        public Task NotifyRoleAsync(string roleName, RealtimeNotificationPayload payload) => _hub.Clients.Group($"role-{roleName}").ReceiveNotification(payload);
    }
}
