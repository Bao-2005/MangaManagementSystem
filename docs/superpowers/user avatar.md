# User Avatar Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add authenticated user avatar upload, persist the avatar as a `FileAsset`, store it in the existing general/default Supabase bucket, and expose the avatar URL on user profile responses.

**Architecture:** Keep the existing Controller -> Business service -> Repository / DataAccess layering. Reuse `IStorageService`, `IFileUploadService`, and `FileAsset`; add a nullable one-to-one `Users.AvatarFileAssetId` relationship for the selected avatar, where one user has at most one avatar and one `FileAsset` can be the avatar for at most one user. Do not add a separate avatar table or dedicated avatar bucket.

**Tech Stack:** .NET 8, ASP.NET Core Web API multipart upload, EF Core 8 migrations, PostgreSQL via Npgsql, Supabase Storage.

---

## File Structure

- Modify: `MangaManagementSystem.DataAccess/Entities/Models/User.cs`
  - Add nullable `AvatarFileAssetId` and `AvatarFileAsset` navigation.
- Modify: `MangaManagementSystem.DataAccess/Entities/Models/FileAsset.cs`
  - Add inverse `AvatarUser` navigation for the one-to-one relationship.
- Modify: `MangaManagementSystem.DataAccess/DbContext/MangaDbContext.cs`
  - Configure optional one-to-one User -> FileAsset avatar relationship.
- Generate: `MangaManagementSystem.DataAccess/Migrations/*_AddUserAvatar.cs`
  - Add `Users.AvatarFileAssetId`, unique filtered index, and FK.
- Modify: `MangaManagementSystem.Business/DTOs/Requests/Files/FileUploadCategory.cs`
  - Add `UserAvatar`.
- Modify: `MangaManagementSystem.Business/Services/Implements/Files/FileUploadService.cs`
  - Add image-only avatar validation and rely on default bucket fallback.
- Create: `MangaManagementSystem.Business/DTOs/Requests/Users/UpdateMyAvatarRequest.cs`
  - Service-layer upload DTO.
- Modify: `MangaManagementSystem.Business/DTOs/Responses/Users/UserProfileResponse.cs`
  - Add `AvatarFileAssetId` and `AvatarUrl`.
- Modify: `MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`
  - Add `UpdateMyAvatarAsync`.
- Modify: `MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`
  - Upload avatar, link avatar asset, soft-delete replaced avatar, map avatar URL.
- Modify: `MangaManagementSystem.WebApi/Controllers/UserController.cs`
  - Add `POST api/users/me/avatar`.
- Modify: `MangaManagementSystem.WebApi/docs/AGENTS.md`
  - Update guide notes after implementation, not with this plan text.

## Task 1: Add User Avatar Persistence

**Files:**
- Modify: `MangaManagementSystem.DataAccess/Entities/Models/User.cs`
- Modify: `MangaManagementSystem.DataAccess/Entities/Models/FileAsset.cs`
- Modify: `MangaManagementSystem.DataAccess/DbContext/MangaDbContext.cs`
- Generate: `MangaManagementSystem.DataAccess/Migrations/*_AddUserAvatar.cs`
- Generate: `MangaManagementSystem.DataAccess/Migrations/*_AddUserAvatar.Designer.cs`
- Modify generated: `MangaManagementSystem.DataAccess/Migrations/MangaDbContextModelSnapshot.cs`

- [ ] **Step 1: Add avatar fields to `User`**

Add this property after `RoleId`:

```csharp
public Guid? AvatarFileAssetId { get; set; }
```

Add this navigation after `Role`:

```csharp
public FileAsset? AvatarFileAsset { get; set; }
```

- [ ] **Step 2: Add inverse one-to-one navigation to `FileAsset`**

Add this navigation beside the other navigations:

```csharp
public User? AvatarUser { get; set; }
```

- [ ] **Step 3: Configure the EF relationship**

In `ConfigureUsers` in `MangaDbContext.cs`, after the email index, add:

```csharp
entity.HasIndex(x => x.AvatarFileAssetId)
    .IsUnique()
    .HasFilter("\"AvatarFileAssetId\" IS NOT NULL");
```

After the `Role` relationship, add:

```csharp
entity.HasOne(x => x.AvatarFileAsset)
    .WithOne(x => x.AvatarUser)
    .HasForeignKey<User>(x => x.AvatarFileAssetId)
    .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **Step 4: Build before migration**

Run:

```bash
dotnet build MangaManagementSystem.sln
```

Expected: build succeeds.

- [ ] **Step 5: Generate migration**

Run:

```bash
dotnet ef migrations add AddUserAvatar --project MangaManagementSystem.DataAccess --startup-project MangaManagementSystem.WebApi
```

Expected: EF creates `AddUserAvatar` migration files and updates the snapshot.

- [ ] **Step 6: Verify migration content**

Confirm the generated `Up` method includes:

```csharp
migrationBuilder.AddColumn<Guid>(
    name: "AvatarFileAssetId",
    table: "Users",
    type: "uuid",
    nullable: true);

migrationBuilder.CreateIndex(
    name: "IX_Users_AvatarFileAssetId",
    table: "Users",
    column: "AvatarFileAssetId",
    unique: true,
    filter: "\"AvatarFileAssetId\" IS NOT NULL");

migrationBuilder.AddForeignKey(
    name: "FK_Users_FileAssets_AvatarFileAssetId",
    table: "Users",
    column: "AvatarFileAssetId",
    principalTable: "FileAssets",
    principalColumn: "FileAssetId",
    onDelete: ReferentialAction.Restrict);
```

Confirm `Down` drops the FK, index, and column.

- [ ] **Step 7: Commit**

Run:

```bash
git add MangaManagementSystem.DataAccess/Entities/Models/User.cs MangaManagementSystem.DataAccess/Entities/Models/FileAsset.cs MangaManagementSystem.DataAccess/DbContext/MangaDbContext.cs MangaManagementSystem.DataAccess/Migrations
git commit -m "feat: add user avatar persistence"
```

## Task 2: Add Avatar Upload Category and Validation

**Files:**
- Modify: `MangaManagementSystem.Business/DTOs/Requests/Files/FileUploadCategory.cs`
- Modify: `MangaManagementSystem.Business/Services/Implements/Files/FileUploadService.cs`
- Inspect: `MangaManagementSystem.WebApi/appsettings.json`

- [ ] **Step 1: Add `UserAvatar` category**

Update the enum:

```csharp
namespace MangaManagementSystem.Business.DTOs.Requests.Files
{
    public enum FileUploadCategory
    {
        Generic,
        ProposalSource,
        ProposalSamplePage,
        ChapterReference,
        TaskSubmission,
        UserAvatar
    }
}
```

- [ ] **Step 2: Add avatar validation rule**

In `FileUploadService.Rules`, add before `Generic`:

```csharp
[FileUploadCategory.UserAvatar] = new(
    5 * 1024 * 1024,
    RequireContentSignature: true,
    new Dictionary<string, string[]>
    {
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".png"] = new[] { "image/png" },
        [".webp"] = new[] { "image/webp" }
    }),
```

- [ ] **Step 3: Preserve default bucket behavior**

Inspect `MangaManagementSystem.WebApi/appsettings.json`. Do not add `UserAvatar` under `Supabase:Storage:Buckets`; `SupabaseStorageService.ResolveBucket` will then use `Supabase:Storage:DefaultBucket`.

Expected avatar object path format:

```text
useravatar/yyyy/MM/dd/<guid>.<extension>
```

- [ ] **Step 4: Build**

Run:

```bash
dotnet build MangaManagementSystem.sln
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

Run:

```bash
git add MangaManagementSystem.Business/DTOs/Requests/Files/FileUploadCategory.cs MangaManagementSystem.Business/Services/Implements/Files/FileUploadService.cs
git commit -m "feat: add avatar upload validation"
```

## Task 3: Add Avatar Profile Service Behavior

**Files:**
- Create: `MangaManagementSystem.Business/DTOs/Requests/Users/UpdateMyAvatarRequest.cs`
- Modify: `MangaManagementSystem.Business/DTOs/Responses/Users/UserProfileResponse.cs`
- Modify: `MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`
- Modify: `MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`

- [ ] **Step 1: Create avatar request DTO**

Create `UpdateMyAvatarRequest.cs`:

```csharp
namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class UpdateMyAvatarRequest
    {
        public string OriginalFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
        public Stream Content { get; set; } = null!;
    }
}
```

- [ ] **Step 2: Add response fields**

In `UserProfileResponse`, add:

```csharp
public Guid? AvatarFileAssetId { get; set; }
public string? AvatarUrl { get; set; }
```

- [ ] **Step 3: Add service contract**

In `IUserService`, add:

```csharp
Task<UserProfileResponse> UpdateMyAvatarAsync(Guid userId, UpdateMyAvatarRequest request, CancellationToken cancellationToken = default);
```

- [ ] **Step 4: Add `UserService` dependencies**

Add using statements:

```csharp
using MangaManagementSystem.Business.DTOs.Requests.Files;
using MangaManagementSystem.Business.Services.Interfaces.Files;
using Microsoft.Extensions.Configuration;
```

Add fields:

```csharp
private readonly IFileUploadService _fileUploadService;
private readonly string _supabaseUrl;
```

Update constructor parameters:

```csharp
IFileUploadService fileUploadService,
IConfiguration configuration
```

Add constructor assignments:

```csharp
_fileUploadService = fileUploadService;
_supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
```

- [ ] **Step 5: Include avatar file in profile query**

In `UserProfileQuery`, add:

```csharp
.Include(x => x.AvatarFileAsset)
```

immediately after `.Include(x => x.Role)`.

- [ ] **Step 6: Implement avatar update**

Add this method to `UserService`:

```csharp
public async Task<UserProfileResponse> UpdateMyAvatarAsync(
    Guid userId,
    UpdateMyAvatarRequest request,
    CancellationToken cancellationToken = default)
{
    var user = await _userRepository.GetAll()
        .Include(x => x.AvatarFileAsset)
        .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null, cancellationToken)
        ?? throw new KeyNotFoundException("User not found.");

    var previousAvatar = user.AvatarFileAsset;

    var upload = await _fileUploadService.UploadAsync(new FileUploadRequest
    {
        Category = FileUploadCategory.UserAvatar,
        Files = new List<UploadFileRequest>
        {
            new()
            {
                OriginalFileName = request.OriginalFileName,
                ContentType = request.ContentType,
                Length = request.Length,
                Content = request.Content
            }
        }
    }, cancellationToken);

    var uploadedAvatar = upload.Files.Single();
    user.AvatarFileAssetId = uploadedAvatar.FileAssetId;

    if (previousAvatar is not null)
    {
        previousAvatar.DeletedAt = DateTime.UtcNow;
    }

    await _userRepository.SaveChangeAsync(cancellationToken);
    return await GetByIdForAdminAsync(userId);
}
```

- [ ] **Step 7: Map avatar fields**

Change `MapProfile` from static to instance:

```csharp
private UserProfileResponse MapProfile(User user, AssignedEditorProjection? assignedEditor)
```

In its returned object, add:

```csharp
AvatarFileAssetId = user.AvatarFileAssetId,
AvatarUrl = BuildAvatarUrl(user.AvatarFileAsset),
```

Add helper:

```csharp
private string? BuildAvatarUrl(FileAsset? avatarFileAsset)
{
    if (avatarFileAsset is null || avatarFileAsset.DeletedAt != null || string.IsNullOrEmpty(_supabaseUrl))
        return null;

    return $"{_supabaseUrl}/storage/v1/object/public/{avatarFileAsset.BucketName}/{avatarFileAsset.ObjectPath}";
}
```

- [ ] **Step 8: Build and commit**

Run:

```bash
dotnet build MangaManagementSystem.sln
git add MangaManagementSystem.Business/DTOs/Requests/Users/UpdateMyAvatarRequest.cs MangaManagementSystem.Business/DTOs/Responses/Users/UserProfileResponse.cs MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs MangaManagementSystem.Business/Services/Implements/Users/UserService.cs
git commit -m "feat: expose user avatar profile data"
```

Expected: build succeeds before commit.

## Task 4: Add Avatar Endpoint

**Files:**
- Modify: `MangaManagementSystem.WebApi/Controllers/UserController.cs`

- [ ] **Step 1: Add endpoint**

Add after `UpdateMe`:

```csharp
[HttpPost("me/avatar")]
[Authorize]
[Consumes("multipart/form-data")]
[RequestSizeLimit(5 * 1024 * 1024)]
[SwaggerOperation(
    Summary = "Upload my avatar",
    Description = "Uploads a JPG, PNG, or WEBP avatar for the authenticated user and stores it in the default/general Supabase bucket.")]
[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> UpdateMyAvatar([FromForm] IFormFile file, CancellationToken cancellationToken)
{
    if (file is null)
        throw new ArgumentException("Avatar file is required.");

    var userId = GetUserId() ?? throw new UnauthorizedAccessException();
    var user = await _userService.UpdateMyAvatarAsync(
        userId,
        new UpdateMyAvatarRequest
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = file.OpenReadStream()
        },
        cancellationToken);

    return Ok(new BaseResponse { Data = user, Message = "Avatar updated successfully." });
}
```

- [ ] **Step 2: Build and commit**

Run:

```bash
dotnet build MangaManagementSystem.sln
git add MangaManagementSystem.WebApi/Controllers/UserController.cs
git commit -m "feat: add user avatar upload endpoint"
```

Expected: build succeeds before commit.

## Task 5: Manual Verification

**Files:**
- Read: `MangaManagementSystem.WebApi/appsettings.json`
- Run: `MangaManagementSystem.WebApi`

- [ ] **Step 1: Apply migration**

Run:

```bash
dotnet ef database update --project MangaManagementSystem.DataAccess --startup-project MangaManagementSystem.WebApi
```

Expected: database update succeeds, `Users.AvatarFileAssetId` exists, and `IX_Users_AvatarFileAssetId` is unique for non-null values.

- [ ] **Step 2: Start API**

Run:

```bash
dotnet run --project MangaManagementSystem.WebApi
```

Expected: API starts, commonly on `http://localhost:5151`.

- [ ] **Step 3: Login**

Run in PowerShell with an existing account:

```powershell
$email = Read-Host "Email"
$password = Read-Host "Password"
$loginBody = @{ email = $email; password = $password } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Method Post -Uri "http://localhost:5151/api/auth/login" -ContentType "application/json" -Body $loginBody
$accessToken = $loginResponse.data.accessToken
```

Expected: `$accessToken` contains a bearer token.

- [ ] **Step 4: Upload valid avatar**

Run with a valid PNG at `D:/tmp/avatar.png`:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5151/api/users/me/avatar" -Headers @{ Authorization = "Bearer $accessToken" } -Form @{ file = Get-Item "D:/tmp/avatar.png" }
```

Expected: HTTP 200; response has non-null `data.avatarFileAssetId` and `data.avatarUrl`; stored `FileAssets.BucketName` equals `Supabase:Storage:DefaultBucket`; no other user can reference the same avatar `FileAssetId`.

- [ ] **Step 5: Reject unsupported file**

Run with a PDF at `D:/tmp/not-avatar.pdf`:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5151/api/users/me/avatar" -Headers @{ Authorization = "Bearer $accessToken" } -Form @{ file = Get-Item "D:/tmp/not-avatar.pdf" }
```

Expected: HTTP 400 with extension or MIME validation message.

- [ ] **Step 6: Confirm profile response**

Run with an Admin token stored in `$adminAccessToken`:

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:5151/api/users" -Headers @{ Authorization = "Bearer $adminAccessToken" }
```

Expected: each user includes `avatarFileAssetId` and `avatarUrl`; users without avatars return `null`.

## Task 6: Update Agent Guide After Implementation

**Files:**
- Modify: `MangaManagementSystem.WebApi/docs/AGENTS.md`

- [ ] **Step 1: Update domain notes**

In `Current Domain Model`, update the user/upload bullets to include:

```markdown
- Users and roles: `User`, `Role`; users may reference one current avatar through the one-to-one `User.AvatarFileAssetId` relationship.
- Production artifacts and uploads: `Manuscript`, `FileAsset`; a `FileAsset` can be the current avatar for at most one user.
```

- [ ] **Step 2: Update API notes**

In `API Layer Rules`, add:

```markdown
- User avatar upload belongs under `POST api/users/me/avatar`; it must remain authenticated and use multipart form data.
```

- [ ] **Step 3: Update storage notes**

In `Business Layer Rules`, add:

```markdown
- User avatars use `FileUploadCategory.UserAvatar` and intentionally rely on `Supabase:Storage:DefaultBucket` rather than a dedicated bucket mapping.
```

- [ ] **Step 4: Final build and commit**

Run:

```bash
dotnet build MangaManagementSystem.sln
git add MangaManagementSystem.WebApi/docs/AGENTS.md
git commit -m "docs: document user avatar behavior"
```

Expected: build succeeds before commit.

## Self-Review

- Spec coverage: The plan adds avatar upload, persists the current avatar on `User` as a one-to-one `FileAsset` relationship, stores avatar files through the existing Supabase storage service, relies on the default/general bucket, and exposes avatar data in profile responses.
- Placeholder scan: No implementation step uses prohibited placeholder phrasing or unspecified error handling.
- Type consistency: `AvatarFileAssetId`, `AvatarFileAsset`, `AvatarUser`, `UserAvatar`, `UpdateMyAvatarRequest`, `UpdateMyAvatarAsync`, and `AvatarUrl` are used consistently across the plan.
