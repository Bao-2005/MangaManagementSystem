using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.WebApi.Controllers
{
    /// <summary>
    /// API endpoints cho Pin Annotation feature.
    /// 
    /// Auth hiện tại dùng DevCurrentUserService — bỏ qua hoàn toàn khi test.
    /// Teammate implement JWT: tạo JwtCurrentUserService và đổi DI trong ServiceCollection.cs.
    /// </summary>
    [ApiController]
    [Route("api/manuscripts/{manuscriptId:guid}/annotations")]
    [Tags("Annotations")]
    public class AnnotationsController : ControllerBase
    {
        private readonly IAnnotationService _annotationService;
        private readonly ICurrentUserService _currentUserService;

        public AnnotationsController(
            IAnnotationService annotationService,
            ICurrentUserService currentUserService)
        {
            _annotationService = annotationService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/annotations
        /// 
        /// Tạo Pin Annotation mới trên một trang của manuscript.
        /// Chỉ Tantou Editor được assign cho series mới được tạo.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create annotation",
            Description = "Tạo một ghi chú ghim (Pin Annotation) mới trên trang của bản thảo. Chỉ Tantou Editor được phân công phụ trách series mới có quyền thực hiện.")]
        public async Task<IActionResult> CreateAnnotation(
            [FromRoute] Guid manuscriptId,
            [FromBody] CreateAnnotationRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _annotationService.CreateAsync(
                    manuscriptId, currentUserId.Value, request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetAnnotations),
                    new { manuscriptId },
                    result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/manuscripts/{manuscriptId}/annotations?versionNo=N&amp;pageNo=N
        /// 
        /// Lấy danh sách annotation theo manuscript.
        /// - versionNo: không truyền = trả về latest version.
        /// - pageNo: không truyền = tất cả trang.
        /// Tantou Editor hoặc Mangaka owner của series mới được xem.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get annotations",
            Description = "Lấy danh sách ghi chú theo bản thảo. Hỗ trợ lọc theo số phiên bản (versionNo) và số trang (pageNo). Tantou Editor hoặc Mangaka sở hữu series mới có quyền xem.")]
        public async Task<IActionResult> GetAnnotations(
            [FromRoute] Guid manuscriptId,
            [FromQuery] int? versionNo = null,
            [FromQuery] int? pageNo = null,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var annotations = await _annotationService.GetAsync(
                    manuscriptId, currentUserId.Value, versionNo, pageNo, cancellationToken);

                return Ok(new { annotations });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/manuscripts/{manuscriptId}/annotations/count?versionNo=N
        /// 
        /// Đếm annotation theo manuscript version.
        /// Mục đích: Manuscript Review module gọi để enforce BR-77
        /// (Revision Required phải có ít nhất 1 annotation).
        /// </summary>
        [HttpGet("count")]
        [SwaggerOperation(
            Summary = "Count annotations",
            Description = "Đếm số lượng ghi chú theo phiên bản bản thảo. Phục vụ việc kiểm tra điều kiện chuyển trạng thái yêu cầu sửa đổi.")]
        public async Task<IActionResult> CountAnnotations(
            [FromRoute] Guid manuscriptId,
            [FromQuery] int? versionNo = null,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var count = await _annotationService.CountAsync(
                    manuscriptId, currentUserId.Value, versionNo, cancellationToken);

                return Ok(new { count });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// PATCH /api/manuscripts/{manuscriptId}/annotations/{annotationId}
        /// 
        /// Cập nhật position và/hoặc content của annotation.
        /// Chỉ author của annotation mới được sửa.
        /// Manuscript phải chưa Approved.
        /// Annotation phải thuộc latest version.
        /// </summary>
        [HttpPatch("{annotationId:guid}")]
        [SwaggerOperation(
            Summary = "Update annotation",
            Description = "Cập nhật vị trí và/hoặc nội dung của ghi chú. Chỉ tác giả của ghi chú mới được sửa. Bản thảo phải chưa được duyệt và ghi chú phải thuộc phiên bản mới nhất.")]
        public async Task<IActionResult> UpdateAnnotation(
            [FromRoute] Guid manuscriptId,
            [FromRoute] Guid annotationId,
            [FromBody] UpdateAnnotationRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _annotationService.UpdateAsync(
                    manuscriptId, annotationId, currentUserId.Value, request, cancellationToken);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE /api/manuscripts/{manuscriptId}/annotations/{annotationId}
        /// 
        /// Soft delete annotation (IsDeleted = true).
        /// Không hard delete — giữ lịch sử để audit (BR-08).
        /// Chỉ author của annotation mới được xóa.
        /// Manuscript phải chưa Approved.
        /// Annotation phải thuộc latest version.
        /// </summary>
        [HttpDelete("{annotationId:guid}")]
        [SwaggerOperation(
            Summary = "Delete annotation",
            Description = "Xóa mềm (soft delete) ghi chú. Chỉ người tạo ghi chú mới có quyền xóa. Bản thảo phải chưa được duyệt và ghi chú phải thuộc phiên bản mới nhất.")]
        public async Task<IActionResult> DeleteAnnotation(
            [FromRoute] Guid manuscriptId,
            [FromRoute] Guid annotationId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                await _annotationService.DeleteAsync(
                    manuscriptId, annotationId, currentUserId.Value, cancellationToken);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
