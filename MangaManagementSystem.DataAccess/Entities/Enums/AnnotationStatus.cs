namespace MangaManagementSystem.DataAccess.Entities.Enums
{
    public enum AnnotationStatus
    {
        Active,
        Deleted
    }

    public static class AnnotationStatusExtensions
    {
        public static bool ToIsDeleted(this AnnotationStatus status)
        {
            return status == AnnotationStatus.Deleted;
        }

        public static AnnotationStatus ToLogicalStatus(this bool isDeleted)
        {
            return isDeleted ? AnnotationStatus.Deleted : AnnotationStatus.Active;
        }
    }
}
