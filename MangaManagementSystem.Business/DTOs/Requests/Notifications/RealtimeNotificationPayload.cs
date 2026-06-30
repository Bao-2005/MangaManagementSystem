using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Requests.Notifications
{
    public class RealtimeNotificationPayload
    {
        public Guid NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string? Link { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
