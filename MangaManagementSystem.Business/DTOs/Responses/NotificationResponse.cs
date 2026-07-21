namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class NotificationResponse
    {
        public Guid NotificationId { get; set; }
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
