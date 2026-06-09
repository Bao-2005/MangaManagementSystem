using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.DTOs.Responses.Auth
{
    public class LoginDataDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserDto User { get; set; } = null!;
    }
}
