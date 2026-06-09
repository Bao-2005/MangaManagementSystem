namespace MangaManagementSystem.Business.DTOs.Responses.Series
{
    public class EscalationResponse
    {
        public Guid EscalationId { get; set; }
        public string Type { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public Guid EntityId { get; set; }
        public Guid SeriesId { get; set; }
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public string? Resolution { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatorName { get; set; } = null!;
        public Guid? ResolvedBy { get; set; }
        public string? ResolverName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
