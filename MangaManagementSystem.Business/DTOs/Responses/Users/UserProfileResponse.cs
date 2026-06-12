namespace MangaManagementSystem.Business.DTOs.Responses.Users
{
    public class UserProfileResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public Guid? AssignedEditorId { get; set; }
        public string? AssignedEditorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
