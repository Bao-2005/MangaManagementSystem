namespace MangaManagementSystem.Business.DTOs.Responses.Users
{
    public class UserAssignmentResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid FromUserId { get; set; }
        public string FromUserName { get; set; } = null!;
        public Guid ToUserId { get; set; }
        public string ToUserName { get; set; } = null!;
        public string ToUserEmail { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
    }
}
