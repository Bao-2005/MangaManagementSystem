using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
{
    public interface ISeriesService
    {
        Task<IEnumerable<SeriesResponse>> GetAllAsync(string? status = null);
        Task<SeriesDetailResponse> GetByIdAsync(Guid id);
        Task<IEnumerable<SeriesResponse>> GetByMangakaAsync(Guid mangakaId);
        Task<SeriesResponse> CreateAsync(Guid mangakaId, CreateSeriesRequest request);
        Task<SeriesResponse> UpdateAsync(Guid id, UpdateSeriesRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
