# Soft Delete + User Management API — Final Plan

## Changes Summary

| Layer | What |
|---|---|
| 11 entity models | Add `DateTime? DeletedAt` |
| DbContext | Configure `DeletedAt` as nullable on all entities (no global query filter) |
| Migration | `AddSoftDeleteToAllEntities` |
| New DTO | `UserProfileResponse` |
| New interface | `IUserService` |
| New service | `UserService` — GetAll (explicit `.Where(DeletedAt == null)`), SoftDelete (cascade) |
| DI | Register `IUserService → UserService` |
| New controller | `UserController` — `GET /api/users`, `DELETE /api/users/{id}/soft-delete` |

No `/me` endpoint — already covered by `GET /api/auth/me`.  
No global EF query filter — all filtering done via explicit LINQ `.Where()`.

---

## Cascade Soft-Delete Scope

When `SoftDeleteAsync(userId)` runs, it sets `DeletedAt = UtcNow` on the user **and** these directly owned relations in a single `SaveChanges`:

| Table | FK to User | Why cascade |
|---|---|---|
| `UserAssignment` | `FromUserId` OR `ToUserId` | Assignment to a deleted user is meaningless |
| `PageTask` | `AssistantId` | Task assigned to deleted assistant |
| `Annotation` | `AuthorId` | Annotations by deleted user |
| `FileAsset` | `UploadedBy` | Files uploaded by deleted user |
| `Series` | `MangakaId` | Series owned by deleted Mangaka |

`Chapter`, `Manuscript`, `ChapterPage`, `PageTaskSubmission` are **not** directly cascaded — they are sub-resources of Series/Manuscript and remain as audit trail.
