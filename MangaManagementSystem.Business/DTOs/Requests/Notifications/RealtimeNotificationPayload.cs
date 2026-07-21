namespace MangaManagementSystem.Business.DTOs.Requests.Notifications
{
    public class RealtimeNotificationPayload
    {
        public Guid NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
