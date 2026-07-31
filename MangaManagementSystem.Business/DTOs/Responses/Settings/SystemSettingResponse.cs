namespace MangaManagementSystem.Business.DTOs.Responses.Settings
{
    public class SystemSettingResponse
    {
        public string Key { get; set; } = null!;

        public int Value { get; set; }

        public string? Description { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
