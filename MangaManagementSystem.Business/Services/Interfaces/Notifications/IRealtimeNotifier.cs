using MangaManagementSystem.Business.DTOs.Requests.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Notifications
{
    public interface IRealtimeNotifier
    {
        Task NotifyUsersAsync(IEnumerable<Guid> userIds, RealtimeNotificationPayload payload);
        Task NotifyRoleAsync(string roleName, RealtimeNotificationPayload payload);
    }
}
