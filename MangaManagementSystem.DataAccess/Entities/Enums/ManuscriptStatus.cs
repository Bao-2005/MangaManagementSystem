using System;

namespace MangaManagementSystem.DataAccess.Entities.Enums
{
    public enum ManuscriptStatus
    {
        Draft,
        Submitted,
        UnderReview,
        RevisionRequired,
        Approved
    }

    public static class ManuscriptStatusExtensions
    {
        public static string ToStorageValue(this ManuscriptStatus status)
        {
            return status switch
            {
                ManuscriptStatus.Draft => "DRAFT",
                ManuscriptStatus.Submitted => "SUBMITTED",
                ManuscriptStatus.UnderReview => "UNDER_REVIEW",
                ManuscriptStatus.RevisionRequired => "REVISION_REQUIRED",
                ManuscriptStatus.Approved => "APPROVED",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
