# Auth Me User Profile Response Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor `GET /api/auth/me` so it returns the full `UserProfileResponse` shape instead of the compact `UserDto` shape.

**Architecture:** Keep authentication-specific login/register behavior in `IAuthService` unchanged. Add a focused current-profile read method to `IUserService`, reuse the existing `UserProfileQuery`/`MapProfile` logic, and inject `IUserService` into `AuthController` only for `GET /api/auth/me`. Add focused unit tests for the service profile projection and controller response shape.

**Tech Stack:** ASP.NET Core 8 Web API, Entity Framework Core queryable repositories, xUnit, FluentAssertions, Moq.

---

## File Structure

- Create: `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`
  - Test project referencing WebApi, Business, and DataAccess.
- Create: `MangaManagementSystem.Tests/Users/InMemoryRepository.cs`
  - Small in-memory `IRepository<T>` test double for service tests.
- Create: `MangaManagementSystem.Tests/Users/UserServiceProfileTests.cs`
  - Verifies `IUserService.GetMyProfileAsync` returns the rich profile response, including assigned editor and avatar URL.
- Create: `MangaManagementSystem.Tests/Auth/AuthControllerMeTests.cs`
  - Verifies `AuthController.Me()` calls `IUserService.GetMyProfileAsync` and returns `BaseResponse.Data` as `UserProfileResponse`.
- Modify: `MangaManagementSystem.sln`
  - Add the test project to the solution.
- Modify: `MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`
  - Add `Task<UserProfileResponse> GetMyProfileAsync(Guid userId);`.
- Modify: `MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`
  - Implement `GetMyProfileAsync` using the existing profile query and active-user filter.
- Modify: `MangaManagementSystem.WebApi/Controllers/AuthController.cs`
  - Inject `IUserService`.
  - Refactor `Me()` to return `UserProfileResponse`.
  - Keep `Register`, `Login`, `Refresh`, `Logout`, and `ChangePassword` behavior unchanged.

---

### Task 1: Add Test Project

**Files:**
- Create: `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`
- Modify: `MangaManagementSystem.sln`

- [ ] **Step 1: Create the test project file**

Create `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="xunit" Version="2.8.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MangaManagementSystem.WebApi\MangaManagementSystem.WebApi.csproj" />
    <ProjectReference Include="..\MangaManagementSystem.Business\MangaManagementSystem.Business.csproj" />
    <ProjectReference Include="..\MangaManagementSystem.DataAccess\MangaManagementSystem.DataAccess.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add the test project to the solution**

Run:

```powershell
dotnet sln MangaManagementSystem.sln add MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj
```

Expected: command succeeds and prints that the project was added.

- [ ] **Step 3: Restore and verify the empty test project builds**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --no-restore
```

Expected: build succeeds with `No test is available` or `Total tests: 0`.

- [ ] **Step 4: Commit**

```powershell
git add MangaManagementSystem.sln MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj
git commit -m "test: add test project"
```

---

### Task 2: Add Failing User Service Profile Test

**Files:**
- Create: `MangaManagementSystem.Tests/Users/InMemoryRepository.cs`
- Create: `MangaManagementSystem.Tests/Users/UserServiceProfileTests.cs`

- [ ] **Step 1: Create the in-memory repository test double**

Create `MangaManagementSystem.Tests/Users/InMemoryRepository.cs`:

```csharp
using MangaManagementSystem.DataAccess.Repositories.Interfaces;

namespace MangaManagementSystem.Tests.Users;

internal sealed class InMemoryRepository<T> : IRepository<T>
{
    private readonly List<T> _items;

    public InMemoryRepository(IEnumerable<T>? items = null)
    {
        _items = items?.ToList() ?? new List<T>();
    }

    public IQueryable<T> GetAll()
    {
        return _items.AsQueryable();
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _items.AddRange(entities);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
    }

    public void Delete(T entity)
    {
        _items.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities.ToList())
        {
            _items.Remove(entity);
        }
    }

    public Task SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Write the failing service test**

Create `MangaManagementSystem.Tests/Users/UserServiceProfileTests.cs`:

```csharp
using FluentAssertions;
using MangaManagementSystem.Business.Services.Implements.Users;
using MangaManagementSystem.Business.Services.Interfaces.Files;
using MangaManagementSystem.DataAccess.Entities.Models;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MangaManagementSystem.Tests.Users;

public class UserServiceProfileTests
{
    [Fact]
    public async Task GetMyProfileAsync_returns_current_user_profile_response()
    {
        var roleId = Guid.NewGuid();
        var editorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var avatarId = Guid.NewGuid();

        var mangakaRole = new Role
        {
            RoleId = roleId,
            RoleName = "Mangaka"
        };

        var editorRole = new Role
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TantouEditor"
        };

        var editor = new User
        {
            UserId = editorId,
            RoleId = editorRole.RoleId,
            Role = editorRole,
            UserName = "editor01",
            Email = "editor@example.com",
            DisplayName = "Senior Editor",
            PasswordHash = "hash",
            CreatedAt = new DateTime(2026, 06, 01, 0, 0, 0, DateTimeKind.Utc)
        };

        var avatar = new FileAsset
        {
            FileAssetId = avatarId,
            BucketName = "general",
            ObjectPath = "user-avatars/current.png",
            OriginalFileName = "current.png",
            StoredFileName = "current.png",
            Extension = ".png",
            FileSizeBytes = 1234,
            MimeType = "image/png"
        };

        var currentUser = new User
        {
            UserId = userId,
            RoleId = roleId,
            Role = mangakaRole,
            AvatarFileAssetId = avatarId,
            AvatarFileAsset = avatar,
            UserName = "mangaka01",
            Email = "mangaka@example.com",
            DisplayName = "Current Mangaka",
            PasswordHash = "hash",
            CreatedAt = new DateTime(2026, 06, 02, 0, 0, 0, DateTimeKind.Utc),
            LastLoginAt = new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc)
        };

        var assignment = new UserAssignment
        {
            AssignmentId = Guid.NewGuid(),
            FromUserId = editorId,
            FromUser = editor,
            ToUserId = userId,
            ToUser = currentUser,
            AssignedAt = new DateTime(2026, 06, 04, 0, 0, 0, DateTimeKind.Utc)
        };
        currentUser.AssignmentsToUser.Add(assignment);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Supabase:Url"] = "https://project.supabase.co"
            })
            .Build();

        var service = new UserService(
            new InMemoryRepository<User>(new[] { currentUser, editor }),
            new InMemoryRepository<UserAssignment>(new[] { assignment }),
            new InMemoryRepository<PageTask>(),
            new InMemoryRepository<Annotation>(),
            new InMemoryRepository<Series>(),
            new InMemoryRepository<Role>(new[] { mangakaRole, editorRole }),
            Mock.Of<IFileUploadService>(),
            configuration);

        var response = await service.GetMyProfileAsync(userId);

        response.UserId.Should().Be(userId);
        response.UserName.Should().Be("mangaka01");
        response.Email.Should().Be("mangaka@example.com");
        response.DisplayName.Should().Be("Current Mangaka");
        response.RoleName.Should().Be("Mangaka");
        response.AssignedEditorId.Should().Be(editorId);
        response.AssignedEditorName.Should().Be("Senior Editor");
        response.AvatarFileAssetId.Should().Be(avatarId);
        response.AvatarUrl.Should().Be("https://project.supabase.co/storage/v1/object/public/general/user-avatars/current.png");
        response.DeletedAt.Should().BeNull();
    }
}
```

- [ ] **Step 3: Run the service test to verify it fails**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --filter "FullyQualifiedName~UserServiceProfileTests"
```

Expected: FAIL with a compiler error similar to:

```text
'UserService' does not contain a definition for 'GetMyProfileAsync'
```

- [ ] **Step 4: Commit**

Do not commit yet. This task intentionally leaves a failing test for Task 3.

---

### Task 3: Implement Current User Profile Service Method

**Files:**
- Modify: `MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`
- Modify: `MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`
- Test: `MangaManagementSystem.Tests/Users/UserServiceProfileTests.cs`

- [ ] **Step 1: Add the interface method**

In `MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs`, replace the interface body with:

```csharp
using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileResponse>> GetAllAsync();
        Task<IEnumerable<UserProfileResponse>> GetAssistantsAsync();
        Task<UserProfileResponse> GetMyProfileAsync(Guid userId);
        Task<UserProfileResponse> AdminUpdateAsync(Guid userId, AdminUpdateUserRequest request);
        Task<UserProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request);
        Task SoftDeleteAsync(Guid userId);
        Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId);
        Task<UserProfileResponse> UpdateMyAvatarAsync(Guid userId, UpdateMyAvatarRequest request, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 2: Add the service method**

In `MangaManagementSystem.Business/Services/Implements/Users/UserService.cs`, add this method after `GetAssistantsAsync()`:

```csharp
public async Task<UserProfileResponse> GetMyProfileAsync(Guid userId)
{
    var projection = await UserProfileQuery()
        .Where(x => x.User.UserId == userId && x.User.DeletedAt == null)
        .FirstOrDefaultAsync()
        ?? throw new KeyNotFoundException("User not found.");

    return MapProfile(projection.User, projection.AssignedEditor, _supabaseUrl);
}
```

- [ ] **Step 3: Run the service test**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --filter "FullyQualifiedName~UserServiceProfileTests"
```

Expected: PASS. If the in-memory query fails because EF Core `Include` extensions are no-ops only for EF providers, keep `UserProfileQuery()` as-is and adjust the test data navigation properties; do not add database-specific logic to production code.

- [ ] **Step 4: Commit**

```powershell
git add MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs MangaManagementSystem.Business/Services/Implements/Users/UserService.cs MangaManagementSystem.Tests/Users/InMemoryRepository.cs MangaManagementSystem.Tests/Users/UserServiceProfileTests.cs
git commit -m "feat: add current user profile service"
```

---

### Task 4: Add Failing Auth Controller Response Test

**Files:**
- Create: `MangaManagementSystem.Tests/Auth/AuthControllerMeTests.cs`

- [ ] **Step 1: Write the failing controller test**

Create `MangaManagementSystem.Tests/Auth/AuthControllerMeTests.cs`:

```csharp
using System.Security.Claims;
using FluentAssertions;
using MangaManagementSystem.API.Controllers;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.Tests.Auth;

public class AuthControllerMeTests
{
    [Fact]
    public async Task Me_returns_user_profile_response_for_authenticated_user()
    {
        var userId = Guid.NewGuid();
        var expectedProfile = new UserProfileResponse
        {
            UserId = userId,
            UserName = "mangaka01",
            Email = "mangaka@example.com",
            DisplayName = "Current Mangaka",
            RoleName = "Mangaka",
            AssignedEditorId = Guid.NewGuid(),
            AssignedEditorName = "Senior Editor",
            CreatedAt = new DateTime(2026, 06, 02, 0, 0, 0, DateTimeKind.Utc),
            LastLoginAt = new DateTime(2026, 06, 03, 0, 0, 0, DateTimeKind.Utc),
            AvatarFileAssetId = Guid.NewGuid(),
            AvatarUrl = "https://project.supabase.co/storage/v1/object/public/general/user-avatars/current.png"
        };

        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.GetMyProfileAsync(userId))
            .ReturnsAsync(expectedProfile);

        var controller = new AuthController(Mock.Of<IAuthService>(), userService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuth"))
                }
            }
        };

        var result = await controller.Me();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<BaseResponse>().Subject;
        body.Message.Should().Be("Success");
        body.Data.Should().BeSameAs(expectedProfile);
        userService.Verify(x => x.GetMyProfileAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Me_returns_unauthorized_when_token_has_no_valid_user_id()
    {
        var controller = new AuthController(Mock.Of<IAuthService>(), Mock.Of<IUserService>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.Me();

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var body = unauthorized.Value.Should().BeOfType<BaseResponse>().Subject;
        body.Message.Should().Be("Unauthorized");
    }
}
```

- [ ] **Step 2: Run the controller test to verify it fails**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --filter "FullyQualifiedName~AuthControllerMeTests"
```

Expected: FAIL with a compiler error similar to:

```text
'AuthController' does not contain a constructor that takes 2 arguments
```

- [ ] **Step 3: Commit**

Do not commit yet. This task intentionally leaves a failing test for Task 5.

---

### Task 5: Refactor AuthController.Me To Use UserProfileResponse

**Files:**
- Modify: `MangaManagementSystem.WebApi/Controllers/AuthController.cs`
- Test: `MangaManagementSystem.Tests/Auth/AuthControllerMeTests.cs`

- [ ] **Step 1: Add the user service using**

In `MangaManagementSystem.WebApi/Controllers/AuthController.cs`, add this using with the other business service usings:

```csharp
using MangaManagementSystem.Business.Services.Interfaces.Users;
```

- [ ] **Step 2: Inject `IUserService`**

In `AuthController`, replace the field and constructor with:

```csharp
private readonly IAuthService _authService;
private readonly IUserService _userService;

public AuthController(IAuthService authService, IUserService userService)
{
    _authService = authService;
    _userService = userService;
}
```

- [ ] **Step 3: Refactor `Me()`**

Replace the current `Me()` method with:

```csharp
[HttpGet("me")]
[Authorize]
[SwaggerOperation(
    Summary = "Get current user",
    Description = "Returns the full profile of the authenticated user based on the Bearer token in the Authorization header.")]
[ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Me()
{
    var userId = GetUserId();
    if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

    var profile = await _userService.GetMyProfileAsync(userId.Value);

    return Ok(new BaseResponse { Data = profile, Message = "Success" });
}
```

- [ ] **Step 4: Remove unused usings if the compiler reports them**

If `MangaManagementSystem.Business.DTOs.Responses.Auth` or `MangaManagementSystem.Business.DTOs.Responses.Users` becomes unused in `AuthController.cs`, remove only the unused using reported by the compiler. Keep `MangaManagementSystem.Business.DTOs.Responses.Users` if `UserDto` is still used by `Register` or `Login`.

- [ ] **Step 5: Run the controller test**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --filter "FullyQualifiedName~AuthControllerMeTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add MangaManagementSystem.WebApi/Controllers/AuthController.cs MangaManagementSystem.Tests/Auth/AuthControllerMeTests.cs
git commit -m "feat: return profile response from auth me"
```

---

### Task 6: Full Verification

**Files:**
- Verify: `MangaManagementSystem.sln`
- Verify: `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`

- [ ] **Step 1: Run all tests**

Run:

```powershell
dotnet test MangaManagementSystem.sln
```

Expected: all tests pass.

- [ ] **Step 2: Build the API**

Run:

```powershell
dotnet build MangaManagementSystem.WebApi\MangaManagementSystem.WebApi.csproj
```

Expected: build succeeds with `0 Error(s)`.

- [ ] **Step 3: Manual API response check**

Run the API with the existing local configuration:

```powershell
dotnet run --project MangaManagementSystem.WebApi\MangaManagementSystem.WebApi.csproj
```

Login through the existing `POST /api/auth/login` endpoint, then call:

```http
GET /api/auth/me
Authorization: Bearer <access-token>
```

Expected response data shape:

```json
{
  "data": {
    "userId": "00000000-0000-0000-0000-000000000000",
    "userName": "mangaka01",
    "email": "mangaka@example.com",
    "displayName": "Current Mangaka",
    "roleName": "Mangaka",
    "assignedEditorId": "00000000-0000-0000-0000-000000000000",
    "assignedEditorName": "Senior Editor",
    "createdAt": "2026-06-02T00:00:00Z",
    "lastLoginAt": "2026-06-03T00:00:00Z",
    "deletedAt": null,
    "avatarFileAssetId": "00000000-0000-0000-0000-000000000000",
    "avatarUrl": "https://project.supabase.co/storage/v1/object/public/general/user-avatars/current.png"
  },
  "message": "Success"
}
```

The exact IDs, names, timestamps, and avatar URL depend on the logged-in database user.

- [ ] **Step 4: Confirm no compact `UserDto` is returned from `GET /api/auth/me`**

Verify the response no longer has this compact shape:

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "email": "mangaka@example.com",
    "name": "Current Mangaka",
    "role": "Mangaka"
  },
  "message": "Success"
}
```

- [ ] **Step 5: Commit verification-only fixes if needed**

If verification required minor fixes, commit them:

```powershell
git add MangaManagementSystem.Business/Services/Interfaces/Users/IUserService.cs MangaManagementSystem.Business/Services/Implements/Users/UserService.cs MangaManagementSystem.WebApi/Controllers/AuthController.cs MangaManagementSystem.Tests
git commit -m "fix: stabilize auth me profile response"
```

Skip this commit if there were no changes after Task 5.

---

## Self-Review

**Spec coverage:** The requested refactor is covered by Task 3 and Task 5: the API `GET /api/auth/me` now reaches the existing user-profile response path. Task 2 proves the service response includes full profile fields. Task 4 proves controller response data is `UserProfileResponse`.

**Placeholder scan:** All task steps include exact paths, commands, and code snippets where code changes are required.

**Type consistency:** `IUserService.GetMyProfileAsync(Guid userId)` is introduced in Task 3 and used by `AuthController.Me()` and `AuthControllerMeTests` with the same signature. `UserProfileResponse` property names match `MangaManagementSystem.Business/DTOs/Responses/Users/UserProfileResponse.cs`.
