# Environment Configuration Guide Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert the Web API to read secrets from environment variables instead of committed `appsettings.json`, with enough explanation for a developer to implement it manually.

**Architecture:** ASP.NET Core builds configuration from several providers. `appsettings.json` loads first, then `appsettings.Development.json`, then environment variables, so environment variables can override JSON values. Nested JSON keys are represented in environment variables with double underscores, for example `Jwt:Key` becomes `Jwt__Key`.

**Tech Stack:** .NET 8, ASP.NET Core Web API, `IConfiguration`, PowerShell environment variables, deployment platform environment settings.

---

## Key Concept: Where Is `.env` Saved?

This project does not currently use a `.env` file automatically.

In .NET / ASP.NET Core, environment variables are usually saved in one of these places:

1. Current terminal session
   - Saved only while that PowerShell window is open.
   - Best for quick local testing.

2. User-level Windows environment variables
   - Saved for your Windows user account.
   - Available to future terminals after reopening them.

3. Machine-level Windows environment variables
   - Saved for all users on the machine.
   - Usually requires admin permission.

4. Deployment platform settings
   - Saved in Azure, Docker, Render, Railway, GitHub Actions, or another host.
   - Best for production.

5. Local `.env` file
   - Saved in the repository folder, but should usually be ignored by Git.
   - ASP.NET Core does not read `.env` files by default.
   - If you want `.env` support, you must add a package or custom loader. This guide does not recommend that for this project because ASP.NET Core already supports environment variables directly.

Recommended for this codebase:

- Use PowerShell `$env:` variables for local development.
- Use deployment platform environment variables for production.
- Do not commit real secrets into `appsettings.json`, `appsettings.Development.json`, README examples, or source code.

## Key Concept: How Does The Code Read Environment Variables?

ASP.NET Core creates this in `MangaManagementSystem.WebApi/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

That builder automatically reads configuration from:

```text
appsettings.json
appsettings.{Environment}.json
User secrets in Development, when configured
Environment variables
Command-line arguments
```

So this JSON:

```json
{
  "Jwt": {
    "Key": "json-value"
  }
}
```

can be overridden by this PowerShell environment variable:

```powershell
$env:Jwt__Key = "environment-value"
```

Then this C# code reads the final value:

```csharp
var jwtKey = builder.Configuration["Jwt:Key"];
```

The app does not read `Jwt__Key` directly. ASP.NET Core converts `__` into `:` internally.

## Required Environment Variables For This Project

Use these names:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=manga_management;Username=postgres;Password=postgres;SSL Mode=Disable"
$env:Jwt__Issuer = "MangaManagementSystem"
$env:Jwt__Audience = "MangaManagementSystemClient"
$env:Jwt__Key = "replace-with-at-least-32-characters-for-hmac-signing"
$env:Jwt__AccessTokenMinutes = "15"
$env:Jwt__RefreshTokenDays = "7"
$env:Client__BaseUrl = "http://localhost:5173"
$env:Supabase__Url = "https://your-project.supabase.co"
$env:Supabase__ServiceRoleKey = "replace-with-service-role-key"
$env:Supabase__Storage__DefaultBucket = "generic-uploads"
$env:Supabase__Storage__Buckets__ProposalSamplePage = "proposal-pages"
$env:Supabase__Storage__Buckets__ProposalSource = "proposal-sources"
$env:Supabase__Storage__Buckets__Generic = "generic-uploads"
```

Mapping examples:

```text
ConnectionStrings__DefaultConnection -> ConnectionStrings:DefaultConnection
Jwt__Key -> Jwt:Key
Supabase__ServiceRoleKey -> Supabase:ServiceRoleKey
Supabase__Storage__Buckets__Generic -> Supabase:Storage:Buckets:Generic
```

## Task 1: Remove Real Secrets From `appsettings.json`

**Files:**
- Modify: `MangaManagementSystem.WebApi/appsettings.json`

- [ ] **Step 1: Replace secret values with blanks**

Use this safe structure:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "BoardDecisionDeadlineWorker": {
    "IntervalMinutes": 1
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Issuer": "MangaManagementSystem",
    "Audience": "MangaManagementSystemClient",
    "Key": "",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  },
  "Client": {
    "BaseUrl": "http://localhost:5173"
  },
  "Supabase": {
    "Url": "",
    "ServiceRoleKey": "",
    "Storage": {
      "DefaultBucket": "generic-uploads",
      "Buckets": {
        "ProposalSamplePage": "proposal-pages",
        "ProposalSource": "proposal-sources",
        "Generic": "generic-uploads"
      }
    }
  }
}
```

- [ ] **Step 2: Check that secrets are gone**

Run:

```powershell
rg "Dbsupabase|sb_secret_|THIS_IS_A_VERY_LONG_SECRET_KEY|aws-1-ap-southeast-1.pooler.supabase.com" MangaManagementSystem.WebApi\appsettings.json
```

Expected: no matches.

## Task 2: Add A Required Configuration Helper

**Files:**
- Create: `MangaManagementSystem.WebApi/Configuration/RequiredConfiguration.cs`

- [ ] **Step 1: Create the helper**

Create this file:

```csharp
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.API.Configuration;

public static class RequiredConfiguration
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{key}' is required.");
        }

        return value;
    }
}
```

Why this is useful:

- `builder.Configuration["Jwt:Key"]` returns `null` when missing.
- The current code uses `jwtKey!`, which hides the null problem from the compiler.
- This helper makes the app fail immediately with a clear message.

## Task 3: Make `Program.cs` Read Required Env-Backed Values

**Files:**
- Modify: `MangaManagementSystem.WebApi/Program.cs`

- [ ] **Step 1: Add the helper namespace**

At the top of `Program.cs`, add:

```csharp
using MangaManagementSystem.API.Configuration;
```

- [ ] **Step 2: Replace connection string and JWT reads**

Find:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}
builder.Services.AddDbContext<MangaDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.Register();
var jwtKey = builder.Configuration["Jwt:Key"];
```

Replace it with:

```csharp
var connectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
var jwtIssuer = builder.Configuration.GetRequiredValue("Jwt:Issuer");
var jwtAudience = builder.Configuration.GetRequiredValue("Jwt:Audience");
var jwtKey = builder.Configuration.GetRequiredValue("Jwt:Key");

builder.Services.AddDbContext<MangaDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.Register();
```

- [ ] **Step 3: Use the already-read JWT values**

Find:

```csharp
ValidIssuer = builder.Configuration["Jwt:Issuer"],
ValidAudience = builder.Configuration["Jwt:Audience"],

IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey!)
),
```

Replace it with:

```csharp
ValidIssuer = jwtIssuer,
ValidAudience = jwtAudience,

IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey)
),
```

Result:

- `ConnectionStrings__DefaultConnection` is read as `ConnectionStrings:DefaultConnection`.
- `Jwt__Issuer` is read as `Jwt:Issuer`.
- `Jwt__Audience` is read as `Jwt:Audience`.
- `Jwt__Key` is read as `Jwt:Key`.

## Task 4: Make Supabase Registration Read Required Env-Backed Values

**Files:**
- Modify: `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`

- [ ] **Step 1: Add the helper namespace**

At the top of `ServiceCollection.cs`, add:

```csharp
using MangaManagementSystem.API.Configuration;
```

- [ ] **Step 2: Replace Supabase config reads**

Find:

```csharp
var url = config["Supabase:Url"]!;
var key = config["Supabase:ServiceRoleKey"]!;
```

Replace it with:

```csharp
var url = config.GetRequiredValue("Supabase:Url");
var key = config.GetRequiredValue("Supabase:ServiceRoleKey");
```

Result:

- `Supabase__Url` is read as `Supabase:Url`.
- `Supabase__ServiceRoleKey` is read as `Supabase:ServiceRoleKey`.
- If either one is missing, the API fails with a clear message.

## Task 5: Set Environment Variables Locally

**Files:**
- No code files changed.

- [ ] **Step 1: Set variables for the current PowerShell session**

Run:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=manga_management;Username=postgres;Password=postgres;SSL Mode=Disable"
$env:Jwt__Issuer = "MangaManagementSystem"
$env:Jwt__Audience = "MangaManagementSystemClient"
$env:Jwt__Key = "replace-with-at-least-32-characters-for-hmac-signing"
$env:Jwt__AccessTokenMinutes = "15"
$env:Jwt__RefreshTokenDays = "7"
$env:Client__BaseUrl = "http://localhost:5173"
$env:Supabase__Url = "https://your-project.supabase.co"
$env:Supabase__ServiceRoleKey = "replace-with-service-role-key"
```

These values are saved only in this terminal session.

- [ ] **Step 2: Optionally save variables for your Windows user**

Run:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Port=5432;Database=manga_management;Username=postgres;Password=postgres;SSL Mode=Disable", "User")
[Environment]::SetEnvironmentVariable("Jwt__Issuer", "MangaManagementSystem", "User")
[Environment]::SetEnvironmentVariable("Jwt__Audience", "MangaManagementSystemClient", "User")
[Environment]::SetEnvironmentVariable("Jwt__Key", "replace-with-at-least-32-characters-for-hmac-signing", "User")
[Environment]::SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15", "User")
[Environment]::SetEnvironmentVariable("Jwt__RefreshTokenDays", "7", "User")
[Environment]::SetEnvironmentVariable("Client__BaseUrl", "http://localhost:5173", "User")
[Environment]::SetEnvironmentVariable("Supabase__Url", "https://your-project.supabase.co", "User")
[Environment]::SetEnvironmentVariable("Supabase__ServiceRoleKey", "replace-with-service-role-key", "User")
```

Close and reopen PowerShell after using `SetEnvironmentVariable(..., "User")`.

## Task 6: Verify The Code Reads Environment Variables

**Files:**
- Verify: `MangaManagementSystem.WebApi/Program.cs`
- Verify: `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`

- [ ] **Step 1: Build**

Run:

```powershell
dotnet build MangaManagementSystem.sln
```

Expected: build succeeds.

- [ ] **Step 2: Run the API**

Run:

```powershell
dotnet run --project MangaManagementSystem.WebApi
```

Expected: API starts. If a required variable is missing, the app shows a clear error like:

```text
Configuration value 'Jwt:Key' is required.
```

- [ ] **Step 3: Prove env vars override JSON**

Set a temporary JWT key:

```powershell
$env:Jwt__Key = "temporary-environment-key-1234567890"
dotnet run --project MangaManagementSystem.WebApi
```

Expected: the app uses `temporary-environment-key-1234567890` for JWT signing instead of the empty `Jwt:Key` in `appsettings.json`.

## Task 7: Production Deployment Rule

**Files:**
- No code files changed.

- [ ] **Step 1: Add environment variables in your hosting provider**

Use the same names:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__Key
Jwt__AccessTokenMinutes
Jwt__RefreshTokenDays
Client__BaseUrl
Supabase__Url
Supabase__ServiceRoleKey
Supabase__Storage__DefaultBucket
Supabase__Storage__Buckets__ProposalSamplePage
Supabase__Storage__Buckets__ProposalSource
Supabase__Storage__Buckets__Generic
```

- [ ] **Step 2: Do not rename keys between local and production**

Keep the exact same variable names everywhere. Only the values should differ.

## Self-Review

- Spec coverage: This guide explains where environment variables are saved, how ASP.NET Core reads them, which variables this project needs, and which code changes are required.
- Placeholder scan: No `TBD`, `TODO`, or incomplete implementation instructions remain.
- Type consistency: The helper method is consistently named `GetRequiredValue`, and every code usage matches its signature.

