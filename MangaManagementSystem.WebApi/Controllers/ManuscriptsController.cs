using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.Business.Manuscripts.DTOs;
using MangaManagementSystem.Business.Manuscripts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.WebApi.Controllers
{
    /// <summary>
    /// API endpoints cho Manuscript feature (Manuscript Submission + Editor Review + Chapter Publishing).
    ///
    /// Auth hiện tại dùng DevCurrentUserService — bỏ qua hoàn toàn khi test.
    /// Teammate implement JWT: tạo JwtCurrentUserService và đổi DI trong ServiceCollection.cs.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ManuscriptsController : ControllerBase
    {
        private readonly IManuscriptService _manuscriptService;
        private readonly ICurrentUserService _currentUserService;

        public ManuscriptsController(
            IManuscriptService manuscriptService,
            ICurrentUserService currentUserService)
        {
            _manuscriptService = manuscriptService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// POST /api/chapters/{chapterId}/manuscripts
        ///
        /// Mangaka submit manuscript mới hoặc resubmit (tạo version mới).
        /// Enforce BR-67 (tất cả PageTask phải Approved), BR-72 (chỉ Mangaka owner series),
        /// BR-73 (versioning), BR-80 (không submit nếu đã Approved).
        /// Trả về 201 Created với manuscript vừa tạo.
        /// </summary>
        [HttpPost("chapters/{chapterId:guid}/manuscripts")]
        public async Task<IActionResult> SubmitManuscript(
            [FromRoute] Guid chapterId,
            [FromBody] SubmitManuscriptRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.SubmitAsync(
                    chapterId, currentUserId.Value, request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetManuscriptById),
                    new { manuscriptId = result.ManuscriptId },
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
        /// GET /api/chapters/{chapterId}/manuscripts
        ///
        /// Lấy danh sách tất cả manuscript versions của một chapter (history).
        /// Mangaka owner, Tantou Editor assigned, hoặc Admin mới được xem.
        /// Trả về list sắp xếp theo VersionNo tăng dần.
        /// </summary>
        [HttpGet("chapters/{chapterId:guid}/manuscripts")]
        public async Task<IActionResult> GetManuscriptsByChapter(
            [FromRoute] Guid chapterId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var manuscripts = await _manuscriptService.GetListByChapterAsync(
                    chapterId, currentUserId.Value, cancellationToken);

                return Ok(new { manuscripts });
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
        /// GET /api/manuscripts/{manuscriptId}
        ///
        /// Lấy chi tiết một manuscript theo ID.
        /// Mangaka owner, Tantou Editor assigned, hoặc Admin mới được xem.
        /// </summary>
        [HttpGet("manuscripts/{manuscriptId:guid}")]
        public async Task<IActionResult> GetManuscriptById(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var manuscript = await _manuscriptService.GetByIdAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

                return Ok(manuscript);
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
        /// POST /api/manuscripts/{manuscriptId}/start-review
        ///
        /// Tantou Editor bắt đầu review — chuyển status Submitted → Under Review.
        /// Enforce BR-74 (assigned editor), BR-75 (latest version), BR-76 (đúng flow).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/start-review")]
        public async Task<IActionResult> StartReview(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.StartReviewAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

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
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/approve
        ///
        /// Tantou Editor approve manuscript — chuyển status Under Review → Approved.
        /// Đồng thời publish chapter (Chapter.Status = "Published").
        /// Enforce BR-74, BR-75, BR-80 (lock), BR-84 (completion 100%).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/approve")]
        public async Task<IActionResult> ApproveManuscript(
            [FromRoute] Guid manuscriptId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.ApproveAsync(
                    manuscriptId, currentUserId.Value, cancellationToken);

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
        }

        /// <summary>
        /// POST /api/manuscripts/{manuscriptId}/request-revision
        ///
        /// Tantou Editor yêu cầu sửa — chuyển status Under Review → Revision Required.
        /// Enforce BR-74, BR-75, BR-77 (cần annotation + feedback), BR-83 (max 3 rounds).
        /// </summary>
        [HttpPost("manuscripts/{manuscriptId:guid}/request-revision")]
        public async Task<IActionResult> RequestRevision(
            [FromRoute] Guid manuscriptId,
            [FromBody] RequestRevisionRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { message = "Chưa xác thực. Vui lòng đăng nhập." });

            try
            {
                var result = await _manuscriptService.RequestRevisionAsync(
                    manuscriptId, currentUserId.Value, request, cancellationToken);

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
    }
}
