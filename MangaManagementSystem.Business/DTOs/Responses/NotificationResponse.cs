namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class NotificationResponse
    {
        public Guid NotificationId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Link { get; set; }
        public string Priority { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
