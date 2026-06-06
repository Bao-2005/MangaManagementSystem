namespace MangaManagementSystem.Business.DTOs.Responses
{
    public class VoteRecordResponse
    {
        public Guid VoteRecordId { get; set; }
        public Guid SeriesId { get; set; }
        public string Period { get; set; } = null!;
        public int ReaderCount { get; set; }
        public int VoteCount { get; set; }
        public string Status { get; set; } = null!;
        public Guid? ConfirmedBy { get; set; }
        public string? ConfirmerName { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
