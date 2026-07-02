namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class SalaryRecord
    {
        public Guid SalaryRecordId { get; set; }

        public Guid AssistantId { get; set; }

        public Guid PageTaskId { get; set; }

        public int Pages { get; set; }

        public decimal RateAtApproval { get; set; }

        public decimal Amount { get; set; }

        public DateTime ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public User Assistant { get; set; } = null!;

        public PageTask PageTask { get; set; } = null!;
    }
}
