using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class FileAsset
    {
        public Guid FileAssetId { get; set; }

        public string BucketName { get; set; } = null!;

        public string ObjectPath { get; set; } = null!;

        public string OriginalFileName { get; set; } = null!;

        public string StoredFileName { get; set; } = null!;

        public string MimeType { get; set; } = null!;

        public long FileSizeBytes { get; set; }

        public string FileCategory { get; set; } = null!;

        public Guid UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public User Uploader { get; set; } = null!;
    }
}
