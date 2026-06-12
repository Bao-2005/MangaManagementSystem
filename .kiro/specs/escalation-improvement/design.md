# Design Document

## Feature: escalation-improvement

## Overview

This design describes four targeted improvements to the escalation subsystem:

1. Restrict `POST /api/escalations` to the `EditorInChiefOnly` authorization policy.
2. Persist an `Escalation` record in `BoardDecisionFinalizationService` whenever a board decision ends in Tie or NoQuorum, before dispatching the notification.
3. Fix `EscalationProfile` to map `CreatedBy`/`ResolvedBy` as `Guid` fields and map display names to `CreatorName`/`ResolverName`.
4. Refactor `EscalationService` to inject and use `IMapper` instead of the private static `Map()` method.
5. Remove the duplicate `IEscalationService` DI registration from `ServiceCollection`.

No new database entities or migrations are required. All changes are confined to the Business and WebApi layers.

---

## Architecture

The system follows a layered architecture:

```
WebApi (Controllers, Authorization)
    │
    ▼
Business (Services, DTOs, AutoMapper Profiles)
    │
    ▼
DataAccess (Entities, Repositories)
```

The five improvements are isolated to specific files:

| Area | File | Change |
|---|---|---|
| Authorization | `EscalationController.cs` | Add `[Authorize(Policy = "EditorInChiefOnly")]` to `Create` action |
| Persistence | `BoardDecisionFinalizationService.cs` | Inject `IEscalationService`; create `Escalation` before notifying |
| AutoMapper profile | `EscalationProfile.cs` | Correct field mappings for `CreatedBy`, `CreatorName`, `ResolvedBy`, `ResolverName` |
| Service mapping | `EscalationService.cs` | Inject `IMapper`; replace static `Map()` calls |
| DI registration | `ServiceCollection.cs` | Remove duplicate `IEscalationService` registration |

---

## Components and Interfaces

### 1. EscalationController — Authorization Fix

**Current state:** The `Create` action uses `[Authorize]` (any authenticated user).

**Target state:** The `Create` action uses `[Authorize(Policy = "EditorInChiefOnly")]`.

```csharp
[HttpPost("api/escalations")]
[Authorize(Policy = "EditorInChiefOnly")]
[SwaggerOperation(Summary = "Raise an escalation")]
public async Task<IActionResult> Create([FromBody] CreateEscalationRequest request)
{
    var userId = GetUserId() ?? throw new UnauthorizedAccessException();
    var result = await _service.CreateAsync(userId, request);
    return CreatedAtAction(nameof(GetById), new { id = result.EscalationId },
        new BaseResponse { Data = result, Message = "Escalation raised." });
}
```

The `Resolve` action already uses `[Authorize(Policy = "EditorInChiefOnly")]` and must remain unchanged.

---

### 2. BoardDecisionFinalizationService — Escalation Persistence

**Current state:** When a board decision finalizes as Tie or NoQuorum, the service calls `TryNotifyEditorInChiefOfFailedOutcomeAsync` but never persists an `Escalation` record.

**Target state:** A new private helper `TryCreateEscalationAsync` is called before notification dispatch. It builds a `CreateEscalationRequest` and calls `_escalationService.CreateAsync(decision.CreatedBy, request)`.

#### Constructor change

```csharp
public BoardDecisionFinalizationService(
    IRepository<BoardDecision> decisionRepo,
    IRepository<Series> seriesRepo,
    IRepository<UserAssignment> assignmentRepo,
    INotificationDispatchService notificationDispatchService,
    IEscalationService escalationService,   // NEW
    MangaDbContext dbContext,
    ILogger<BoardDecisionFinalizationService> logger)
```

#### New helper method

```csharp
private async Task TryCreateEscalationAsync(BoardDecision decision, string outcome)
{
    try
    {
        var request = new CreateEscalationRequest
        {
            Type       = "BoardDecision",
            EntityType = "BoardDecision",
            EntityId   = decision.BoardDecisionId,
            SeriesId   = decision.SeriesId,
            Priority   = "High",
            Reason     = $"Board decision for series '{decision.Series.Title}' ended in {outcome}."
        };
        await _escalationService.CreateAsync(decision.CreatedBy, request);
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Failed to create escalation for board decision {BoardDecisionId} with outcome {Outcome}.",
            decision.BoardDecisionId, outcome);
    }
}
```

#### Call sites in `ProcessDeadlineAsync`

Before each `TryNotifyEditorInChiefOfFailedOutcomeAsync` call (NoQuorum and Tie branches):

```csharp
// NoQuorum branch
await TryCreateEscalationAsync(decision, "no quorum");
await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);

// Tie branch
await TryCreateEscalationAsync(decision, "a tie");
await TryNotifyEditorInChiefOfFailedOutcomeAsync(decision);
```

The `Status` field of the new escalation is set to `"Open"` inside `EscalationService.CreateAsync` (no change required there).

---

### 3. EscalationProfile — Mapping Fix

**Current state (broken):** `CreatedBy` is mapped from `Creator.DisplayName` (a `string`) to `EscalationResponse.CreatedBy` (a `Guid`) — type mismatch. `ResolvedBy` is similarly incorrect.

**Target state:**

```csharp
public class EscalationProfile : Profile
{
    public EscalationProfile()
    {
        CreateMap<Escalation, EscalationResponse>()
            // Guid identity fields — map directly
            .ForMember(dest => dest.CreatedBy,
                opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.ResolvedBy,
                opt => opt.MapFrom(src => src.ResolvedBy))
            // Human-readable display names
            .ForMember(dest => dest.CreatorName,
                opt => opt.MapFrom(src => src.Creator != null ? src.Creator.DisplayName : string.Empty))
            .ForMember(dest => dest.ResolverName,
                opt => opt.MapFrom(src => src.Resolver != null ? src.Resolver.DisplayName : null));
    }
}
```

---

### 4. EscalationService — IMapper Injection

**Current state:** `EscalationService` takes only `IRepository<Escalation>` and has a private static `Map()` method.

**Target state:** `IMapper` is injected; the static `Map()` method is removed; all mapping calls use `_mapper.Map<EscalationResponse>(entity)`.

```csharp
public class EscalationService : IEscalationService
{
    private readonly IRepository<Escalation> _repo;
    private readonly IMapper _mapper;

    public EscalationService(IRepository<Escalation> repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EscalationResponse>> GetBySeriesAsync(Guid seriesId)
        => await _repo.GetAll()
            .Include(e => e.Creator)
            .Include(e => e.Resolver)
            .Where(e => e.SeriesId == seriesId && e.DeletedAt == null)
            .ProjectTo<EscalationResponse>(_mapper.ConfigurationProvider)
            .ToListAsync();

    public async Task<EscalationResponse> GetByIdAsync(Guid id)
    {
        var e = await _repo.GetAll()
            .Include(e => e.Creator)
            .Include(e => e.Resolver)
            .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
            ?? throw new KeyNotFoundException("Escalation not found.");
        return _mapper.Map<EscalationResponse>(e);
    }

    public async Task<EscalationResponse> CreateAsync(Guid createdByUserId, CreateEscalationRequest request)
    {
        var esc = new Escalation
        {
            Type       = request.Type,
            EntityType = request.EntityType,
            EntityId   = request.EntityId,
            SeriesId   = request.SeriesId,
            Priority   = request.Priority,
            Reason     = request.Reason,
            Status     = "Open",
            CreatedBy  = createdByUserId,
            CreatedAt  = DateTime.UtcNow
        };
        await _repo.AddAsync(esc);
        await _repo.SaveChangeAsync();
        return await GetByIdAsync(esc.EscalationId);
    }

    public async Task<EscalationResponse> ResolveAsync(Guid id, Guid resolverUserId, UpdateEscalationRequest request)
    {
        var e = await _repo.GetAll()
            .Include(e => e.Creator)
            .Include(e => e.Resolver)
            .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
            ?? throw new KeyNotFoundException("Escalation not found.");
        if (request.Status     != null) e.Status     = request.Status;
        if (request.Resolution != null) e.Resolution = request.Resolution;
        e.ResolvedBy = resolverUserId;
        e.ResolvedAt = DateTime.UtcNow;
        _repo.Update(e);
        await _repo.SaveChangeAsync();
        return _mapper.Map<EscalationResponse>(e);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var e = await _repo.GetAll()
            .FirstOrDefaultAsync(x => x.EscalationId == id && x.DeletedAt == null)
            ?? throw new KeyNotFoundException("Escalation not found.");
        e.DeletedAt = DateTime.UtcNow;
        _repo.Update(e);
        await _repo.SaveChangeAsync();
    }
}
```

Note: `GetBySeriesAsync` can use AutoMapper's `ProjectTo<T>` extension (from `AutoMapper.Extensions.Microsoft.DependencyInjection`) to push projection to the database query level, which is more efficient and avoids loading navigation properties that aren't needed.

---

### 5. ServiceCollection — Remove Duplicate DI Registration

**Current state:** `IEscalationService` is registered once explicitly; a second implicit or explicit registration may exist.

**Target state:** Exactly one `services.AddScoped<IEscalationService, EscalationService>()` line exists in `ServiceCollection.Register`.

---

## Data Models

No new entity or database changes are required.

### Escalation fields set by BoardDecisionFinalizationService

| Field | Value |
|---|---|
| `Type` | `"BoardDecision"` |
| `EntityType` | `"BoardDecision"` |
| `EntityId` | `decision.BoardDecisionId` |
| `SeriesId` | `decision.SeriesId` |
| `Priority` | `"High"` |
| `Status` | `"Open"` (set by `EscalationService.CreateAsync`) |
| `Reason` | Human-readable string describing Tie or NoQuorum |
| `CreatedBy` | `decision.CreatedBy` |
| `CreatedAt` | `DateTime.UtcNow` (set by `EscalationService.CreateAsync`) |

---

## Error Handling

### Escalation persistence failure in BoardDecisionFinalizationService

- The `TryCreateEscalationAsync` method wraps the call in a try/catch.
- `OperationCanceledException` is re-thrown to respect cancellation tokens.
- All other exceptions are logged at `Error` level and swallowed, so notification dispatch is not blocked.

### AutoMapper configuration errors

- `EscalationProfile` must pass `AssertConfigurationIsValid()` at startup. The corrected mapping removes all type-mismatch sources.

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Escalation is persisted on Tie or NoQuorum outcome

*For any* board decision that `BoardDecisionFinalizationService` finalizes with a `Tie` or `NoQuorum` result, an `Escalation` record SHALL be created with `Type = "BoardDecision"`, `EntityType = "BoardDecision"`, `EntityId = BoardDecisionId`, `SeriesId = decision.SeriesId`, `Priority = "High"`, `Status = "Open"`, and `CreatedBy = decision.CreatedBy`.

**Validates: Requirements 2.1, 2.2, 2.3, 2.4**

---

### Property 2: Escalation mapping round-trip preserves all identity and name fields

*For any* `Escalation` entity with non-null `Creator` and any combination of null/non-null `Resolver`, mapping through the `EscalationProfile` AutoMapper configuration SHALL produce an `EscalationResponse` where:
- `CreatedBy` equals `escalation.CreatedBy` (Guid),
- `CreatorName` equals `escalation.Creator.DisplayName`,
- `ResolvedBy` equals `escalation.ResolvedBy` (Guid or null),
- `ResolverName` equals `escalation.Resolver?.DisplayName` (string or null).

**Validates: Requirements 3.1, 3.2, 3.3, 3.4**

---

### Property 3: IMapper-based EscalationService returns correct response fields

*For any* `Escalation` entity persisted and retrieved via `EscalationService.GetBySeriesAsync` or `EscalationService.GetByIdAsync`, the returned `EscalationResponse` SHALL have `CreatedBy`, `CreatorName`, `ResolvedBy`, and `ResolverName` values consistent with Property 2 (i.e., the IMapper path produces no regression relative to the previously defined field mapping contract).

**Validates: Requirements 4.4, 4.5**


---

## Testing Strategy

### Unit Tests (Example-Based)

- **Authorization on POST /api/escalations**: Verify that a non-EditorInChief token receives HTTP 403 and an EditorInChief token proceeds to service invocation.
- **Resolve endpoint unchanged**: Verify `[Authorize(Policy = "EditorInChiefOnly")]` is still present on `PUT /api/escalations/{id}/resolve`.
- **Escalation persistence failure resilience**: Mock `IEscalationService.CreateAsync` to throw; verify `TryNotifyEditorInChiefOfFailedOutcomeAsync` is still called and the exception is logged.
- **AutoMapper configuration validity**: Call `mapper.ConfigurationProvider.AssertConfigurationIsValid()` to confirm no type-mismatch or unmapped-member errors exist in `EscalationProfile`.
- **Single DI registration**: Inspect the `IServiceCollection` built by `ServiceCollection.Register` and assert exactly one `ServiceDescriptor` is registered for `IEscalationService` with `ServiceLifetime.Scoped`.

### Property-Based Tests

- **Property 1 — Escalation persistence on Tie/NoQuorum**: Generate random `BoardDecision` objects with random `BoardDecisionId`, `SeriesId`, and `CreatedBy` values, drive `ProcessDeadlineAsync` to a Tie or NoQuorum outcome (via mocked votes), and assert that `IEscalationService.CreateAsync` was invoked with arguments satisfying the field invariants. Run ≥ 100 iterations.
  - Tag: `Feature: escalation-improvement, Property 1: Escalation is persisted on Tie or NoQuorum outcome`

- **Property 2 — Mapping round-trip**: Generate random `Escalation` entities with varying `CreatedBy` Guids, `Creator.DisplayName` strings, null/non-null `Resolver`, and `ResolvedBy` Guids. Map each entity through `EscalationProfile` and assert all four identity/name fields match. Run ≥ 100 iterations.
  - Tag: `Feature: escalation-improvement, Property 2: Escalation mapping round-trip preserves all identity and name fields`

- **Property 3 — IMapper service response correctness**: Generate random `Escalation` entities and assert that `EscalationService` returns `EscalationResponse` values consistent with the mapping contract (no regression from replacing the static `Map()` method). Run ≥ 100 iterations.
  - Tag: `Feature: escalation-improvement, Property 3: IMapper-based EscalationService returns correct response fields`
