using MangaManagementSystem.Business.DTOs.Requests.Files;
using MangaManagementSystem.Business.Services.Interfaces.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Produces("application/json")]
    [Tags("Files")]
    public class FileController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(FileUploadLimits.MaxTotalUploadBytes)]
        [SwaggerOperation(Summary = "Upload one or more reusable file assets")]
        public async Task<IActionResult> Upload(
            [FromForm] FileUploadCategory category,
            [FromForm] List<IFormFile>? files,
            CancellationToken cancellationToken)
        {
            var request = new FileUploadRequest
            {
                Category = category,
                Files = (files ?? new List<IFormFile>()).Select(file => new UploadFileRequest
                {
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Content = file.OpenReadStream()
                }).ToList()
            };

            var result = await _fileUploadService.UploadAsync(request, cancellationToken);
            return Ok(new BaseResponse { Data = result, Message = "Files uploaded." });
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Retrieve file asset metadata and public URL by ID",
            Description = "Returns the FileAssetResponse including the Supabase public URL for preview or download.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
        {
            var asset = await _fileUploadService.GetByIdAsync(id, cancellationToken);
            if (asset is null)
                return NotFound(new BaseResponse { Message = "File not found." });
            return Ok(new BaseResponse { Data = asset, Message = "Success" });
        }
    }
}
