using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IGenreService
    {
        Task<IEnumerable<GenreResponse>> GetAllAsync();
        Task<GenreResponse> GetByIdAsync(Guid id);
        Task<GenreResponse> CreateAsync(GenreRequest request);
        Task<GenreResponse> UpdateAsync(Guid id, GenreRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
