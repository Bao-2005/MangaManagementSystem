# Implementation Plan: escalation-improvement

## Overview

Five targeted fixes to the escalation subsystem, all confined to Business and WebApi layers with no database migrations required. Changes: restrict POST /api/escalations to EditorInChief, persist Escalation records on Tie/NoQuorum outcomes, fix EscalationProfile type mismatches, inject IMapper into EscalationService, and remove a duplicate DI registration.

## Tasks

- [ ] 1. Fix EscalationProfile mapping
  - [ ] 1.1 Correct `EscalationProfile` to map `CreatedBy`/`ResolvedBy` as `Guid` fields and `CreatorName`/`ResolverName` from navigation property `DisplayName`
    - Open `MangaManagementSystem.Business/Mappers/Profiles/EscalationProfile.cs`
    - Replace the broken `.ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Creator.DisplayName))` with a direct Guid mapping from `src.CreatedBy`
    - Replace the broken `ResolvedBy` mapping with a direct mapping from `src.ResolvedBy`
    - Add `.ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.DisplayName : string.Empty))`
    - Add `.ForMember(dest => dest.ResolverName, opt => opt.MapFrom(src => src.Resolver != null ? src.Resolver.DisplayName : null))`
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [ ]* 1.2 Write property test for EscalationProfile mapping round-trip
    - **Property 2: Escalation mapping round-trip preserves all identity and name fields**
    - Generate random `Escalation` objects with varying `CreatedBy` Guids, `Creator.DisplayName`, null/non-null `Resolver`, `ResolvedBy` Guids
    - Assert `response.CreatedBy == escalation.CreatedBy`, `response.CreatorName == escalation.Creator.DisplayName`, `response.ResolvedBy == escalation.ResolvedBy`, `response.ResolverName == escalation.Resolver?.DisplayName`
    - Validate AutoMapper configuration with `mapper.ConfigurationProvider.AssertConfigurationIsValid()`
    - Run ≥ 100 iterations
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_
    - **Validates: Requirements 3.1, 3.2, 3.3, 3.4**

- [ ] 2. Refactor EscalationService to use IMapper
  - [ ] 2.1 Inject `IMapper` into `EscalationService` and replace static `Map()` calls
    - Open `MangaManagementSystem.Business/Services/Implements/Series/EscalationService.cs`
    - Add `using AutoMapper;` and `using AutoMapper.Extensions.Microsoft.DependencyInjection;`
    - Add `private readonly IMapper _mapper;` field
    - Update the constructor to accept `IMapper mapper` and assign it to `_mapper`
    - In `GetBySeriesAsync`, replace `.Select(e => Map(e)).ToListAsync()` with `.ProjectTo<EscalationResponse>(_mapper.ConfigurationProvider).ToListAsync()` (remove `.Include` for navigations since ProjectTo handles them)
    - In `GetByIdAsync`, replace `return Map(e)` with `return _mapper.Map<EscalationResponse>(e)`
    - In `ResolveAsync`, replace `return Map(e)` with `return _mapper.Map<EscalationResponse>(e)`
    - Delete the private static `Map()` method entirely
    - _Requirements: 4.1, 4.2, 4.3_

  - [ ]* 2.2 Write property test for IMapper-based EscalationService response correctness
    - **Property 3: IMapper-based EscalationService returns correct response fields**
    - Generate random `Escalation` entities and verify `EscalationResponse` fields produced by the IMapper path are identical to the previously defined field mapping contract (no regression)
    - Cover `GetByIdAsync` and `ResolveAsync` paths
    - Run ≥ 100 iterations
    - _Requirements: 4.4, 4.5_
    - **Validates: Requirements 4.4, 4.5**

- [ ] 3. Checkpoint — Ensure AutoMapper and service tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 4. Restrict POST /api/escalations to EditorInChief
  - [ ] 4.1 Add `[Authorize(Policy = "EditorInChiefOnly")]` to the `Create` action in `EscalationController`
    - Open `MangaManagementSystem.WebApi/Controllers/EscalationController.cs`
    - On the `Create` action, replace `[Authorize]` with `[Authorize(Policy = "EditorInChiefOnly")]`
    - Confirm `Resolve` action still has `[Authorize(Policy = "EditorInChiefOnly")]` unchanged
    - _Requirements: 1.1, 1.2, 1.3_

  - [ ]* 4.2 Write unit tests for POST /api/escalations authorization
    - Test that a caller without the `EditorInChief` role receives HTTP 403 on `POST /api/escalations`
    - Test that a caller with the `EditorInChief` role proceeds to service invocation
    - Verify `[Authorize(Policy = "EditorInChiefOnly")]` is still present on `PUT /api/escalations/{id}/resolve`
    - _Requirements: 1.1, 1.2, 1.3_

- [ ] 5. Remove duplicate IEscalationService DI registration
  - [ ] 5.1 Remove the duplicate `IEscalationService` registration from `ServiceCollection`
    - Open `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`
    - Confirm there is currently exactly one `services.AddScoped<IEscalationService, EscalationService>()` line (one registration present)
    - If a second registration exists, remove it so exactly one remains
    - Verify the remaining registration uses `ServiceLifetime.Scoped`
    - _Requirements: 5.1, 5.2_

  - [ ]* 5.2 Write unit test for single IEscalationService DI registration
    - Build the `IServiceCollection` via `ServiceCollection.Register` and inspect the resulting `ServiceDescriptor` list
    - Assert exactly one descriptor is registered for `IEscalationService` with `ServiceLifetime.Scoped`
    - _Requirements: 5.1, 5.2_

- [ ] 6. Persist Escalation on Tie/NoQuorum in BoardDecisionFinalizationService
  - [ ] 6.1 Inject `IEscalationService` into `BoardDecisionFinalizationService` and add `TryCreateEscalationAsync` helper
    - Open `MangaManagementSystem.Business/Services/Implements/Series/BoardDecisionFinalizationService.cs`
    - Add `private readonly IEscalationService _escalationService;` field
    - Update the constructor signature to accept `IEscalationService escalationService` and assign it
    - Add the private `TryCreateEscalationAsync(BoardDecision decision, string outcome)` helper method that builds a `CreateEscalationRequest` with `Type = "BoardDecision"`, `EntityType = "BoardDecision"`, `EntityId = decision.BoardDecisionId`, `SeriesId = decision.SeriesId`, `Priority = "High"`, `Reason = $"Board decision for series '{decision.Series.Title}' ended in {outcome}."` and calls `_escalationService.CreateAsync(decision.CreatedBy, request)`
    - The helper must catch and log all non-cancellation exceptions without rethrowing (swallowed to not block notification dispatch)
    - _Requirements: 2.3, 2.4, 2.5, 2.6_

  - [ ] 6.2 Call `TryCreateEscalationAsync` before `TryNotifyEditorInChiefOfFailedOutcomeAsync` in both NoQuorum and Tie branches of `ProcessDeadlineAsync`
    - In the NoQuorum branch (after the decision is saved and transaction committed), add `await TryCreateEscalationAsync(decision, "no quorum");` immediately before `await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);`
    - In the Tie branch (after the decision is saved and transaction committed), add `await TryCreateEscalationAsync(decision, "a tie");` immediately before `await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);`
    - _Requirements: 2.1, 2.2_

  - [ ]* 6.3 Write property test for escalation persistence on Tie/NoQuorum
    - **Property 1: Escalation is persisted on Tie or NoQuorum outcome**
    - Generate random `BoardDecision` objects with random `BoardDecisionId`, `SeriesId`, and `CreatedBy`
    - Drive `ProcessDeadlineAsync` to Tie or NoQuorum via mocked votes
    - Assert `IEscalationService.CreateAsync` was invoked with arguments satisfying: `Type == "BoardDecision"`, `EntityType == "BoardDecision"`, `EntityId == decision.BoardDecisionId`, `SeriesId == decision.SeriesId`, `Priority == "High"`, `CreatedBy == decision.CreatedBy`
    - Assert `TryNotifyEditorInChiefOfFailedOutcomeAsync` is still called even when `IEscalationService.CreateAsync` throws
    - Run ≥ 100 iterations
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4**

- [ ] 7. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties
- Unit tests validate specific examples and edge cases
- The design uses C# / ASP.NET Core — all code examples and test frameworks should target that stack (e.g., xUnit + FsCheck or CsCheck for property tests)
- No database migrations are required; all changes are in Business and WebApi layers only

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "4.1", "5.1"] },
    { "id": 1, "tasks": ["1.2", "4.2", "5.2", "2.1"] },
    { "id": 2, "tasks": ["2.2", "6.1"] },
    { "id": 3, "tasks": ["6.2"] },
    { "id": 4, "tasks": ["6.3"] }
  ]
}
```
