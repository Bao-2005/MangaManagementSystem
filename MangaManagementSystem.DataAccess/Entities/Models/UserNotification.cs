using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class UserNotification
    {
        public Guid UserNotificationId { get; set; }
        public Guid UserId { get; set; }
        public Guid NotificationId { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User User { get; set; } = null!;
        public Notification Notification { get; set; } = null!;
    }
}
