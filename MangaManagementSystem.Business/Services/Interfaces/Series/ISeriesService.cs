using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface ISeriesService
    {
        Task<IEnumerable<SeriesResponse>> GetAllAsync(string? status = null);
        Task<SeriesDetailResponse> GetByIdAsync(Guid id);
        Task<IEnumerable<SeriesResponse>> GetByMangakaAsync(Guid mangakaId);
        Task<SeriesDetailResponse> CreateAsync(Guid mangakaId, CreateSeriesRequest request);
        Task<SeriesDetailResponse> UpdateAsync(Guid id, Guid mangakaId, UpdateSeriesRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
