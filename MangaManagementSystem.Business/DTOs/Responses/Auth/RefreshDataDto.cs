namespace MangaManagementSystem.Business.DTOs.Responses.Auth
{
    public class RefreshDataDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
