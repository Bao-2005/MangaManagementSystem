namespace MangaManagementSystem.Business.DTOs.Requests.Files
{
    public static class FileUploadLimits
    {
        public const int MaxFilesPerRequest = 20;
        public const long MaxTotalUploadBytes = 100 * 1024 * 1024;
    }
}
