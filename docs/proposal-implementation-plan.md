# Proposal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the proposal flow from Mangaka upload through Tantou review annotations, editorial board voting, escalation handover, activation, and chapter creation gates.

**Architecture:** Keep the existing layered structure: Controller -> Business service -> Repository/DataAccess -> PostgreSQL. Add workflow services around the existing CRUD model so proposal, board decision, escalation, and chapter gates are enforced in the business layer instead of controllers.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core 8, PostgreSQL via Npgsql, AutoMapper, Swagger, existing repository pattern, Supabase-backed `FileAsset` metadata.

---

## Business Rule Scope

This plan covers the business-rule flow requested by the user:

- Series proposal: BR-14, BR-15, BR-17, BR-18, BR-19, BR-21, BR-24.
- Editorial board voting: BR-27, BR-28, BR-29, BR-30, BR-31, BR-33, BR-34, BR-35, BR-37.
- Chapter creation through BR-46: BR-40, BR-41, BR-42, BR-43, BR-46.
- Cross-cutting rules used by the flow: BR-03, BR-04, BR-06, BR-07, BR-128, BR-129, BR-135.
- Page-task rules from BR-49 onward are out of scope except where BR-46 depends on page-task approval status.

## Current State

- Existing entities already include `Series`, `ProposalPage`, `BoardDecision`, `BoardVote`, `Escalation`, `FileAsset`, and manuscript `Annotation`.
- Existing services/controllers are mostly CRUD and do not enforce the full proposal/voting workflow.
- Existing proposal creation stores `Series.Status = "Proposed"`, but business rules require proposal lifecycle states: `Draft -> UnderReview -> Approved | Rejected | Expired`.
- Existing `CreateSeriesRequest` conflicts with BR-15: title allows 150 chars and synopsis requires minimum 200 chars. BR-15 requires title `<= 100` and synopsis `100-2000`.
- Existing `Annotation` belongs to `Manuscript`, so proposal page annotation should be added separately instead of overloading manuscript annotation.
- Existing board vote logic blocks duplicate votes only. It does not enforce eligibility, conflict of interest, quorum, majority, reject reason, expiration, or escalation.

## Future Changes

- Add a dedicated proposal workflow instead of relying on general `SeriesController` CRUD updates.
- Add file upload endpoints for proposal source zip and sample-page images.
- Add proposal-page annotations with FE-supplied coordinates and comments.
- Add Tantou review actions: submit to board, activate approved proposal, escalate expired/tied/no-quorum decisions.
- Add board voting finalization with quorum and majority calculation.
- Add Editor-in-Chief resolution for escalated proposal decisions.
- Add audit log and optimistic concurrency support for governance-sensitive entities.
- Update API documentation so FE uses the new workflow endpoints and BR-15 values.

## File Impact Map

### Files to Create

- `MangaManagementSystem.DataAccess/Entities/Models/ProposalAnnotation.cs`
  - Stores Tantou annotation comments for proposal sample pages.
- `MangaManagementSystem.DataAccess/Entities/Models/AuditLog.cs`
  - Stores immutable audit entries for BR-128 and BR-129.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateProposalRequest.cs`
  - Multipart proposal creation fields: title, synopsis, publication type, genres, source zip, sample pages.
- `MangaManagementSystem.Business/DTOs/Requests/Series/SubmitProposalToBoardRequest.cs`
  - Optional Tantou note when submitting proposal to board.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateProposalAnnotationRequest.cs`
  - Proposal page annotation coordinates and comment.
- `MangaManagementSystem.Business/DTOs/Requests/Series/UpdateProposalAnnotationRequest.cs`
  - Proposal annotation content/position update payload.
- `MangaManagementSystem.Business/DTOs/Requests/Series/ResolveBoardEscalationRequest.cs`
  - Editor-in-Chief approve/reject resolution payload.
- `MangaManagementSystem.Business/DTOs/Responses/Series/ProposalAnnotationResponse.cs`
  - Proposal annotation response shape.
- `MangaManagementSystem.Business/DTOs/Responses/Series/BoardDecisionSummaryResponse.cs`
  - Vote summary: approve count, reject count, valid vote count, quorum flag, current result.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IProposalWorkflowService.cs`
  - Proposal creation, submit review, submit board, activation, and escalation workflow contract.
- `MangaManagementSystem.Business/Services/Implements/Series/ProposalWorkflowService.cs`
  - Business-rule implementation for proposal workflow.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IProposalAnnotationService.cs`
  - Proposal annotation contract.
- `MangaManagementSystem.Business/Services/Implements/Series/ProposalAnnotationService.cs`
  - Proposal annotation implementation.
- `MangaManagementSystem.Business/Services/Interfaces/IFileAssetService.cs`
  - File metadata creation contract.
- `MangaManagementSystem.Business/Services/Implements/FileAssetService.cs`
  - File metadata implementation.
- `MangaManagementSystem.Business/Services/Interfaces/IStorageService.cs`
  - Storage abstraction for uploaded files.
- `MangaManagementSystem.Business/Services/Implements/SupabaseStorageService.cs`
  - Supabase upload implementation or local development-compatible adapter.
- `MangaManagementSystem.Business/Services/Interfaces/IAuditLogService.cs`
  - Audit log creation contract.
- `MangaManagementSystem.Business/Services/Implements/AuditLogService.cs`
  - Audit log implementation.
- `MangaManagementSystem.WebApi/Controllers/ProposalWorkflowController.cs`
  - Workflow endpoints for proposal creation, review, board submission, activation, and escalation.
- `MangaManagementSystem.WebApi/Controllers/ProposalAnnotationController.cs`
  - Proposal page annotation endpoints.

### Files to Modify

- `MangaManagementSystem.DataAccess/DbContext/MangaDbContext.cs`
  - Add `DbSet<ProposalAnnotation>`, `DbSet<AuditLog>`, entity mappings, relationships, indexes, delete behavior, and board decision concurrency token.
- `MangaManagementSystem.DataAccess/Entities/Models/Series.cs`
  - Keep existing proposal-related fields but align status use with proposal lifecycle constants.
- `MangaManagementSystem.DataAccess/Entities/Models/ProposalPage.cs`
  - Add `ICollection<ProposalAnnotation>` navigation.
- `MangaManagementSystem.DataAccess/Entities/Models/BoardDecision.cs`
  - Add optimistic concurrency field such as `RowVersion` or PostgreSQL-compatible concurrency token.
- `MangaManagementSystem.DataAccess/Entities/Models/User.cs`
  - Add navigation collections for `ProposalAnnotation` and `AuditLog` if needed.
- `MangaManagementSystem.DataAccess/Entities/Enums/SeriesStatus.cs`
  - Align values with canonical active series lifecycle.
- `MangaManagementSystem.DataAccess/Entities/Enums/ProposalStatus.cs`
  - Add `Expired` and ensure values match BR-14.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateSeriesRequest.cs`
  - Either deprecate for proposal creation or correct validation to BR-15 if kept.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateBoardVoteRequest.cs`
  - Enforce reject comment requirement through service logic; keep DTO simple.
- `MangaManagementSystem.Business/DTOs/Responses/Series/SeriesDetailResponse.cs`
  - Include proposal pages, proposal annotations summary if required by FE.
- `MangaManagementSystem.Business/DTOs/Responses/Series/BoardDecisionResponse.cs`
  - Include vote summary, quorum state, deadline state, and finalization details.
- `MangaManagementSystem.Business/Services/Interfaces/Series/ISeriesService.cs`
  - Avoid exposing unsafe status mutation for proposal workflow.
- `MangaManagementSystem.Business/Services/Implements/Series/SeriesService.cs`
  - Enforce or delegate proposal validation, active title uniqueness, and restricted status changes.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IBoardDecisionService.cs`
  - Add decision summary/finalization methods.
- `MangaManagementSystem.Business/Services/Implements/Series/BoardDecisionService.cs`
  - Implement quorum, majority, expiration, irreversible result checks, and status transitions.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IBoardVoteService.cs`
  - Return decision summary after cast vote.
- `MangaManagementSystem.Business/Services/Implements/Series/BoardVoteService.cs`
  - Enforce eligibility, conflict of interest, duplicate vote, reject reason, and deadline.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IEscalationService.cs`
  - Add constrained board-decision handover creation/resolution methods.
- `MangaManagementSystem.Business/Services/Implements/Series/EscalationService.cs`
  - Enforce Tantou-only escalation and Editor-in-Chief resolution rules.
- `MangaManagementSystem.Business/Services/Implements/Chapters/ChapterService.cs`
  - Enforce BR-40, BR-41, BR-42, and BR-46.
- `MangaManagementSystem.WebApi/Controllers/SeriesController.cs`
  - Keep general reads; route creation to proposal workflow or mark old create endpoint as legacy.
- `MangaManagementSystem.WebApi/Controllers/BoardDecisionController.cs`
  - Replace unsafe admin create/update paths with workflow-aware board decision actions.
- `MangaManagementSystem.WebApi/Controllers/EscalationController.cs`
  - Route board decision handover through constrained service methods.
- `MangaManagementSystem.WebApi/Controllers/ChapterController.cs`
  - Ensure chapter creation uses current authenticated user and service-level ownership checks.
- `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`
  - Register new workflow, annotation, storage, file asset, and audit services.
- `MangaManagementSystem.WebApi/Program.cs`
  - Only update if multipart upload limits, request size limits, or authorization policies need adjustment.
- `MangaManagementSystem.Business/Mappers/DependencyInjection.cs`
  - Register new AutoMapper profile if a profile is added.
- `MangaManagementSystem.Business/Mappers/Profiles/BoardProfile.cs`
  - Add or update board decision summary mappings.
- `MangaManagementSystem.Business/Mappers/Profiles/SeriesProfile.cs`
  - Add proposal detail mappings.
- `docs/API_CONTRACT.md`
  - Update endpoints, payloads, statuses, and BR-15 values.
- `MangaManagementSystem.WebApi/docs/AGENTS.md`
  - Update current domain model and feature workflow notes.

### Files Affected Indirectly

- `MangaManagementSystem.DataAccess/Migrations/*`
  - New EF migration generated from schema changes.
- `MangaManagementSystem.DataAccess/Migrations/MangaDbContextModelSnapshot.cs`
  - Updated by EF migration generation.
- `MangaManagementSystem.sln`
  - No expected changes unless test projects are added.
- `MangaManagementSystem.Business/MangaManagementSystem.Business.csproj`
  - Update only if storage implementation requires a new package.
- `MangaManagementSystem.WebApi/MangaManagementSystem.WebApi.csproj`
  - Update only if upload/storage package references are added.
- `MangaManagementSystem.WebApi/appsettings.json`
  - Do not add secrets. Add only non-secret upload/storage configuration if required.

## Implementation Checkpoints

### Checkpoint 1: Normalize Constants and Validation

- [ ] Add shared constants/enums for proposal statuses, active series statuses, board decision statuses, decision results, decision types, assignment types, and allowed publication types.
- [ ] Correct BR-15 validation: title `<= 100`, synopsis `100-2000`, at least one valid genre, valid publication type, and at least 5 sample pages.
- [ ] Enforce BR-17: proposal title cannot match an active series title.
- [ ] Enforce BR-19: a Mangaka can have at most one `Draft` or `UnderReview` proposal.
- [ ] Prevent direct arbitrary status mutation from generic update endpoints.

### Checkpoint 2: Add Proposal Upload Creation

- [ ] Add `IStorageService` and `IFileAssetService`.
- [ ] Add `POST /api/proposals` as `multipart/form-data`.
- [ ] Store optional source zip and required sample-page images.
- [ ] Create `FileAsset` rows for uploads.
- [ ] Create `Series` as proposal draft.
- [ ] Create `ProposalPage` rows with sequential page numbers.
- [ ] Wrap database changes in a transaction.

### Checkpoint 3: Add Proposal Review and Annotations

- [ ] Add `ProposalAnnotation` entity and EF mapping.
- [ ] Add request/response DTOs and service methods for proposal annotations.
- [ ] Add endpoints under `/api/proposal-pages/{proposalPageId}/annotations`.
- [ ] Enforce object-level authorization: only assigned Tantou Editor can create proposal annotations.
- [ ] Allow annotation only while proposal is `UnderReview`.
- [ ] Store FE-provided position fields exactly enough for FE to render annotations back on the page.

### Checkpoint 4: Submit Proposal to Editorial Board

- [ ] Add `POST /api/proposals/{seriesId}/submit-review` for Mangaka to move `Draft -> UnderReview`.
- [ ] Add `POST /api/proposals/{seriesId}/submit-board` for assigned Tantou Editor.
- [ ] Validate proposal completeness again at board submission.
- [ ] Create `BoardDecision` with `DecisionType = "SeriesProposal"`, `Status = "Open"`, and `VotingDeadline = UtcNow + 7 days`.
- [ ] Block duplicate open board decisions for the same proposal.
- [ ] Notify active Editorial Board members.

### Checkpoint 5: Enforce Board Voting Rules

- [ ] Update board vote route to `POST /api/board-decisions/{boardDecisionId}/votes`.
- [ ] Enforce active `EditorialBoard` role at request time.
- [ ] Enforce conflict of interest: voter cannot be series Mangaka, assigned Tantou Editor, assigned Assistant, proposal creator, or decision creator.
- [ ] Block votes after deadline or after decision finalization.
- [ ] Block duplicate votes.
- [ ] Require reject reason/comment with at least 50 characters.
- [ ] Return current vote summary after vote creation.

### Checkpoint 6: Finalize Board Decisions

- [ ] Add a finalization method used after each vote and by manual deadline checks.
- [ ] Require quorum of at least 3 valid votes.
- [ ] Approve when approve votes are greater than 50 percent of valid votes.
- [ ] Reject when reject votes are greater than 50 percent of valid votes and finalization conditions are met.
- [ ] If deadline passes with equal approve/reject votes, set decision to escalation-needed state and notify Editor-in-Chief.
- [ ] If deadline passes without quorum, mark `Expired` or `NoQuorum` and notify Editor-in-Chief.
- [ ] Update proposal status to `Approved`, `Rejected`, or `Expired` only through finalization logic.

### Checkpoint 7: Activate Approved Proposal

- [ ] Add `POST /api/proposals/{seriesId}/activate`.
- [ ] Allow only assigned Tantou Editor.
- [ ] Require proposal status `Approved`.
- [ ] Require finalized approved board decision with quorum.
- [ ] Set series status to `Active`.
- [ ] Block Mangaka self-activation.

### Checkpoint 8: Escalate to Editor-in-Chief

- [ ] Add constrained board decision escalation creation.
- [ ] Allow assigned Tantou Editor to escalate only expired, tied, no-quorum, or deadline-passed decisions with no final result.
- [ ] Store escalation as `Type = "BoardDecisionHandover"` and `EntityType = "BoardDecision"`.
- [ ] Add Editor-in-Chief resolution flow that approves or rejects the proposal and finalizes the decision.
- [ ] Notify Tantou and Mangaka after resolution.

### Checkpoint 9: Enforce Chapter Creation Through BR-46

- [ ] Enforce BR-40: only Mangaka owner can create chapters for active series.
- [ ] Enforce BR-41: chapter number must be unique and monotonically increasing within the series.
- [ ] Enforce BR-42: publication date cannot be in the past; submission deadline is publication date minus 14 days; deadline must be at least 3 days after chapter creation.
- [ ] Enforce BR-43: add overdue chapter notification path for Tantou Editor.
- [ ] Enforce BR-46: chapter can move to submitted only when all required page tasks are approved.

### Checkpoint 10: Add Audit and Concurrency

- [ ] Add immutable audit log for `Series`, `BoardDecision`, `BoardVote`, `Escalation`, and `Chapter`.
- [ ] Capture actor ID, actor role, action, entity type, entity ID, UTC timestamp, old value, new value, from status, and to status.
- [ ] Add optimistic concurrency to `BoardDecision`.
- [ ] Ensure decision finalization and downstream status updates happen transactionally.

### Checkpoint 11: Update Documentation

- [ ] Update `docs/API_CONTRACT.md` with new proposal, annotation, board vote, escalation, activation, and chapter gate endpoints.
- [ ] Update `MangaManagementSystem.WebApi/docs/AGENTS.md` with new domain model notes and workflow responsibilities.
- [ ] Document that `docs/Top50_Business_Rules_Manga.md` is the source of truth for BR-15 conflicts.

## API Shape

### Proposal Creation

`POST /api/proposals`

Role: `Mangaka`

Content type: `multipart/form-data`

Fields:

- `title`: required, max 100 chars.
- `synopsis`: required, 100-2000 chars.
- `publicationType`: required, from configured list.
- `genreIds`: required, at least one valid genre ID.
- `sourceZip`: optional zip file.
- `samplePages`: required image files, at least 5.

Result:

- Creates proposal draft series.
- Creates proposal sample pages.
- Returns proposal detail.

### Proposal Review

`POST /api/proposals/{seriesId}/submit-review`

Role: `Mangaka`

Result:

- Moves proposal from `Draft` to `UnderReview`.

`POST /api/proposal-pages/{proposalPageId}/annotations`

Role: assigned `TantouEditor`

Body:

```json
{
  "positionX": 120.5,
  "positionY": 340.25,
  "content": "Panel composition needs clearer focus."
}
```

Result:

- Stores annotation for FE coordinate rendering.

### Board Submission and Voting

`POST /api/proposals/{seriesId}/submit-board`

Role: assigned `TantouEditor`

Result:

- Creates open board decision with 7 calendar day voting window.

`POST /api/board-decisions/{boardDecisionId}/votes`

Role: `EditorialBoard`

Body:

```json
{
  "voteValue": false,
  "comment": "The proposal needs stronger character motivation before launch."
}
```

Result:

- Records valid vote and returns current board decision summary.

### Escalation

`POST /api/board-decisions/{boardDecisionId}/escalate`

Role: assigned `TantouEditor`

Result:

- Creates handover escalation for Editor-in-Chief when decision expired, tied, or lacks quorum.

`POST /api/escalations/{id}/resolve`

Role: `EditorInChief`

Body:

```json
{
  "resolution": "Approved",
  "comment": "Approved after handover review."
}
```

Result:

- Finalizes escalation and updates proposal/decision status.

### Activation

`POST /api/proposals/{seriesId}/activate`

Role: assigned `TantouEditor`

Result:

- Activates the approved proposal as an active series.

## Test Plan

- Proposal validation rejects title over 100 chars.
- Proposal validation rejects synopsis shorter than 100 chars or longer than 2000 chars.
- Proposal validation rejects invalid genre IDs.
- Proposal validation rejects invalid publication type.
- Proposal validation rejects fewer than 5 sample pages.
- Proposal validation rejects duplicate proposal title when an active series has the same title.
- Proposal validation rejects second draft or under-review proposal from the same Mangaka.
- Assigned Tantou Editor can create proposal annotations.
- Unassigned Tantou Editor cannot create proposal annotations.
- Mangaka cannot create Tantou annotations.
- Board submission fails when proposal is not under review.
- Board submission fails for unassigned Tantou Editor.
- Board submission creates a 7-day voting deadline.
- Editorial Board member can cast one valid vote.
- Duplicate vote is rejected.
- Conflict-of-interest vote is rejected.
- Reject vote with comment shorter than 50 characters is rejected.
- Vote after deadline is rejected.
- Decision with fewer than 3 valid votes does not finalize as approved or rejected.
- Decision with quorum and approve votes greater than 50 percent finalizes as approved.
- Decision with quorum and equal approve/reject votes after deadline enters escalation path.
- Expired no-quorum decision can be escalated by assigned Tantou Editor.
- Editor-in-Chief can resolve escalation.
- Mangaka cannot activate proposal.
- Tantou cannot activate without finalized approved board decision with quorum.
- Assigned Tantou can activate approved proposal.
- Non-owner cannot create chapter.
- Owner cannot create chapter for non-active series.
- Chapter creation rejects invalid publication/deadline dates.
- Chapter submission is blocked until all required page tasks are approved.

## Acceptance Criteria

- Mangaka can create a proposal with uploaded files and at least 5 sample pages.
- Tantou Editor can review the proposal and leave page-positioned annotations from FE coordinate data.
- Tantou Editor can submit a complete under-review proposal to the editorial board.
- Editorial Board voting follows eligibility, conflict, quorum, majority, deadline, and reject-reason rules.
- Expired, tied, or no-quorum decisions can be escalated by Tantou Editor to Editor-in-Chief.
- Editor-in-Chief can resolve escalated decisions.
- Series activation requires a finalized approved board decision with quorum and Tantou review.
- Chapter creation is blocked unless BR-40, BR-41, BR-42, and BR-46 gates are satisfied.
- API docs and agent docs reflect the new workflow.

## Assumptions

- Supabase remains the storage target because `appsettings.json` already contains Supabase configuration and the domain model already uses `FileAsset`.
- Proposal annotations are separate from manuscript annotations to preserve the current manuscript review model.
- `docs/Top50_Business_Rules_Manga.md` overrides older conflicts in `docs/API_CONTRACT.md`.
- Tantou assignment uses `UserAssignment` with `FromUserId = MangakaId`, `ToUserId = TantouEditorId`, and `AssignmentType = "TantouEditor"`.
- Assistant conflict checks use existing assignment data where assistants are assigned to the same Mangaka or series context.
- Existing CRUD endpoints can remain for reads/admin support, but workflow endpoints are the only supported path for proposal state transitions.
