namespace MangaManagementSystem.Business.Auth.Interfaces
{
    /// <summary>
    /// Abstraction để lấy thông tin user đang gọi API.
    /// 
    /// Hiện tại dùng <see cref="DevCurrentUserService"/> — bỏ qua toàn bộ auth khi test.
    /// Teammate implement <c>JwtCurrentUserService</c> khi có JWT:
    ///   - GetCurrentUserId() đọc từ claim "sub"
    ///   - BypassAuthorization = false
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Trả về UserId của người dùng hiện tại.
        /// Null nếu chưa xác thực (chỉ xảy ra khi BypassAuthorization = false và chưa login).
        /// </summary>
        Guid? GetCurrentUserId();

        /// <summary>
        /// Nếu true → bỏ qua toàn bộ role check và permission check trong Business layer.
        /// 
        /// Dev/Test: true (DevCurrentUserService)
        /// Production: false (JwtCurrentUserService — teammate implement)
        /// </summary>
        bool BypassAuthorization { get; }
    }
}
