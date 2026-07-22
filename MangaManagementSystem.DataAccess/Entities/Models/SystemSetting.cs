namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class SystemSetting
    {
        public Guid SystemSettingId { get; set; }

        public string Key { get; set; } = null!;

        public string Value { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
