# Admin Registration and User Assignment Plan

## Goal

Restrict account registration so only Admin users can create accounts. When an Admin creates a Mangaka account, the Admin must choose an existing Tantou Editor user from the database. The system records that editor-to-mangaka relationship in a new `UserAssignments` table.

For non-Mangaka accounts, Admin can create the account normally without creating an assignment.

## Key Decisions

- Registration is no longer public. The `register` endpoint requires Admin authorization.
- `UserAssignments.FromUserId` represents the Tantou Editor.
- `UserAssignments.ToUserId` represents the Mangaka.
- `AssignmentType` is removed from the table design because this table is only for user-to-user assignments in this feature.
- `UserAssignments.Status` is a boolean active flag, not a string workflow status.
- The first supported assignment scenario is Tantou Editor assigned to Mangaka.
- Admin chooses the account role by sending `RoleId` in the registration request.
- Prefer existing generic `IRepository<T>` for basic persistence. Add a specific repository only if assignment queries become complex or duplicated.

## Data Model Changes

### [NEW] `UserAssignment`

Add a new entity under:

`MangaManagementSystem.DataAccess/Entities/Models/UserAssignment.cs`

Fields:

- `AssignmentId: Guid`
- `FromUserId: Guid`
- `ToUserId: Guid`
- `Status: bool`
- `AssignedAt: DateTime`
- `UnassignedAt: DateTime?`

Navigation properties:

- `FromUser: User`
- `ToUser: User`

Recommended defaults:

- `AssignmentId = Guid.NewGuid()` when creating.
- `Status = true` when creating an active assignment.
- `AssignedAt = DateTime.UtcNow` when creating.
- `UnassignedAt = null` while `Status == true`.

### [MODIFY] `User`

Add navigation collections:

- `AssignmentsFromUser`: assignments where the user is the source editor.
- `AssignmentsToUser`: assignments where the user is the target mangaka.

### [MODIFY] `MangaDbContext`

Add:

- `DbSet<UserAssignment> UserAssignments`
- `ConfigureUserAssignments(modelBuilder)`

Configuration:

- Table name: `UserAssignments`
- Primary key: `AssignmentId`
- `Status` required, default value `true`.
- `AssignedAt` required.
- `UnassignedAt` optional.
- `FromUserId` required FK to `Users.UserId`, `DeleteBehavior.Restrict`.
- `ToUserId` required FK to `Users.UserId`, `DeleteBehavior.Restrict`.
- Add an index on `FromUserId`.
- Add an index on `ToUserId`.
- Add a unique filtered index for one active editor assignment per Mangaka:
  - Columns: `ToUserId`
  - Filter: `Status = 1`

If EF Core filtered indexes are not desired, enforce the “one active assignment” rule in service logic first and add the database constraint later.

### [NEW] Migration

Create an EF Core migration for the new table after model changes.

Suggested migration name:

`AddUserAssignments`

## DTO Changes

### [MODIFY] `RegisterRequest`

Use existing role selection:

```csharp
public Guid RoleId { get; set; }
```

Add Tantou Editor selection:

```csharp
public Guid? TantouEditorId { get; set; }
```

Rules:

- Admin chooses the account role through `RoleId`.
- Required only when the selected role is `Mangaka`.
- Ignored or rejected for non-Mangaka roles. Recommended: reject it for non-Mangaka roles to keep API behavior explicit.

## API Changes

### [MODIFY] `AuthController.Register`

Change:

- Remove `[AllowAnonymous]`.
- Add `[Authorize(Policy = "AdminOnly")]`.
- Update Swagger summary/description to say Admin creates accounts.
- Keep `BaseResponse` behavior unchanged.

Expected behavior:

- Unauthenticated request: `401`.
- Authenticated non-Admin request: `403`.
- Admin request: proceeds to `AuthService.RegisterAsync`.

## Business Logic Changes

### [MODIFY] `AuthService`

Inject:

- `IRepository<UserAssignment>`

Registration flow:

1. Normalize email and username as currently implemented.
2. Validate email/username uniqueness.
3. Load the Admin-selected target role using `request.RoleId`.
4. If role does not exist, throw `KeyNotFoundException`.
5. Create the user with `RoleId = request.RoleId` as currently implemented.
6. If role is `Mangaka`:
   - Require `request.TantouEditorId`.
   - Verify the Tantou Editor user exists.
   - Include or query the editor role and require `Role.RoleName == "Tantou Editor"`.
   - Ensure the editor user is active.
   - Create a `UserAssignment`:
     - `FromUserId = request.TantouEditorId.Value`
     - `ToUserId = newUser.UserId`
     - `Status = true`
     - `AssignedAt = DateTime.UtcNow`
     - `UnassignedAt = null`
7. If role is not `Mangaka`:
   - Reject non-null `TantouEditorId` with `ArgumentException`.
   - Do not create a `UserAssignment`.
8. Save all changes in one unit of work.
9. Return the same `AuthResponse` shape as today.

Important transaction concern:

- Because `IRepository<User>` and `IRepository<UserAssignment>` are scoped and share the same EF `MangaDbContext`, adding the user and assignment before one `SaveChangeAsync()` should save both together.
- Do not save the new user before assignment validation succeeds; otherwise a failed assignment could leave an unassigned Mangaka account.

## Exception Mapping

Use built-in exceptions first:

- Duplicate email/username: `InvalidOperationException` -> `409`
- Missing role: `KeyNotFoundException` -> `404`
- Missing Tantou Editor for Mangaka registration: `ArgumentException` -> `400`
- `TantouEditorId` supplied for non-Mangaka account: `ArgumentException` -> `400`
- Tantou Editor user not found: `KeyNotFoundException` -> `404`
- Selected user is not a Tantou Editor: `ArgumentException` -> `400`
- Selected Tantou Editor inactive: `InvalidOperationException` -> `409`

No custom exception is required for this feature unless future rules need more precise status handling.

## Repository Strategy

Initial implementation:

- Use `IRepository<User>`, `IRepository<Role>`, and `IRepository<UserAssignment>`.
- Use `GetAll()` with LINQ and `Include(...)` for role checks.

Add `IUserRepository` or `IUserAssignmentRepository` later only if:

- queries become repeated in multiple services,
- filtered `Status == true` assignment checks become complex,
- or service code starts leaking too much EF query detail.

## Verification Plan

Automated:

- Run `dotnet build MangaManagementSystem.sln`.
- Run `rg "AllowAnonymous|Authorize" MangaManagementSystem.WebApi/Controllers/AuthController.cs` and verify register is Admin-only.
- Run `rg "UserAssignment|UserAssignments" MangaManagementSystem.DataAccess MangaManagementSystem.Business` and verify entity, DbContext, and service usage exist.

Manual/API checks:

- Anonymous register request returns `401`.
- Non-Admin register request returns `403`.
- Admin creates non-Mangaka account without `TantouEditorId`: succeeds and no assignment row is created.
- Admin creates non-Mangaka account with `TantouEditorId`: returns `400`.
- Admin creates Mangaka without `TantouEditorId`: returns `400`.
- Admin creates Mangaka with nonexistent editor ID: returns `404`.
- Admin creates Mangaka with non-editor user ID: returns `400`.
- Admin creates Mangaka with active Tantou Editor ID: succeeds and creates one assignment row with `Status = true`.

## Out of Scope

- Reassignment or unassignment endpoints.
- Listing assigned Mangaka by editor.
- Listing editor for existing Mangaka.
- Changing roles after account creation.
- Seeding roles or users.
- Custom exception classes.
