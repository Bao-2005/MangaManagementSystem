namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}
