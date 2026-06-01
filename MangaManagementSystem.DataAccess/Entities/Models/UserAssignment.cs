namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class UserAssignment
    {
        public Guid AssignmentId { get; set; }

        public Guid FromUserId { get; set; }

        public Guid ToUserId { get; set; }

        public bool Status { get; set; } = true;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UnassignedAt { get; set; }

        public User FromUser { get; set; } = null!;

        public User ToUser { get; set; } = null!;
    }
}
