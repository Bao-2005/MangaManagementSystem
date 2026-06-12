namespace MangaManagementSystem.Business.DTOs.Responses.Tasks
{
    public class SubmissionResponse
    {
        public Guid SubmissionId { get; set; }
        public Guid PageTaskId { get; set; }
        public int VersionNo { get; set; }
        public Guid SubmittedFileAssetId { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
