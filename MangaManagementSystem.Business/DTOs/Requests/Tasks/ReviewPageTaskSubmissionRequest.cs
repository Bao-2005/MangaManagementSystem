using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks;

public class ReviewPageTaskSubmissionRequest
{
    [MaxLength(1000)]
    public string? Feedback { get; set; }
}

