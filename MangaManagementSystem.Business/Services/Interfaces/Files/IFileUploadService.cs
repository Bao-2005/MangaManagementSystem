using MangaManagementSystem.Business.DTOs.Requests.Files;
using MangaManagementSystem.Business.DTOs.Responses.Files;

namespace MangaManagementSystem.Business.Services.Interfaces.Files
{
    public interface IFileUploadService
    {
        Task<FileUploadResponse> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
        Task<FileAssetResponse?> GetByIdAsync(Guid fileAssetId, CancellationToken cancellationToken = default);
    }
}
