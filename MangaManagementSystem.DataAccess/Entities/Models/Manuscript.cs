using System;
using System.Collections.Generic;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Manuscript
    {
        public Guid ManuscriptId { get; set; }

        public Guid ChapterId { get; set; }

        public int VersionNo { get; set; }

        public Guid? PreviewFileAssetId { get; set; }

        public Guid? SourceFileAssetId { get; set; }

        public string Status { get; set; } = "Submitted";

        public string? Feedback { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi lại UserId của người đã submit bản thảo này (BR-72, BR-129).
        /// Required — mọi manuscript phải biết ai submit.
        /// </summary>
        public Guid SubmittedBy { get; set; }

        /// <summary>
        /// Ghi lại UserId của editor đã thực hiện review (audit).
        /// Nullable — chỉ có giá trị sau khi Editor bắt đầu review.
        /// </summary>
        public Guid? ReviewedBy { get; set; }

        /// <summary>
        /// Đếm số lần revision round đã thực hiện cho chapter này (BR-83).
        /// Tăng lên mỗi khi có Revision Required. Tối đa 3 rounds.
        /// Default 0.
        /// </summary>
        public int RevisionCount { get; set; } = 0;

        // Navigation properties
        public Chapter Chapter { get; set; } = null!;

        public FileAsset? PreviewFileAsset { get; set; }

        public FileAsset? SourceFileAsset { get; set; }

        /// <summary>Navigation tới User đã submit bản thảo này.</summary>
        public User Submitter { get; set; } = null!;

        /// <summary>Navigation tới Editor đã review (nullable).</summary>
        public User? Reviewer { get; set; }

        public ICollection<ChapterPage> ChapterPages { get; set; } = new List<ChapterPage>();

        public ICollection<PageTask> PageTasks { get; set; } = new List<PageTask>();

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}

