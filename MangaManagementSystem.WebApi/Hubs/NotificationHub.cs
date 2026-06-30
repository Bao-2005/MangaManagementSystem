using MangaManagementSystem.Business.Services.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MangaManagementSystem.WebApi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var role = Context.User!.FindFirst(ClaimTypes.Role)?.Value;

            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }

            if (role != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");
            }

            await base.OnConnectedAsync();
        }
    }
}
