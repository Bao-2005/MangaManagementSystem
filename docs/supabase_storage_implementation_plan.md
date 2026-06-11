# Refactor File Storage: LocalStorage → Supabase Storage

Replace `LocalStorageService` with `SupabaseStorageService` that uploads to Supabase Storage buckets, keeping `IStorageService` as the abstraction so all callers remain unchanged.

---

## Resolved Design Decisions

### ✅ All Buckets Are Public

**Why:** The FE needs to retrieve files directly via URL without going through the API. In Supabase, a **public** bucket means any file can be accessed by its URL directly (no auth needed). A **private** bucket requires a signed URL generated server-side for each access. Since you want `FE → Supabase URL directly`, all buckets are set to **public**.

> [!NOTE]
> Buckets must be **manually created** in the Supabase Dashboard (Storage tab) and set to **public** before the app starts. The `SupabaseStorageService` does not auto-create buckets — it uploads into existing ones.

### ✅ Config-Driven Bucket Mapping (Extensible with Zero Code Change)

This is the core extensibility mechanism. `SupabaseStorageService` reads the bucket mapping **entirely from `appsettings.json`** at startup:

```json
"Supabase": {
  "Storage": {
    "DefaultBucket": "generic-uploads",
    "Buckets": {
      "ProposalSamplePage": "proposal-pages",
      "ProposalSource":     "proposal-sources"
    }
  }
}
```

To add a new bucket in the future (e.g. for a `ChapterPage` feature):

1. Create the bucket in Supabase Dashboard.
2. Add `"ChapterPage": "chapter-pages"` to `appsettings.json`.
3. Add `ChapterPage` to the `FileUploadCategory` enum.
4. **No other code changes needed** — `SupabaseStorageService` looks up the config dict at runtime.

> [!NOTE]
> **What is `Generic`?** It is the catch-all fallback category used when no specific category is appropriate (e.g. uploading profile pictures, cover art, or testing). `DefaultBucket` in config is the fallback when a category key is not found in the `Buckets` map.

### ✅ `PublicUrl` added to `StoredFileResult`

Add `string? PublicUrl` to `StoredFileResult`. For all public buckets, this is always populated. Callers that don't use it (like rollback logic) safely ignore it.

### ✅ Use Supabase NuGet Package

Use the official `Supabase` C# client (`supabase-csharp`) via NuGet. Provides a clean `Storage` API:

```csharp
await client.Storage.From("bucket").Upload(bytes, "path/to/file.png");
client.Storage.From("bucket").GetPublicUrl("path/to/file.png");
await client.Storage.From("bucket").Remove(new List<string> { "path/to/file.png" });
```

### ✅ `StoredFileResult` stays in `IStorageService.cs`

`StoredFileResult` is **not** a controller-level DTO — it's an internal value object used between `SupabaseStorageService` and `FileUploadService`. Keeping it co-located with the interface it belongs to is idiomatic. No need to move it to `DTOs/Responses/`.

---

## Starting Scope (3 Buckets)

| `FileUploadCategory` | Supabase Bucket    | Purpose                                                   |
| -------------------- | ------------------ | --------------------------------------------------------- |
| `ProposalSamplePage` | `proposal-pages`   | Preview images displayed in FE proposal cards             |
| `ProposalSource`     | `proposal-sources` | Source ZIP for editors to download                        |
| `Generic`            | `generic-uploads`  | Catch-all fallback for assets without a specific category |

All 3 buckets must be created in Supabase Dashboard (set to **public**) before running. `TaskSubmission` can be added later with zero code change.

> [!NOTE]
> **`generic-uploads`** is the `DefaultBucket` — it's used when the upload `category` value is not found in the `Buckets` config map. Even if you never upload with `Generic` category explicitly, it acts as a safety net so uploads never fail due to a missing bucket mapping.

---

## Proposed Changes

### Business Layer

#### [MODIFY] [IStorageService.cs](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.Business/Services/Interfaces/Files/IStorageService.cs)

Add `string? PublicUrl` to `StoredFileResult`:

```csharp
public class StoredFileResult
{
    public string BucketName { get; set; } = null!;
    public string ObjectPath { get; set; } = null!;
    public string StoredFileName { get; set; } = null!;
    public string? PublicUrl { get; set; }  // ← NEW: null for private buckets
}
```

#### [NEW] `SupabaseStorageService.cs`

Path: `MangaManagementSystem.Business/Services/Implements/Files/SupabaseStorageService.cs`

Implements `IStorageService`. Key behaviors:

1. **Constructor** — takes `IConfiguration` and `Supabase.Client`; reads `Supabase:Storage:Buckets` section into a `Dictionary<string, string>` at startup.
2. **Bucket resolution** — `category` string is looked up in the config dict; falls back to `DefaultBucket` if not found.
3. **Object path** — same date-partitioned UUID pattern as `LocalStorageService`:
   `<category>/<yyyy>/<MM>/<dd>/<uuid>.<ext>`
4. **Upload** — `client.Storage.From(bucket).Upload(bytes, objectPath)`.
5. **Public URL** — `client.Storage.From(bucket).GetPublicUrl(objectPath)` → set on `StoredFileResult.PublicUrl`.
6. **Delete** — `client.Storage.From(bucket).Remove(new List<string> { objectPath })`.

#### [MODIFY] [FileAssetResponse.cs](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.Business/DTOs/Responses/Files/FileAssetResponse.cs)

Add `string? PublicUrl` to `FileAssetResponse` so the FE receives the direct Supabase URL in the upload response:

```csharp
public class FileAssetResponse
{
    ...
    public string? PublicUrl { get; set; }  // ← NEW
}
```

#### [MODIFY] [FileUploadService.cs](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.Business/Services/Implements/Files/FileUploadService.cs)

- Inject `IConfiguration` to read `Supabase:Url`.
- Update `Map(FileAsset asset)` to compute and populate `FileAssetResponse.PublicUrl`:
  ```csharp
  PublicUrl = $"{_supabaseUrl}/storage/v1/object/public/{asset.BucketName}/{asset.ObjectPath}"
  ```

> [!NOTE]
> **Confirmed: Option A** — `PublicUrl` is reconstructed on the fly in `FileUploadService.Map()` using:
> `{SupabaseUrl}/storage/v1/object/public/{asset.BucketName}/{asset.ObjectPath}`
>
> No DB migration needed. `FileAsset` entity stays unchanged. `FileUploadService` will need `IConfiguration` injected to read `Supabase:Url` for URL construction.

---

### WebApi Layer

#### [MODIFY] [appsettings.json](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.WebApi/appsettings.json)

Extend the existing `Supabase` block:

```json
"Supabase": {
  "Url": "https://tquxgekvreihsrkdwduq.supabase.co",
  "ServiceRoleKey": "",
  "Storage": {
    "DefaultBucket": "generic-uploads",
    "Buckets": {
      "ProposalSamplePage": "proposal-pages",
      "ProposalSource":     "proposal-sources",
      "Generic":            "generic-uploads"
    }
  }
}
```

> [!NOTE]
> **To add a new bucket in the future** — only these two steps are needed:
>
> 1. Create the bucket in Supabase Dashboard (set to public).
> 2. Add `"CategoryName": "bucket-name"` to this `Buckets` section.

#### [MODIFY] [ServiceCollection.cs](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs)

- Register `Supabase.Client` as a singleton (initialized with URL + ServiceRoleKey).
- Swap storage registration:

```csharp
// Register Supabase client
services.AddSingleton(provider => {
    var config = provider.GetRequiredService<IConfiguration>();
    var url = config["Supabase:Url"]!;
    var key = config["Supabase:ServiceRoleKey"]!;
    var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
    var client = new Supabase.Client(url, key, options);
    client.InitializeAsync().GetAwaiter().GetResult();
    return client;
});

// Swap LocalStorageService → SupabaseStorageService
services.AddScoped<IStorageService, SupabaseStorageService>();
```

#### [MODIFY] [MangaManagementSystem.WebApi.csproj](file:///d:/Class/SU26/PRN232/CODE/MangaManagementSystem/MangaManagementSystem.WebApi/MangaManagementSystem.WebApi.csproj)

Add the Supabase NuGet package:

```xml
<PackageReference Include="Supabase" Version="1.1.1" />
```

---

## Files Summary

| File                                  | Action | Notes                                                       |
| ------------------------------------- | ------ | ----------------------------------------------------------- |
| `IStorageService.cs`                  | MODIFY | Add `PublicUrl` to `StoredFileResult`                       |
| `SupabaseStorageService.cs`           | NEW    | Config-driven Supabase implementation                       |
| `LocalStorageService.cs`              | Keep   | Retained, not deleted                                       |
| `FileAssetResponse.cs`                | MODIFY | Add `PublicUrl` field                                       |
| `FileUploadService.cs`                | MODIFY | Pass `PublicUrl` through in `Map()` helper                  |
| `appsettings.json`                    | MODIFY | Add `Supabase:Storage` bucket config                        |
| `ServiceCollection.cs`                | MODIFY | Register `Supabase.Client` singleton + swap storage service |
| `MangaManagementSystem.WebApi.csproj` | MODIFY | Add `Supabase` NuGet package                                |

---

## Prerequisite (Do Before Running)

1. In **Supabase Dashboard → Storage**, create the following buckets and set each to **public**:
   - `proposal-pages`
   - `proposal-sources`
   - `generic-uploads`
   - (add more later as needed — zero code change required)

---

## Verification Plan

```bash
dotnet build MangaManagementSystem.sln
dotnet run --project MangaManagementSystem.WebApi
```

Manual checks:

1. Upload a `ProposalSamplePage` file → response includes `publicUrl` → URL opens in browser.
2. Upload a `ProposalSource` ZIP → appears in `proposal-sources` bucket in Supabase Dashboard.
3. Upload with an unmapped category → falls back to `generic-uploads` bucket.
4. Delete a file → disappears from Supabase bucket.
5. Full proposal creation flow with sample pages still works end-to-end.
