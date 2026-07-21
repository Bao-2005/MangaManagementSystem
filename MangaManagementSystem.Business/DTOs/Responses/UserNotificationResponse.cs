namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class UserNotificationResponse
    {
        public Guid UserNotificationId { get; set; }
        public Guid NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
