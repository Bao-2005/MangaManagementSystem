# Manga Management System

Manga Management System is a .NET 8 solution for managing manga production data. The current codebase is structured as a layered ASP.NET Core Web API with a Business layer and an Entity Framework Core DataAccess layer.

## Solution Structure

```text
MangaManagementSystem/
├── MangaManagementSystem.API/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Middleware/
│   ├── Program.cs
│   ├── appsettings.json
│   └── MangaManagementSystem.API.csproj
├── MangaManagementSystem.Business/
│   ├── DTOs/
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Mappers/
│   ├── Services/
│   │   ├── Interfaces/
│   │   └── Implements/
│   └── MangaManagementSystem.Business.csproj
├── MangaManagementSystem.DataAccess/
│   ├── DbContext/
│   ├── Entities/
│   │   ├── Enums/
│   │   └── Models/
│   ├── Repositories/
│   │   ├── Implements/
│   │   └── Interfaces/
│   └── MangaManagementSystem.DataAccess.csproj
├── MangaManagementSystem.sln
└── README.md
```

## Projects

| Project                            | Purpose                                                                                                                          |
| ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `MangaManagementSystem.API`        | ASP.NET Core Web API entry point. Registers controllers, Swagger, SQL Server EF Core, repositories, and business infrastructure. |
| `MangaManagementSystem.Business`   | Business layer for DTOs, services, and AutoMapper registration. Some folders are scaffolded for future implementation.           |
| `MangaManagementSystem.DataAccess` | EF Core data model, `MangaDbContext`, entity configurations, and a generic repository implementation.                            |

## Domain Model

The DataAccess layer currently defines these main entities:

- `User`, `Role`, `UserRole`
- `Series`, `Chapter`, `ChapterPage`
- `Manuscript`
- `FileAsset`
- `PageTask`, `PageTaskSubmission`
- `Annotation`

`MangaDbContext` configures table mappings, required fields, indexes, relationships, and delete behavior for the manga workflow.

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- AutoMapper
- Swagger / Swashbuckle

## Configuration

The API reads the database connection string from `MangaManagementSystem.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=*****;Database=MangaManagementSystemDb;User Id=*****;Password=*****;TrustServerCertificate=True;"
}
```

Update `DefaultConnection` for your local SQL Server instance before running the API.

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server container

### Restore and Build

```bash
dotnet restore
dotnet build
```

### Run the API

```bash
dotnet run --project MangaManagementSystem.API
```

Development launch profiles expose:

- HTTP: `http://localhost:5151`
- HTTPS: `https://localhost:7059`
- Swagger UI: `/swagger`

## Current Notes

- The API currently includes the default `WeatherForecastController`.
- Business DTO and service folders are prepared but not yet populated.
- EF Core entity configuration is present, but migrations are not included in the current tree.
- A generic repository is registered through dependency injection as `IRepository<T>`.
