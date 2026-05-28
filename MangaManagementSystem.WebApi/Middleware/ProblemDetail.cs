namespace MangaManagementSystem.API.Middleware
{
    public class ProblemDetail
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public PathString Instance { get; set; }
    }
}
