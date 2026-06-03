namespace MangaManagementSystem.DataAccess.Entities.Enums
{
    public enum UserRole
    {
        Mangaka,
        Assistant,
        TantouEditor,
        EditorialBoard,
        EditorInChief,
        Admin
    }

    public static class UserRoleExtensions
    {
        public static string ToStorageValue(this UserRole role)
        {
            return role switch
            {
                UserRole.Mangaka => "Mangaka",
                UserRole.Assistant => "Assistant",
                UserRole.TantouEditor => "Tantou Editor",
                UserRole.EditorialBoard => "Editorial Board",
                UserRole.EditorInChief => "Editor-in-Chief",
                UserRole.Admin => "Admin",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
