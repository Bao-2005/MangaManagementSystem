using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
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
