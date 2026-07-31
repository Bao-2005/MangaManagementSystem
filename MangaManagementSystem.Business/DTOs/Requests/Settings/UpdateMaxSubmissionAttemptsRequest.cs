using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Settings
{
    public class UpdateMaxSubmissionAttemptsRequest
    {
        [Range(1, 20)]
        public int Value { get; set; }
    }
}
