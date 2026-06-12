using MangaManagementSystem.DataAccess.Entities.Models;

namespace MangaManagementSystem.DataAccess.Repositories.Interfaces
{
    public interface IEscalationRepository : IRepository<Escalation>
    {
        Task<bool> SeriesExistsAsync(Guid seriesId, CancellationToken ct = default);

        Task<bool> EntityBelongsToSeriesAsync(
            string entityType,
            Guid entityId,
            Guid seriesId,
            CancellationToken ct = default);
    }
}
