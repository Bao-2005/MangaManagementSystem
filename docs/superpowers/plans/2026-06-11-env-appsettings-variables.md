# Environment Appsettings Variables Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move runtime secrets out of committed `appsettings.json` and make the Web API read database, JWT, client, and Supabase settings from environment variables with clear validation and documentation.

**Architecture:** ASP.NET Core already loads environment variables after JSON configuration, so double-underscore environment variables such as `ConnectionStrings__DefaultConnection` can override nested appsettings keys without custom providers. The plan keeps non-secret defaults in JSON, replaces committed secrets with blank placeholders, validates required secret settings at startup, and documents local PowerShell setup commands. A small test project verifies configuration binding precedence and required-key validation without starting the full API host.

**Tech Stack:** .NET 8, ASP.NET Core configuration, xUnit, Microsoft.Extensions.Configuration, PowerShell environment variables.

---

## File Structure

- Modify `MangaManagementSystem.WebApi/appsettings.json`
  - Keep non-secret defaults.
  - Replace committed database, JWT signing, and Supabase service role secrets with empty strings.
- Modify `MangaManagementSystem.WebApi/appsettings.Development.json`
  - Add safe local defaults for non-secret developer settings only.
- Create `MangaManagementSystem.WebApi/Configuration/RequiredConfiguration.cs`
  - Centralize validation for required configuration keys.
  - Provide a reusable `GetRequiredValue` helper for `Program.cs` and service registration.
- Modify `MangaManagementSystem.WebApi/Program.cs`
  - Validate required configuration before registering EF Core and JWT auth.
  - Stop using `jwtKey!` on possibly missing config.
- Modify `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`
  - Validate Supabase settings before constructing the Supabase client.
- Create `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`
  - Add an xUnit test project for configuration behavior.
- Create `MangaManagementSystem.Tests/Configuration/RequiredConfigurationTests.cs`
  - Test required value failure behavior and environment variable override behavior.
- Modify `MangaManagementSystem.sln`
  - Add the test project to the solution.
- Modify `README.md`
  - Correct stale project names.
  - Document the required environment variables and local run commands.

## Required Environment Variables

ASP.NET Core maps double underscores to nested configuration sections. These are the variables the app must support:

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

### Task 1: Add Configuration Validation Tests

**Files:**
- Create: `MangaManagementSystem.Tests/MangaManagementSystem.Tests.csproj`
- Create: `MangaManagementSystem.Tests/Configuration/RequiredConfigurationTests.cs`
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
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="xunit" Version="2.8.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MangaManagementSystem.WebApi\MangaManagementSystem.WebApi.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add the test project to the solution**

Run:

```powershell
dotnet sln MangaManagementSystem.sln add MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj
```

Expected: output includes:

```text
Project `MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj` added to the solution.
```

- [ ] **Step 3: Write failing configuration tests**

Create `MangaManagementSystem.Tests/Configuration/RequiredConfigurationTests.cs`:

```csharp
using MangaManagementSystem.API.Configuration;
using Microsoft.Extensions.Configuration;

namespace MangaManagementSystem.Tests.Configuration;

public sealed class RequiredConfigurationTests
{
    [Fact]
    public void GetRequiredValue_ReturnsConfiguredValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "abcdefghijklmnopqrstuvwxyz123456"
            })
            .Build();

        var value = configuration.GetRequiredValue("Jwt:Key");

        Assert.Equal("abcdefghijklmnopqrstuvwxyz123456", value);
    }

    [Fact]
    public void GetRequiredValue_ThrowsWhenValueIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => configuration.GetRequiredValue("Jwt:Key"));

        Assert.Equal("Configuration value 'Jwt:Key' is required.", exception.Message);
    }

    [Fact]
    public void GetRequiredValue_ThrowsWhenValueIsWhitespace()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Supabase:ServiceRoleKey"] = "   "
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => configuration.GetRequiredValue("Supabase:ServiceRoleKey"));

        Assert.Equal("Configuration value 'Supabase:ServiceRoleKey' is required.", exception.Message);
    }

    [Fact]
    public void EnvironmentVariables_OverrideJsonStyleConfigurationKeys()
    {
        const string variableName = "Jwt__Key";
        var previousValue = Environment.GetEnvironmentVariable(variableName);

        try
        {
            Environment.SetEnvironmentVariable(variableName, "environment-secret-key-1234567890");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "json-secret-key"
                })
                .AddEnvironmentVariables()
                .Build();

            Assert.Equal("environment-secret-key-1234567890", configuration.GetRequiredValue("Jwt:Key"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, previousValue);
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj
```

Expected: FAIL because namespace `MangaManagementSystem.API.Configuration` and method `GetRequiredValue` do not exist.

- [ ] **Step 5: Commit**

Run:

```powershell
git add MangaManagementSystem.Tests MangaManagementSystem.sln
git commit -m "test: add configuration validation coverage"
```

Expected: commit succeeds.

### Task 2: Implement Required Configuration Helper

**Files:**
- Create: `MangaManagementSystem.WebApi/Configuration/RequiredConfiguration.cs`
- Test: `MangaManagementSystem.Tests/Configuration/RequiredConfigurationTests.cs`

- [ ] **Step 1: Add required configuration helper**

Create `MangaManagementSystem.WebApi/Configuration/RequiredConfiguration.cs`:

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

- [ ] **Step 2: Run configuration tests**

Run:

```powershell
dotnet test MangaManagementSystem.Tests\MangaManagementSystem.Tests.csproj --filter RequiredConfigurationTests
```

Expected: PASS.

- [ ] **Step 3: Commit**

Run:

```powershell
git add MangaManagementSystem.WebApi\Configuration\RequiredConfiguration.cs MangaManagementSystem.Tests\Configuration\RequiredConfigurationTests.cs
git commit -m "feat: add required configuration helper"
```

Expected: commit succeeds.

### Task 3: Remove Secrets From Appsettings

**Files:**
- Modify: `MangaManagementSystem.WebApi/appsettings.json`
- Modify: `MangaManagementSystem.WebApi/appsettings.Development.json`

- [ ] **Step 1: Replace production appsettings secrets with empty values**

Replace the full contents of `MangaManagementSystem.WebApi/appsettings.json` with:

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

- [ ] **Step 2: Add safe development defaults**

Replace the full contents of `MangaManagementSystem.WebApi/appsettings.Development.json` with:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Client": {
    "BaseUrl": "http://localhost:5173"
  },
  "BoardDecisionDeadlineWorker": {
    "IntervalMinutes": 1
  }
}
```

- [ ] **Step 3: Build after JSON changes**

Run:

```powershell
dotnet build MangaManagementSystem.sln
```

Expected: PASS because JSON files are copied as content but do not require secrets at build time.

- [ ] **Step 4: Confirm secrets are no longer present in appsettings**

Run:

```powershell
rg "Dbsupabase|sb_secret_|aws-1-ap-southeast-1.pooler.supabase.com|THIS_IS_A_VERY_LONG_SECRET_KEY" MangaManagementSystem.WebApi\appsettings*.json
```

Expected: no matches and exit code `1`.

- [ ] **Step 5: Commit**

Run:

```powershell
git add MangaManagementSystem.WebApi\appsettings.json MangaManagementSystem.WebApi\appsettings.Development.json
git commit -m "chore: remove secrets from appsettings"
```

Expected: commit succeeds.

### Task 4: Validate Required Runtime Configuration In Startup

**Files:**
- Modify: `MangaManagementSystem.WebApi/Program.cs`
- Modify: `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`
- Test: `MangaManagementSystem.Tests/Configuration/RequiredConfigurationTests.cs`

- [ ] **Step 1: Update Program.cs to use required values**

In `MangaManagementSystem.WebApi/Program.cs`, add this using near the top:

```csharp
using MangaManagementSystem.API.Configuration;
```

Replace:

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

with:

```csharp
var connectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");
var jwtIssuer = builder.Configuration.GetRequiredValue("Jwt:Issuer");
var jwtAudience = builder.Configuration.GetRequiredValue("Jwt:Audience");
var jwtKey = builder.Configuration.GetRequiredValue("Jwt:Key");

builder.Services.AddDbContext<MangaDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.Register();
```

Replace this JWT validation block:

```csharp
ValidIssuer = builder.Configuration["Jwt:Issuer"],
ValidAudience = builder.Configuration["Jwt:Audience"],

IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey!)
),
```

with:

```csharp
ValidIssuer = jwtIssuer,
ValidAudience = jwtAudience,

IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtKey)
),
```

- [ ] **Step 2: Update Supabase registration to use required values**

In `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`, add this using near the top:

```csharp
using MangaManagementSystem.API.Configuration;
```

Replace:

```csharp
var url = config["Supabase:Url"]!;
var key = config["Supabase:ServiceRoleKey"]!;
```

with:

```csharp
var url = config.GetRequiredValue("Supabase:Url");
var key = config.GetRequiredValue("Supabase:ServiceRoleKey");
```

- [ ] **Step 3: Build startup changes**

Run:

```powershell
dotnet build MangaManagementSystem.sln
```

Expected: PASS.

- [ ] **Step 4: Run tests**

Run:

```powershell
dotnet test MangaManagementSystem.sln
```

Expected: PASS.

- [ ] **Step 5: Commit**

Run:

```powershell
git add MangaManagementSystem.WebApi\Program.cs MangaManagementSystem.WebApi\Extensions\ServiceCollection.cs
git commit -m "feat: validate required runtime configuration"
```

Expected: commit succeeds.

### Task 5: Document Environment Setup

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Replace stale configuration and run sections**

In `README.md`, replace the existing `## Configuration` and `## Getting Started` sections with:

```markdown
## Configuration

The Web API project is `MangaManagementSystem.WebApi`. Runtime secrets are not stored in `appsettings.json`; configure them with environment variables before running the API.

ASP.NET Core maps double underscores (`__`) to nested configuration keys. For PowerShell local development:

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

Use platform environment settings for deployed environments such as Azure App Service, Docker, GitHub Actions, or Vercel-connected deployment pipelines. Keep `Supabase__ServiceRoleKey`, `Jwt__Key`, and `ConnectionStrings__DefaultConnection` out of source control.

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL database
- Supabase project and storage buckets if file upload endpoints are used

### Restore and Build

```powershell
dotnet restore
dotnet build MangaManagementSystem.sln
```

### Run Tests

```powershell
dotnet test MangaManagementSystem.sln
```

### Run the API

Configure the required environment variables first, then run:

```powershell
dotnet run --project MangaManagementSystem.WebApi
```

Development launch profiles expose:

- HTTP: `http://localhost:5151`
- HTTPS: `https://localhost:7059`
- Swagger UI: `/swagger`
```

- [ ] **Step 2: Fix stale project name references**

Replace remaining references to `MangaManagementSystem.API/` and `MangaManagementSystem.API.csproj` in `README.md` with `MangaManagementSystem.WebApi/` and `MangaManagementSystem.WebApi.csproj`.

- [ ] **Step 3: Verify README no longer references the old API path**

Run:

```powershell
rg "MangaManagementSystem\.API|SQL Server" README.md
```

Expected: no matches and exit code `1`.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md
git commit -m "docs: document environment configuration"
```

Expected: commit succeeds.

### Task 6: Final Verification

**Files:**
- Verify: full solution and secret scan

- [ ] **Step 1: Build the solution**

Run:

```powershell
dotnet build MangaManagementSystem.sln
```

Expected: PASS.

- [ ] **Step 2: Run all tests**

Run:

```powershell
dotnet test MangaManagementSystem.sln
```

Expected: PASS.

- [ ] **Step 3: Scan tracked text files for the removed secrets**

Run:

```powershell
rg "Dbsupabase|sb_secret_|THIS_IS_A_VERY_LONG_SECRET_KEY|aws-1-ap-southeast-1.pooler.supabase.com" .
```

Expected: no matches outside git history and this implementation plan's verification command text. If matches appear in runtime configuration or documentation examples, remove them.

- [ ] **Step 4: Verify API starts with explicit env vars**

Run these in the same PowerShell session:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=manga_management;Username=postgres;Password=postgres;SSL Mode=Disable"
$env:Jwt__Issuer = "MangaManagementSystem"
$env:Jwt__Audience = "MangaManagementSystemClient"
$env:Jwt__Key = "replace-with-at-least-32-characters-for-hmac-signing"
$env:Supabase__Url = "https://your-project.supabase.co"
$env:Supabase__ServiceRoleKey = "replace-with-service-role-key"
dotnet run --project MangaManagementSystem.WebApi --no-build
```

Expected: application starts and logs listening URLs. If the local database or placeholder Supabase credentials are not reachable, startup may fail later when those clients are used; missing configuration should not be the failure.

- [ ] **Step 5: Commit final verification notes if any docs changed**

Run:

```powershell
git status --short
```

Expected: no uncommitted changes. If verification required README correction, commit it with:

```powershell
git add README.md
git commit -m "docs: clarify local environment setup"
```

## Self-Review

- Spec coverage: The plan covers environment-variable configuration for appsettings, removal of committed secrets, required runtime validation, tests, and README setup instructions.
- Placeholder scan: No `TBD`, `TODO`, “implement later,” or unspecified code steps remain.
- Type consistency: `RequiredConfiguration.GetRequiredValue(this IConfiguration configuration, string key)` is used consistently from `Program.cs`, `ServiceCollection.cs`, and tests.

