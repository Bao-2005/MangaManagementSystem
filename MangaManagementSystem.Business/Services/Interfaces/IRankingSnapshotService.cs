using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface IRankingSnapshotService
    {
        Task<IEnumerable<RankingSnapshotResponse>> GetAllByPeriodAsync(string? period = null);
        Task<IEnumerable<RankingSnapshotResponse>> GetBySeriesAsync(Guid seriesId);
        Task SoftDeleteAsync(Guid id);
    }
}
