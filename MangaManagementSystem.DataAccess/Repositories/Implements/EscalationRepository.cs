using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.DataAccess.Repositories.Implements
{
    public class EscalationRepository : Repository<Escalation>, IEscalationRepository
    {
        private readonly MangaDbContext _context;

        public EscalationRepository(MangaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> SeriesExistsAsync(Guid seriesId, CancellationToken ct = default)
        {
            return await _context.Series
                .AnyAsync(s => s.SeriesId == seriesId && s.DeletedAt == null, ct);
        }

        public async Task<bool> EntityBelongsToSeriesAsync(
            string entityType,
            Guid entityId,
            Guid seriesId,
            CancellationToken ct = default)
        {
            return entityType switch
            {
                "Series" => entityId == seriesId,

                "Chapter" => await _context.Chapters.AnyAsync(
                    c => c.ChapterId == entityId && c.SeriesId == seriesId && c.DeletedAt == null,
                    ct),

                "Manuscript" => await _context.Manuscripts.AnyAsync(
                    m => m.ManuscriptId == entityId
                        && m.Chapter.SeriesId == seriesId
                        && m.DeletedAt == null
                        && m.Chapter.DeletedAt == null,
                    ct),

                "PageTask" => await _context.PageTasks.AnyAsync(
                    t => t.PageTaskId == entityId
                        && t.Chapter.SeriesId == seriesId
                        && t.DeletedAt == null
                        && t.Chapter.DeletedAt == null,
                    ct),

                "PageTaskSubmission" => await _context.PageTaskSubmissions.AnyAsync(
                    s => s.SubmissionId == entityId
                        && s.PageTask.Chapter.SeriesId == seriesId
                        && s.DeletedAt == null
                        && s.PageTask.DeletedAt == null
                        && s.PageTask.Chapter.DeletedAt == null,
                    ct),

                "BoardDecision" => await _context.BoardDecisions.AnyAsync(
                    d => d.BoardDecisionId == entityId && d.SeriesId == seriesId && d.DeletedAt == null,
                    ct),

                _ => false
            };
        }
    }
}
