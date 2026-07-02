namespace MangaManagementSystem.Business.DTOs.Responses.SalaryRecords
{
    public class SalaryRecordResponse
    {
        public Guid SalaryRecordId { get; set; }

        public Guid AssistantId { get; set; }

        public string? AssistantName { get; set; }

        public Guid PageTaskId { get; set; }

        public string? TaskType { get; set; }

        public int PageStart { get; set; }

        public int PageEnd { get; set; }

        public int Pages { get; set; }

        public decimal RateAtApproval { get; set; }

        public decimal Amount { get; set; }

        public DateTime ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
