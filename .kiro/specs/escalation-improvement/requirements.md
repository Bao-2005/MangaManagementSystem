# Requirements Document

## Introduction

This feature addresses four targeted improvements to the escalation subsystem of the MangaManagementSystem API. The changes tighten access control on the escalation creation endpoint, ensure that `BoardDecisionFinalizationService` persists an `Escalation` record before sending notifications on Tie and NoQuorum outcomes, correct type mismatches in `EscalationProfile`, and wire `EscalationService` to use AutoMapper via `IMapper`. A duplicate `IEscalationService` DI registration is also removed. No new entities or database migrations are required.

## Glossary

- **EscalationController**: The ASP.NET Core controller at `MangaManagementSystem.API.Controllers.EscalationController` that exposes the escalation REST endpoints.
- **EscalationService**: The service implementation `MangaManagementSystem.Business.Services.Implements.Series.EscalationService` that handles CRUD operations for `Escalation` entities.
- **IEscalationService**: The interface `MangaManagementSystem.Business.Services.Interfaces.Series.IEscalationService` that defines the escalation service contract.
- **EscalationProfile**: The AutoMapper profile `MangaManagementSystem.Business.Mappers.Profiles.EscalationProfile` that defines the mapping from `Escalation` entity to `EscalationResponse` DTO.
- **EscalationResponse**: The DTO `MangaManagementSystem.Business.DTOs.Responses.Series.EscalationResponse` returned by escalation queries.
- **BoardDecisionFinalizationService**: The service `MangaManagementSystem.Business.Services.Implements.Series.BoardDecisionFinalizationService` that finalizes board decisions and handles Tie and NoQuorum outcomes.
- **INotificationDispatchService**: The service interface used by `BoardDecisionFinalizationService` to send notifications to the Editor-in-Chief.
- **EditorInChiefOnly Policy**: The ASP.NET Core authorization policy that restricts access to users with the `EditorInChief` role.
- **ServiceCollection**: The static extension class `MangaManagementSystem.API.Extensions.ServiceCollection` that registers all dependency-injection services.
- **IMapper**: The AutoMapper `IMapper` interface injected into services to perform object-to-object mapping.
- **Escalation**: The entity model `MangaManagementSystem.DataAccess.Entities.Models.Escalation` representing an escalation record in the database.

## Requirements

### Requirement 1

**User Story:** As an API security owner, I want the POST /api/escalations endpoint restricted to EditorInChief users, so that arbitrary callers cannot raise escalations directly through the REST API.

#### Acceptance Criteria

1. WHEN a request is received at `POST /api/escalations`, THE `EscalationController` SHALL require the `EditorInChiefOnly` authorization policy before processing the request body.
2. IF a caller without the `EditorInChief` role sends a request to `POST /api/escalations`, THEN THE `EscalationController` SHALL return an HTTP 403 Forbidden response.
3. WHILE the `EditorInChiefOnly` policy is applied to `POST /api/escalations`, THE `EscalationController` SHALL continue to require the `EditorInChiefOnly` authorization policy on the existing `PUT /api/escalations/{id}/resolve` endpoint without change.

---

### Requirement 2

**User Story:** As a system operator, I want `BoardDecisionFinalizationService` to persist an `Escalation` record whenever a board decision ends in a Tie or NoQuorum, so that the Editor-in-Chief has a traceable escalation entity to act on, not just a transient notification.

#### Acceptance Criteria

1. WHEN `BoardDecisionFinalizationService` sets a board decision result to `NoQuorum`, THE `BoardDecisionFinalizationService` SHALL create and persist an `Escalation` record referencing the affected `BoardDecision` and `Series` before dispatching the notification to the Editor-in-Chief.
2. WHEN `BoardDecisionFinalizationService` sets a board decision result to `Tie`, THE `BoardDecisionFinalizationService` SHALL create and persist an `Escalation` record referencing the affected `BoardDecision` and `Series` before dispatching the notification to the Editor-in-Chief.
3. THE `BoardDecisionFinalizationService` SHALL set the new `Escalation` record's `Type` to `"BoardDecision"`, `EntityType` to `"BoardDecision"`, `EntityId` to the `BoardDecisionId`, `SeriesId` to the decision's `SeriesId`, `Priority` to `"High"`, `Status` to `"Open"`, and `Reason` to a human-readable description of the Tie or NoQuorum outcome.
4. THE `BoardDecisionFinalizationService` SHALL set `Escalation.CreatedBy` to the `BoardDecision.CreatedBy` field to record the user who originated the decision that required escalation.
5. IF the `Escalation` record creation fails, THEN THE `BoardDecisionFinalizationService` SHALL log the failure and continue to attempt the notification dispatch so that the Editor-in-Chief is still alerted even when persistence fails.
6. THE `BoardDecisionFinalizationService` SHALL depend on `IEscalationService` injected through its constructor to create escalation records, so that escalation creation follows the same service layer contract used elsewhere in the system.

---

### Requirement 3

**User Story:** As a developer, I want `EscalationProfile` to map `Escalation.CreatedBy` (a `Guid`) to `EscalationResponse.CreatedBy` (a `Guid`) and `Escalation.Creator.DisplayName` to `EscalationResponse.CreatorName`, so that the AutoMapper configuration matches the DTO property types and names without runtime errors.

#### Acceptance Criteria

1. THE `EscalationProfile` SHALL map `Escalation.CreatedBy` directly to `EscalationResponse.CreatedBy` so that the `CreatedBy` field in the response carries the creator's `Guid` identifier.
2. THE `EscalationProfile` SHALL map `Escalation.Creator.DisplayName` to `EscalationResponse.CreatorName` so that the human-readable creator name is available in the response.
3. THE `EscalationProfile` SHALL map `Escalation.ResolvedBy` directly to `EscalationResponse.ResolvedBy` so that the `ResolvedBy` field carries the resolver's `Guid` identifier or `null` when unresolved.
4. THE `EscalationProfile` SHALL map `Escalation.Resolver.DisplayName` to `EscalationResponse.ResolverName` when the `Resolver` navigation property is non-null, and map `null` to `EscalationResponse.ResolverName` when `Resolver` is null.
5. WHEN the AutoMapper configuration is validated at application startup, THE `EscalationProfile` SHALL produce no type-mismatch or unmapped-member configuration errors for the `Escalation` to `EscalationResponse` mapping.

---

### Requirement 4

**User Story:** As a developer, I want `EscalationService` to use the injected `IMapper` for object mapping instead of the private static `Map()` method, so that the `EscalationProfile` AutoMapper configuration is exercised consistently and the service is simpler to maintain.

#### Acceptance Criteria

1. THE `EscalationService` SHALL accept `IMapper` as a constructor parameter and store it as a private field for use in all mapping operations.
2. THE `EscalationService` SHALL use `_mapper.Map<EscalationResponse>(entity)` in place of every call to the static `Map()` method.
3. THE `EscalationService` SHALL remove the private static `Map()` method after replacing all its call sites with `IMapper` calls.
4. WHEN `EscalationService.GetBySeriesAsync` is called, THE `EscalationService` SHALL return the same `EscalationResponse` field values as previously returned by the static `Map()` method, ensuring no regression in response content.
5. WHEN `EscalationService.ResolveAsync` is called on an entity that has just been updated, THE `EscalationService` SHALL map the updated `Escalation` entity through `IMapper` before returning the response so that the resolver's name and identifier are reflected in the result.

---

### Requirement 5

**User Story:** As a developer, I want the duplicate `IEscalationService` DI registration removed from `ServiceCollection`, so that the dependency injection container has a single, unambiguous registration for `IEscalationService`.

#### Acceptance Criteria

1. THE `ServiceCollection` SHALL contain exactly one registration of `IEscalationService` mapped to `EscalationService` after the duplicate is removed.
2. WHEN the application starts, THE `ServiceCollection` SHALL register `IEscalationService` with scoped lifetime so that each HTTP request receives its own `EscalationService` instance.
