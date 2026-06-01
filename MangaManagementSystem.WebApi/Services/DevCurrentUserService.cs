using MangaManagementSystem.Business.Auth.Interfaces;

namespace MangaManagementSystem.WebApi.Services
{
    /// <summary>
    /// Implementation TẠM THỜI dùng cho môi trường DEV / TEST.
    /// 
    /// - Không cần JWT, không cần header X-User-Id.
    /// - Bỏ qua toàn bộ kiểm tra role và permission.
    /// - Dùng một DevUserId cố định để gán AuthorId khi tạo entity.
    /// 
    /// TODO (teammate): Khi implement JWT, tạo file JwtCurrentUserService.cs với:
    ///   public Guid? GetCurrentUserId()
    ///       => Guid.TryParse(httpContext.User.FindFirst("sub")?.Value, out var id) ? id : null;
    ///   public bool BypassAuthorization => false;
    /// Sau đó đổi DI registration trong ServiceCollection.cs:
    ///   services.AddScoped&lt;ICurrentUserService, JwtCurrentUserService&gt;();
    /// </summary>
    public class DevCurrentUserService : ICurrentUserService
    {
        /// <summary>
        /// GUID cố định dùng cho dev/test.
        /// Có thể seed DB với GUID này làm TantouEditorId / MangakaId nếu cần test object-level check.
        /// </summary>
        public static readonly Guid DevUserId = new("11111111-1111-1111-1111-111111111111");

        public Guid? GetCurrentUserId() => DevUserId;

        /// <summary>
        /// true = bỏ qua tất cả role check, object-level check trong Business layer.
        /// </summary>
        public bool BypassAuthorization => true;
    }
}
