using MangaManagementSystem.DataAccess.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class PageTaskSubmission
    {
        public Guid SubmissionId { get; set; }

        public Guid PageTaskId { get; set; }

        public int VersionNo { get; set; }

        public Guid SubmittedFileAssetId { get; set; }

        public PageTaskSubmissionStatus Status { get; set; } = PageTaskSubmissionStatus.Submitted;

        public string? Note { get; set; }

        public string? RejectReason { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public PageTask PageTask { get; set; } = null!;

        public FileAsset SubmittedFileAsset { get; set; } = null!;
    }
}
