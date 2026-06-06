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
        //public string StorageProvider { get; set; } = null!;
        public string BucketName { get; set; } = null!;
        public string ObjectPath { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string StoredFileName { get; set; } = null!;
        public string Extension { get; set; } = null!; //.zip, .png, ... -> luu duoi file -> doi khi extension va mimetype khong giong nhau nen luu le
        public long FileSizeBytes { get; set; }
        //public string FileType { get; set; } = null!;
        public string MimeType { get; set; } = null!; //image.png -> luu loai file hien thi
        public DateTime? DeletedAt { get; set; }

        public Series? SeriesSourceZip { get; set; }
        public ProposalPage? ProposalPagePreview { get; set; }
        public ICollection<PageTaskSubmission> PageTaskSubmissions { get; set; } = new List<PageTaskSubmission>();
    }
}
