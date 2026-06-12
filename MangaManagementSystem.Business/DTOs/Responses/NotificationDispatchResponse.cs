namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class NotificationDispatchResponse
    {
        public Guid? NotificationId { get; set; }
        public NotificationDispatchStatus Status { get; set; }
        public List<Guid> DeliveredUserIds { get; set; } = new();
        public List<Guid> SkippedMissingUserIds { get; set; } = new();
        public List<Guid> SkippedInactiveUserIds { get; set; } = new();
        public int RequestedRecipientCount { get; set; }
        public int DeliveredRecipientCount { get; set; }
        public string Message { get; set; } = null!;
    }
}
