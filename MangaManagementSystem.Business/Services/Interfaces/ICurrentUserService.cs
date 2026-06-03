namespace MangaManagementSystem.Business.Services.Interfaces
{
    /// <summary>
    /// Abstraction để lấy thông tin user đang gọi API.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Trả về UserId của người dùng hiện tại.
        /// Null nếu chưa xác thực.
        /// </summary>
        Guid? GetCurrentUserId();
    }
}
