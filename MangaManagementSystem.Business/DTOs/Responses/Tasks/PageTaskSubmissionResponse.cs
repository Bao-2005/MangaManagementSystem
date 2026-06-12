using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaManagementSystem.DataAccess.Entities.Enums;

namespace MangaManagementSystem.Business.DTOs.Responses.Tasks;

public class PageTaskSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public Guid PageTaskId { get; set; }
    public int VersionNo { get; set; }
    public Guid SubmittedFileAssetId { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ObjectPath { get; set; }
    public PageTaskSubmissionStatus Status { get; set; }
    public string? Note { get; set; }
    public string? RejectReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

