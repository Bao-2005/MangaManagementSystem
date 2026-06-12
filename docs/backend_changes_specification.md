# Backend Changes Specification

This document details the required backend changes in the `MangaManagementSystem` solution to support the new features and integrations in the Manga Production & Editorial Workspace frontend.

---

## 1. File Streaming Endpoint (Download & Preview)

### Target File
`MangaManagementSystem.WebApi/Controllers/FileController.cs`

### Purpose
Allows the frontend to retrieve/download raw files (such as ZIP manuscript packages and sample preview pages) which are saved in the server's local storage folder.

### Required Code Additions

#### A. Add the following namespaces:
```csharp
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO;
```

#### B. Inject `IRepository<FileAsset>` and `IConfiguration` into the constructor:
```csharp
private readonly IFileUploadService _fileUploadService;
private readonly IRepository<FileAsset> _fileAssetRepository;
private readonly string _rootPath;

public FileController(
    IFileUploadService fileUploadService,
    IRepository<FileAsset> fileAssetRepository,
    IConfiguration configuration)
{
    _fileUploadService = fileUploadService;
    _fileAssetRepository = fileAssetRepository;
    _rootPath = configuration["FileStorage:RootPath"]
        ?? Path.Combine(Path.GetTempPath(), "MangaManagementSystem", "uploads");
}
```

#### C. Add the `GET` endpoint to stream files:
```csharp
[HttpGet("{id:guid}")]
[AllowAnonymous]
[SwaggerOperation(Summary = "Retrieve/download a file asset by ID")]
public async Task<IActionResult> GetFile(Guid id)
{
    var asset = await _fileAssetRepository.GetAll()
        .FirstOrDefaultAsync(x => x.FileAssetId == id && x.DeletedAt == null);

    if (asset == null)
    {
        return NotFound(new BaseResponse { Message = "File not found." });
    }

    var fullPath = Path.Combine(_rootPath, asset.ObjectPath.Replace('/', Path.DirectorySeparatorChar));
    if (!System.IO.File.Exists(fullPath))
    {
        return NotFound(new BaseResponse { Message = "Physical file not found on storage." });
    }

    var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    return File(stream, asset.MimeType, asset.OriginalFileName);
}
```

---

## 2. Retrieve Assigned Mangakas for Editor

### Purpose
Allows an editor (Tantou Editor) to fetch a list of Mangakas who are assigned to them.

### Changes:

### A. Interface
`MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`

Add the method declaration:
```csharp
Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId);
```

### B. Implementation
`MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`

Add the method implementation:
```csharp
public async Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId)
{
    return await _userRepository.GetAll()
        .Include(x => x.Role)
        .Include(x => x.AssignmentsToUser)
            .ThenInclude(x => x.FromUser)
        .Where(x => x.DeletedAt == null && x.AssignmentsToUser.Any(a => a.FromUserId == editorId && a.DeletedAt == null && a.UnassignedAt == null))
        .Select(x => new
        {
            User = x,
            AssignedEditor = x.AssignmentsToUser
                .Where(a => a.DeletedAt == null && a.UnassignedAt == null && a.FromUser.DeletedAt == null)
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new
                {
                    a.FromUserId,
                    a.FromUser.DisplayName
                })
                .FirstOrDefault()
        })
        .Select(x => new UserProfileResponse
        {
            UserId = x.User.UserId,
            UserName = x.User.UserName,
            Email = x.User.Email,
            DisplayName = x.User.DisplayName,
            RoleName = x.User.Role.RoleName,
            AssignedEditorId = x.AssignedEditor == null ? null : x.AssignedEditor.FromUserId,
            AssignedEditorName = x.AssignedEditor == null ? null : x.AssignedEditor.DisplayName,
            CreatedAt = x.User.CreatedAt,
            LastLoginAt = x.User.LastLoginAt,
            DeletedAt = x.User.DeletedAt
        })
        .ToListAsync();
}
```

### C. Controller
`MangaManagementSystem.WebApi/Controllers/UserController.cs`

Add the endpoint:
```csharp
[HttpGet("my-mangakas")]
[Authorize(Policy = "TantouEditorOnly")]
[SwaggerOperation(
    Summary = "Get assigned mangakas for the current editor",
    Description = "Tantou Editor only. Returns all active mangakas assigned to the current editor.")]
[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetMyMangakas()
{
    var editorId = GetUserId() ?? throw new UnauthorizedAccessException();
    var mangakas = await _userService.GetAssignedMangakasAsync(editorId);
    return Ok(new BaseResponse { Data = mangakas, Message = "Success" });
}
```

---

## 3. Include Email in User Assignment Responses

### Purpose
Expose the email of the assigned user in the mapping responses so the frontend can display it correctly.

### Changes:

### A. DTO Response
`MangaManagementSystem.Business/DTOs/Responses/Users/UserAssignmentResponse.cs`

Add the property:
```csharp
public string ToUserEmail { get; set; } = null!;
```

### B. Service Mapping
`MangaManagementSystem.Business/Services/Implements/Users/UserAssignmentService.cs`

Map `ToUserEmail` inside the conversion:
```csharp
private static UserAssignmentResponse MapToResponse(UserAssignment a) => new()
{
    AssignmentId = a.AssignmentId, 
    FromUserId = a.FromUserId, 
    FromUserName = a.FromUser?.DisplayName ?? "",
    ToUserId = a.ToUserId, 
    ToUserName = a.ToUser?.DisplayName ?? "",
    ToUserEmail = a.ToUser?.Email ?? "", // <-- Map this property
    AssignedAt = a.AssignedAt, 
    UnassignedAt = a.UnassignedAt
};
```
