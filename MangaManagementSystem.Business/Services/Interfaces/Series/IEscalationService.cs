using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;

namespace MangaManagementSystem.Business.Services.Interfaces.Series
{
    public interface IEscalationService
    {
        Task<IEnumerable<EscalationResponse>> GetBySeriesAsync(Guid seriesId);
        Task<EscalationResponse> GetByIdAsync(Guid id);
        Task<EscalationResponse> CreateAsync(Guid createdByUserId, CreateEscalationRequest request);
        Task<EscalationResponse> ResolveAsync(Guid id, Guid resolverUserId, UpdateEscalationRequest request);
        Task SoftDeleteAsync(Guid id);
    }
}
