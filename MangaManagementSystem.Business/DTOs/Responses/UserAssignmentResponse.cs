namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class UserAssignmentResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid FromUserId { get; set; }
        public string FromUserName { get; set; } = null!;
        public Guid ToUserId { get; set; }
        public string ToUserName { get; set; } = null!;
        public string AssignmentType { get; set; } = null!;
        public bool Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
    }
}
