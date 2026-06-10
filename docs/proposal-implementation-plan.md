# Proposal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the series proposal flow from Mangaka proposal creation through reusable file upload, Tantou review, editorial board voting, automatic deadline finalization, Editor-in-Chief handover, and active series activation.

**Architecture:** Keep the existing layered structure: Controller -> Business service -> Repository/DataAccess -> PostgreSQL. Proposal behavior should live in the Series module because a proposal is the pre-active lifecycle of a `Series`; file upload, board voting, escalation, and reusable notification remain separate supporting services.

**Tech Stack:** .NET 8, ASP.NET Core Web API, EF Core 8, PostgreSQL via Npgsql, AutoMapper, Swagger, existing repository pattern, existing `FileAsset`, `Notification`, and `UserNotification` tables, optional SignalR for real-time delivery.

---

## Business Rule Scope

This plan covers:

- Series proposal: BR-14, BR-15, BR-17, BR-18, BR-19, BR-21, BR-24.
- Editorial board voting: BR-27, BR-28, BR-29, BR-30, BR-31, BR-33, BR-34, BR-35, BR-37.
- Cross-cutting authorization rules: BR-03, BR-04, BR-06, BR-07.
- Chapter creation and page-task rules are out of scope for this plan.

## Proposed New Business Rules

These are not currently in `Top50_Business_Rules_Manga.md`, but they can become official business rules because they define behavior for expired/tied/no-quorum board decisions.

- **BR-New-01: Editor-in-Chief Deadline Extension Rule**
  - When a proposal board decision reaches its first deadline and cannot be finalized because it is tied or has fewer than 3 valid votes, the Editor-in-Chief may extend the voting deadline exactly once.
  - The extension must create a clear record on the `BoardDecision`, such as `ExtensionCount = 1`, `ExtendedBy`, `ExtendedAt`, and `ExtensionReason`.
  - A decision that has already been extended cannot be extended again.

- **BR-New-02: Editor-in-Chief Special Decision Rule**
  - If an extended voting deadline passes and the board decision still cannot be finalized by normal quorum/majority rules, the Editor-in-Chief may make a special final decision of `Approved` or `Rejected`.
  - The special decision must store the deciding user, timestamp, and reason.
  - If rejected, the special decision reason must be written to `Series.RejectReason`.

## Current State

- Existing entities already include `Series`, `ProposalPage`, `BoardDecision`, `BoardVote`, `Escalation`, `FileAsset`, `Notification`, and `UserNotification`.
- Existing services/controllers are mostly CRUD and do not enforce the full proposal/voting workflow.
- Existing proposal creation stores `Series.Status = "Proposed"`, but business rules require proposal lifecycle states.
- Existing `CreateSeriesRequest` conflicts with BR-15: title allows 150 chars and synopsis requires minimum 200 chars. BR-15 requires title `<= 100` and synopsis `100-2000`.
- Existing `Series.RejectReason` should be used for Tantou/editorial rejection feedback. Proposal annotations are not needed for this proposal workflow.
- Existing board vote logic blocks duplicate votes only. It does not enforce eligibility, conflict of interest, quorum, majority, reject reason, expiration, extension, or Editor-in-Chief special decision.
- Existing `NotificationService.BroadcastAsync` creates persisted notifications, but it should be improved for reuse and recipient validation.
- Existing file metadata can be reused through `FileAsset`, but there is no reusable upload service or multipart API yet.

## Status Model

- Use a single `SeriesStatus` enum for both proposal lifecycle and active series lifecycle:
  - `Draft`
  - `UnderReview`
  - `BoardVoting`
  - `Approved`
  - `Rejected`
  - `Expired`
  - `Active`
  - `Cancelled`
- Store `Series.Status` in the database as a string using EF enum conversion.
- Do not add a separate `ProposalStatus` field or enum.

## Future Changes

- Modify current Series CRUD/services/controllers to enforce proposal rules instead of adding a completely separate proposal module.
- Add reusable file upload support that can serve proposal pages, source zip files, manuscript/task submissions, and future upload needs.
- Remove proposal annotation work from the plan.
- Use `Series.RejectReason` for Tantou/editorial rejection feedback.
- Add board voting finalization with quorum, majority, expiration, extension, and special-decision handling.
- Add backend automatic deadline processing.
- Add reusable notification dispatch that can be called by any service.
- Optionally add SignalR for real-time notifications while keeping persisted notifications as source of truth.
- Update API documentation so FE uses the revised workflow endpoints and BR-15 values.

## File Impact Map

### Files to Create

- `MangaManagementSystem.Business/DTOs/Requests/Files/FileUploadRequest.cs`
  - Reusable upload metadata such as bucket/type/category when needed.
- `MangaManagementSystem.Business/DTOs/Responses/Files/FileAssetResponse.cs`
  - Reusable uploaded file metadata response.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateProposalRequest.cs`
  - Proposal creation payload using uploaded `FileAssetId` references.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateProposalWithFilesRequest.cs`
  - Multipart proposal creation payload if proposal creation and upload are combined in one endpoint.
- `MangaManagementSystem.Business/DTOs/Requests/Series/SubmitProposalToBoardRequest.cs`
  - Optional Tantou note when submitting proposal to board.
- `MangaManagementSystem.Business/DTOs/Requests/Series/RejectProposalRequest.cs`
  - Tantou/editorial rejection reason payload.
- `MangaManagementSystem.Business/DTOs/Requests/Series/ExtendBoardDecisionRequest.cs`
  - Editor-in-Chief extension reason and new deadline.
- `MangaManagementSystem.Business/DTOs/Requests/Series/SpecialBoardDecisionRequest.cs`
  - Editor-in-Chief special approve/reject decision payload.
- `MangaManagementSystem.Business/DTOs/Responses/Series/BoardDecisionSummaryResponse.cs`
  - Vote summary: approve count, reject count, valid vote count, quorum flag, current result, deadline state.
- `MangaManagementSystem.Business/DTOs/Responses/NotificationDispatchResponse.cs`
  - Reusable notification result including delivered users, skipped users, and no-recipient state.
- `MangaManagementSystem.Business/Services/Interfaces/Series/ISeriesProposalWorkflowService.cs`
  - Series proposal workflow contract.
- `MangaManagementSystem.Business/Services/Implements/Series/SeriesProposalWorkflowService.cs`
  - Proposal state transitions, validation, Tantou review, board submission, activation.
- `MangaManagementSystem.Business/Services/Interfaces/Files/IFileUploadService.cs`
  - Reusable upload contract that stores file bytes and creates `FileAsset` records.
- `MangaManagementSystem.Business/Services/Implements/Files/FileUploadService.cs`
  - Reusable upload implementation.
- `MangaManagementSystem.Business/Services/Interfaces/Files/IStorageService.cs`
  - Storage-provider abstraction for Supabase/local storage.
- `MangaManagementSystem.Business/Services/Implements/Files/SupabaseStorageService.cs`
  - Supabase-backed storage implementation.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IBoardDecisionFinalizationService.cs`
  - Board decision finalization and deadline processing contract.
- `MangaManagementSystem.Business/Services/Implements/Series/BoardDecisionFinalizationService.cs`
  - Quorum, majority, expiration, extension, and special-decision logic.
- `MangaManagementSystem.Business/Services/Interfaces/INotificationDispatchService.cs`
  - Reusable notification dispatch contract.
- `MangaManagementSystem.Business/Services/Implements/NotificationDispatchService.cs`
  - User/role recipient resolution and persisted notification creation.
- `MangaManagementSystem.WebApi/Controllers/SeriesProposalController.cs`
  - Workflow endpoints for proposal create, review, reject, board submission, activation.
- `MangaManagementSystem.WebApi/Controllers/FileController.cs`
  - Reusable upload endpoint for any workflow that needs file assets.
- `MangaManagementSystem.WebApi/BackgroundServices/BoardDecisionDeadlineWorker.cs`
  - Backend automatic processing for decisions whose deadlines have passed.
- Optional: `MangaManagementSystem.WebApi/Hubs/NotificationHub.cs`
  - SignalR hub for real-time notification delivery.

### Files to Modify

- `MangaManagementSystem.DataAccess/DbContext/MangaDbContext.cs`
  - Map `Series.Status` with `HasConversion<string>()`.
  - Add board decision extension/special-decision fields.
  - Register indexes needed for deadline worker queries.
- `MangaManagementSystem.DataAccess/Entities/Models/Series.cs`
  - Use `SeriesStatus Status` for proposal and active lifecycle.
  - Continue using `RejectReason` for proposal rejection feedback.
- `MangaManagementSystem.DataAccess/Entities/Models/BoardDecision.cs`
  - Add fields such as `ExtensionCount`, `ExtendedBy`, `ExtendedAt`, `ExtensionReason`, `SpecialDecisionBy`, `SpecialDecisionAt`, `SpecialDecisionReason`.
- `MangaManagementSystem.DataAccess/Entities/Models/User.cs`
  - Add navigation properties only if needed for new board decision extension/special-decision fields.
- `MangaManagementSystem.DataAccess/Entities/Models/FileAsset.cs`
  - Add upload category, owner, or storage provider fields only if required for reusable upload tracking.
- `MangaManagementSystem.DataAccess/Entities/Enums/SeriesStatus.cs`
  - Include both proposal lifecycle values and active series lifecycle values.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateSeriesRequest.cs`
  - Correct BR-15 validation or deprecate in favor of `CreateProposalRequest`.
- `MangaManagementSystem.Business/DTOs/Requests/Series/CreateBoardVoteRequest.cs`
  - Keep DTO simple; service enforces reject reason rule.
- `MangaManagementSystem.Business/DTOs/Requests/CreateNotificationRequest.cs`
  - Add optional recipient mode fields only if needed; otherwise keep and wrap through dispatch service.
- `MangaManagementSystem.Business/DTOs/Responses/Series/SeriesDetailResponse.cs`
  - Include `Series.Status`, reject reason, latest board decision summary if useful for FE.
- `MangaManagementSystem.Business/DTOs/Responses/Series/BoardDecisionResponse.cs`
  - Include vote summary, quorum state, deadline state, extension count, and special-decision details.
- `MangaManagementSystem.Business/Services/Interfaces/Series/ISeriesService.cs`
  - Remove or restrict unsafe arbitrary proposal/series status mutation.
- `MangaManagementSystem.Business/Services/Implements/Series/SeriesService.cs`
  - Enforce or delegate proposal validation, title uniqueness, one-active-proposal limit, and safe state transitions.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IBoardDecisionService.cs`
  - Add summary and extension/special-decision methods or delegate to finalization service.
- `MangaManagementSystem.Business/Services/Implements/Series/BoardDecisionService.cs`
  - Keep CRUD/query responsibilities; delegate business finalization to `BoardDecisionFinalizationService`.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IBoardVoteService.cs`
  - Return decision summary after cast vote.
- `MangaManagementSystem.Business/Services/Implements/Series/BoardVoteService.cs`
  - Enforce eligibility, conflict of interest, duplicate vote, reject reason, deadline, and trigger recalculation.
- `MangaManagementSystem.Business/Services/Interfaces/Series/IEscalationService.cs`
  - Add or keep handover support if escalations remain the UI mechanism for chief review.
- `MangaManagementSystem.Business/Services/Implements/Series/EscalationService.cs`
  - Support Editor-in-Chief handover review if using `Escalation` records for tie/expired decisions.
- `MangaManagementSystem.Business/Services/Interfaces/INotificationService.cs`
  - Keep existing user notification reads; add helper methods only if not creating separate dispatch service.
- `MangaManagementSystem.Business/Services/Implements/NotificationService.cs`
  - Keep read/mark/broadcast behavior; use `NotificationDispatchService` for reusable workflow sends.
- `MangaManagementSystem.WebApi/Controllers/SeriesController.cs`
  - Keep general reads and legacy-safe CRUD; route proposal state changes through workflow endpoints.
- `MangaManagementSystem.WebApi/Controllers/BoardDecisionController.cs`
  - Replace unsafe admin create/update paths with workflow-aware vote/finalize/extend/special-decision actions.
- `MangaManagementSystem.WebApi/Controllers/EscalationController.cs`
  - Use only if chief handover is represented as an escalation record.
- `MangaManagementSystem.WebApi/Controllers/NotificationController.cs`
  - Keep user notification retrieval/read endpoints.
- `MangaManagementSystem.WebApi/Extensions/ServiceCollection.cs`
  - Register proposal workflow, reusable file upload/storage, finalization, notification dispatch, and background worker.
- `MangaManagementSystem.WebApi/Program.cs`
  - Register multipart upload limits if needed, storage configuration, and SignalR only if real-time notifications are included.
- `docs/API_CONTRACT.md`
  - Update endpoints, payloads, statuses, and BR-15 values.
- `MangaManagementSystem.WebApi/docs/AGENTS.md`
  - Update current domain model and workflow notes.

### Files Affected Indirectly

- `MangaManagementSystem.DataAccess/Migrations/*`
  - New EF migration if board decision extension/special-decision fields or extra `FileAsset` fields are added; status normalization may use a data-only migration.
- `MangaManagementSystem.DataAccess/Migrations/MangaDbContextModelSnapshot.cs`
  - Updated by EF migration generation.
- `MangaManagementSystem.Business/MangaManagementSystem.Business.csproj`
  - Update only if reusable upload/storage implementation requires a package reference.
- `MangaManagementSystem.WebApi/MangaManagementSystem.WebApi.csproj`
  - Update only if SignalR, upload, or storage package references are needed.
- `MangaManagementSystem.WebApi/appsettings.json`
  - Add only non-secret worker interval or upload configuration if required.

## Implementation Checkpoints

### Checkpoint 1: Normalize Series Status

- [x] Use one `SeriesStatus` enum for proposal and active series lifecycle.
- [x] Store `Series.Status` as a string in the database through EF enum conversion.
- [x] Add or confirm statuses: `Draft`, `UnderReview`, `BoardVoting`, `Approved`, `Rejected`, `Expired`, `Active`, `Cancelled`.
- [x] Keep `Series.RejectReason` as the canonical rejection feedback field.
- [x] Remove proposal annotation work from implementation scope.

### Checkpoint 2: Adapt Series CRUD to Proposal Rules

- [x] Correct BR-15 validation: title `<= 100`, synopsis `100-2000`, at least one valid genre, valid publication type, and at least 5 sample pages.
- [x] Enforce BR-17: proposal title cannot match an active series title.
- [x] Enforce BR-19: a Mangaka can have at most one `Draft`, `UnderReview`, or `BoardVoting` proposal.
- [x] Prevent direct arbitrary status mutation from generic update endpoints.
- [x] Keep reads in existing `SeriesController`; put state transitions behind explicit workflow endpoints.

### Checkpoint 3: Reusable File Upload Service

- [x] Add reusable upload endpoint such as `POST /api/files`.
- [x] Accept `multipart/form-data` with one or more files.
- [x] Validate file type, extension, MIME type, and file size based on requested upload category.
- [x] Upload file bytes to storage through `IStorageService`.
- [x] Create `FileAsset` records with bucket name, object path, original filename, stored filename, extension, file size, and MIME type.
- [x] Return `FileAssetId` values for later use by proposal pages, source zip, task submissions, and future upload workflows.
- [x] If all files fail validation, return a clear business error.
- [x] If some files fail validation, either reject the whole request or return partial failure according to the chosen API policy; prefer rejecting the whole request for proposal uploads.

### Checkpoint 4: Proposal File and Sample Page Handling

- [x] Proposal creation should support uploaded file references from the reusable upload service.
- [x] `CreateProposalRequest` accepts optional `SourceZipFileAssetId` and required `SamplePageFileAssetIds`.
- [x] Validate every referenced file exists and is not deleted.
- [x] Validate source zip file extension/MIME category when provided.
- [x] Validate sample page file assets are images.
- [x] Create `ProposalPage` rows from `SamplePageFileAssetIds`.
- [x] Require at least 5 non-deleted proposal pages before Tantou/board submission.
- [x] Optional future convenience endpoint: `POST /api/proposals/with-files` — deferred as intended (plan marks it optional).

### Checkpoint 5: Tantou Review Without Annotations

- [x] Add `POST /api/proposals/{seriesId}/submit-review` for Mangaka to move `Draft -> UnderReview`.
- [x] Add Tantou reject endpoint that writes to `Series.RejectReason` and sets `Series.Status` to `Rejected`.
- [x] Allow Mangaka to create a new proposal after rejection, following BR-19.
- [x] Add Tantou submit-to-board endpoint for valid under-review proposal.
- [x] Enforce object-level authorization: only assigned Tantou Editor can reject or submit to board.

### Checkpoint 6: Reusable Notification Dispatch

- [x] Add `INotificationDispatchService`.
- [x] Support sending to explicit user IDs.
- [x] Support sending to all active users in a role, especially `EditorialBoard` and `EditorInChief`.
- [x] Validate recipients exist and are active.
- [x] If no recipients are resolved, return a clear `NoRecipients` result or throw a business exception.
- [x] If some requested users do not exist, skip them and include skipped IDs in the response.
- [x] Persist notifications through existing `Notification` and `UserNotification` tables.
- [x] Optionally push real-time events through SignalR after persistence.

### Checkpoint 7: Submit Proposal to Editorial Board

- [x] Add `POST /api/proposals/{seriesId}/submit-board`.
- [x] Validate proposal is `UnderReview`.
- [x] Validate caller is assigned Tantou Editor.
- [x] Validate proposal completeness again.
- [x] Create `BoardDecision` with `DecisionType = "SeriesProposal"`, `Status = "Open"`, and `VotingDeadline = UtcNow + 7 days`.
- [x] Set `Series.Status` to `BoardVoting`.
- [x] Block duplicate open board decisions for the same proposal.
- [x] Use notification dispatch service to notify active Editorial Board members.
- [x] If there are no active board recipients, return a clear business failure and do not silently continue.

### Checkpoint 8: Enforce Board Voting Rules

- [x] Update board vote route to `POST /api/board-decisions/{boardDecisionId}/votes`.
- [x] Enforce active `EditorialBoard` role at request time.
- [x] Enforce conflict of interest: voter cannot be series Mangaka, assigned Tantou Editor, assigned Assistant, proposal creator, or decision creator.
- [x] Block votes after deadline or after decision finalization.
- [x] Block duplicate votes.
- [x] Require reject reason/comment with at least 50 characters.
- [x] Return current vote summary after vote creation.
- [x] Trigger finalization recalculation after each vote.

### Checkpoint 9: Board Decision Finalization Logic

- [x] Add `BoardDecisionFinalizationService`.
- [x] Count only valid, non-deleted, conflict-free votes.
- [x] Require quorum of at least 3 valid votes.
- [x] Approve when approve votes are greater than 50 percent of valid votes.
- [x] Reject when reject votes are greater than 50 percent of valid votes.
- [x] Write rejection reason to `Series.RejectReason`; choose an aggregated reject summary or require Tantou/chief final reason.
- [x] If deadline passes with fewer than 3 valid votes, set `Series.Status` to `Expired` and decision result `NoQuorum`.
- [x] If deadline passes with equal approve/reject votes, mark decision as `Tie` and notify Editor-in-Chief.
- [x] Do not let FE decide finalization. FE only displays state and calls allowed actions.

### Checkpoint 10: Automatic Deadline Processing

- [x] Add `BoardDecisionDeadlineWorker` as ASP.NET Core `BackgroundService`.
- [x] Worker runs every 1-5 minutes.
- [x] Worker queries open board decisions where `VotingDeadline <= UtcNow`.
- [x] Worker calls `BoardDecisionFinalizationService`.
- [x] Worker must be idempotent: processing the same decision twice must not duplicate finalization or notifications.
- [x] For this project, use `BackgroundService`; future production option can be Hangfire, Quartz.NET, or database scheduler.

### Checkpoint 11: Editor-in-Chief Extension and Special Decision

- [x] Notify Editor-in-Chief when first deadline ends in tie or no quorum.
- [x] Add `POST /api/board-decisions/{id}/extend-deadline`.
- [x] Allow only `EditorInChief`.
- [x] Allow extension only once.
- [x] Require extension reason and new future deadline.
- [x] After extended deadline passes, worker finalizes normally if possible.
- [x] If still tied or no quorum after the extension, allow `POST /api/board-decisions/{id}/special-decision`.
- [x] Special decision must be `Approved` or `Rejected` and include reason.
- [x] If rejected, write reason to `Series.RejectReason`.
- [x] If approved, set `Series.Status` to `Approved`.

### Checkpoint 12: Activate Approved Proposal

- [ ] Add `POST /api/proposals/{seriesId}/activate`.
- [ ] Allow only assigned Tantou Editor.
- [ ] Require `Series.Status` to be `Approved`.
- [ ] Require finalized approved board decision with quorum or valid Editor-in-Chief special approval.
- [ ] Set series status to `Active`.
- [ ] Block Mangaka self-activation.

### Checkpoint 13: Update Documentation

- [x] Update `docs/API_CONTRACT.md` with revised proposal, rejection, board vote, notification, extension, special-decision, and activation endpoints.
- [x] Update `MangaManagementSystem.WebApi/docs/AGENTS.md` with new workflow responsibilities.
- [x] Add BR-New-01 and BR-New-02 to project business-rule documentation if approved by the team.


## Finalize Board Voting Behavior

Finalization is backend-owned.

The FE must not determine whether a proposal is approved, rejected, expired, or tied. FE can display the latest state and call allowed actions, but the backend owns:

- Deadline comparison.
- Valid vote count.
- Conflict-of-interest exclusion.
- Quorum calculation.
- Majority calculation.
- `Series.Status` update.
- Notification dispatch.

### Normal Finalization

When a vote is cast or a deadline worker processes an expired deadline:

- Count valid votes.
- If valid votes are at least 3:
  - If approve votes are greater than 50 percent of valid votes, set board result `Approved` and `Series.Status` to `Approved`.
  - If reject votes are greater than 50 percent of valid votes, set board result `Rejected`, set `Series.Status` to `Rejected`, and set `Series.RejectReason`.
  - If approve votes equal reject votes after deadline, set board result `Tie` and notify Editor-in-Chief.
- If valid votes are fewer than 3 after deadline:
  - Set board result `NoQuorum`.
  - Set `Series.Status` to `Expired`.
  - Notify Editor-in-Chief.

### Extension and Special Decision

- First failed deadline because of tie/no quorum:
  - Editor-in-Chief may extend deadline once.
- Second failed deadline after extension:
  - Editor-in-Chief may issue special `Approved` or `Rejected` decision.

## Reusable Notification Behavior

Notification sending should be a reusable service, not embedded in board voting only.

Expected behavior:

- Resolve target users by role or explicit user IDs.
- Validate users exist and are not deleted/deactivated.
- If no recipients exist, report that clearly to the caller.
- If some recipients are invalid, skip them and report skipped IDs.
- Persist notification rows before any real-time push.
- Return delivery summary.

SignalR can be added for real-time notifications:

- Database tables remain source of truth.
- SignalR pushes notification events to connected clients.
- Offline users still receive persisted notifications when they later call notification APIs.

## Test Plan

- Proposal validation rejects title over 100 chars.
- Proposal validation rejects synopsis shorter than 100 chars or longer than 2000 chars.
- Proposal validation rejects invalid genre IDs.
- Proposal validation rejects invalid publication type.
- Reusable file upload rejects invalid file type, invalid MIME type, and oversized files.
- Reusable file upload creates `FileAsset` rows for valid files.
- Proposal creation rejects missing sample page file references.
- Proposal creation rejects sample page file references that are not images.
- Proposal creation rejects deleted or missing `FileAssetId` references.
- Proposal validation rejects fewer than 5 sample pages.
- Proposal validation rejects duplicate proposal title when an active series has the same title.
- Proposal validation rejects second draft, under-review, or board-voting proposal from the same Mangaka.
- Assigned Tantou Editor can reject proposal and write `Series.RejectReason`.
- Unassigned Tantou Editor cannot reject or submit proposal to board.
- Mangaka can create new proposal after rejection.
- Board submission fails when proposal is not under review.
- Board submission creates a 7-day voting deadline.
- Board notification fails clearly if no active Editorial Board users exist.
- Editorial Board member can cast one valid vote.
- Duplicate vote is rejected.
- Conflict-of-interest vote is rejected.
- Reject vote with comment shorter than 50 characters is rejected.
- Vote after deadline is rejected.
- Decision with fewer than 3 valid votes expires after deadline.
- Decision with quorum and approve votes greater than 50 percent finalizes as approved.
- Decision with quorum and reject votes greater than 50 percent finalizes as rejected and writes reject reason.
- Decision with equal approve/reject votes after deadline notifies Editor-in-Chief.
- Editor-in-Chief can extend deadline once.
- Editor-in-Chief cannot extend deadline twice.
- After extended deadline, Editor-in-Chief can issue special approve/reject decision if still tied or no quorum.
- Mangaka cannot activate proposal.
- Tantou cannot activate without approved proposal.
- Assigned Tantou can activate approved proposal.

## Acceptance Criteria

- Mangaka can create and submit a proposal with enough sample pages.
- File upload is reusable and returns `FileAssetId` values for proposal and future workflows.
- Tantou Editor reviews by approving board submission or rejecting with `Series.RejectReason`.
- Editorial Board voting follows eligibility, conflict, quorum, majority, deadline, and reject-reason rules.
- Backend automatically processes expired voting deadlines.
- No-quorum decisions become expired after deadline.
- Tied decisions notify Editor-in-Chief.
- Editor-in-Chief can extend once, then make a special final decision after the second failed deadline.
- Notifications are sent through reusable dispatch logic and report no-recipient/missing-recipient cases.
- Optional SignalR real-time notification can be added without replacing persisted notification storage.
- Series activation requires approved proposal and Tantou activation action.

## Assumptions

- `Series.RejectReason` is the canonical proposal rejection feedback field.
- Proposal annotations are intentionally out of scope.
- `docs/Top50_Business_Rules_Manga.md` overrides older conflicts in `docs/API_CONTRACT.md`.
- Tantou assignment uses `UserAssignment` with `FromUserId = MangakaId`, `ToUserId = TantouEditorId`, and `AssignmentType = "TantouEditor"`.
- Existing `Notification` and `UserNotification` tables are enough for persisted notifications.
- Existing `FileAsset` table is the reusable metadata table for uploaded files.
- Audit log is removed from this proposal implementation plan.
- SignalR is optional and can be implemented after persisted notification dispatch is reliable.
- Chapter creation is intentionally excluded from this revision and should be planned separately.
