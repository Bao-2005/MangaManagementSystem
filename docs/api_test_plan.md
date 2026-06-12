# API Test Plan — Full Proposal Lifecycle

## Ground Truth: Roles & Assignment Direction

```
UserAssignments:  FromUserId = Tantou Editor  →  ToUserId = Mangaka
```

Accounts you need to seed (via `POST /api/auth/register`):

| Actor | Role | Notes |
|---|---|---|
| Admin | `Admin` | Registers all other accounts |
| Tantou Editor | `TantouEditor` | Must exist before Mangaka is created |
| Mangaka | `Mangaka` | Requires `AssignedFromUserId` = Tantou Editor's ID |
| Editorial Board ×3 | `EditorialBoard` | Need at least 3 for quorum |
| Editor-in-Chief | `EditorInChief` | Needed for extension / special decision path |

---

## Phase 0 — Account Setup (Admin)

> All register calls can be anonymous for now (the `[Authorize]` attribute is commented out on register).

### 0.1 Register Tantou Editor
```http
POST /api/auth/register
{
  "userName": "tantou01",
  "email": "tantou01@test.com",
  "displayName": "Tantou Editor One",
  "password": "Test@123",
  "roleId": "<TantouEditor RoleId>"
}
```
→ Save the returned `id` as `{tantouEditorId}`

### 0.2 Register Mangaka (assigns editor)
```http
POST /api/auth/register
{
  "userName": "mangaka01",
  "email": "mangaka01@test.com",
  "displayName": "Mangaka One",
  "password": "Test@123",
  "roleId": "<Mangaka RoleId>",
  "assignedFromUserId": "{tantouEditorId}"
}
```
→ Save `id` as `{mangakaId}`

### 0.3 Register 3 Editorial Board members
```http
POST /api/auth/register  × 3
{ ..., "roleId": "<EditorialBoard RoleId>" }
```
→ Save IDs as `{board1Id}`, `{board2Id}`, `{board3Id}`

### 0.4 Register Editor-in-Chief
```http
POST /api/auth/register
{ ..., "roleId": "<EditorInChief RoleId>" }
```
→ Save as `{eicId}`

### 0.5 Get Role IDs (to know what to put in roleId above)
```http
GET /api/roles   [AdminOnly]
```

---

## Phase 1 — Login (All Actors)

Each actor logs in and saves their `token` for subsequent requests.

```http
POST /api/auth/login
{ "email": "mangaka01@test.com", "password": "Test@123" }
```
→ `Authorization: Bearer {token}` on all subsequent requests

**Repeat for:** tantou01, board1/2/3, eic

---

## Phase 2 — Upload Sample Pages (Mangaka)

Mangaka must upload at least **5 image files** before proposal can be submitted.

### 2.1 Upload images
```http
POST /api/files
Authorization: Bearer {mangakaToken}
Content-Type: multipart/form-data

category = ProposalPage
files = [page1.png, page2.png, page3.png, page4.png, page5.png]
```
→ Response contains array of `fileAssetId` values. Save as `{pageAssetId1..5}`

---

## Phase 3 — Create Proposal (Mangaka)

### 3.1 Create the series/proposal
```http
POST /api/series
Authorization: Bearer {mangakaToken}
{
  "title": "My Manga Proposal",
  "synopsis": "A detailed synopsis that is between 100 and 2000 characters long...",
  "publicationType": "Weekly",
  "genreIds": ["{genreId}"],
  "samplePageFileAssetIds": [
    "{pageAssetId1}", "{pageAssetId2}", "{pageAssetId3}",
    "{pageAssetId4}", "{pageAssetId5}"
  ]
}
```
→ Status = `Draft`. Save `seriesId` as `{seriesId}`

> **Get Genres:** `GET /api/genres` [Authorized]

### 3.2 (Optional) Add more proposal pages individually
```http
POST /api/series/{seriesId}/proposal-pages
Authorization: Bearer {mangakaToken}
{ "fileAssetId": "{pageAssetId6}" }
```

### 3.3 Verify assignment is visible
```http
GET /api/user-assignments/from-me
Authorization: Bearer {mangakaToken}
```
→ Should return the Tantou Editor assignment row

---

## Phase 4 — Submit for Tantou Review (Mangaka)

```http
POST /api/proposals/{seriesId}/submit-review
Authorization: Bearer {mangakaToken}
```
→ Status = `UnderReview`

---

## Phase 5 — Tantou Editor Review

### 5A — Reject Path (optional)
```http
POST /api/proposals/{seriesId}/reject
Authorization: Bearer {tantouToken}
{ "rejectReason": "Please revise the synopsis to better explain the core conflict." }
```
→ Status = `Rejected`. Mangaka may create a new proposal (repeat Phase 2–4).

### 5B — Submit to Board (happy path)
```http
POST /api/proposals/{seriesId}/submit-to-board
Authorization: Bearer {tantouToken}
```
or
```http
POST /api/proposals/{seriesId}/submit-board
Authorization: Bearer {tantouToken}
```
→ Status = `BoardVoting`. A `BoardDecision` is created with 7-day deadline.
→ All EditorialBoard members receive a notification.
→ Save `boardDecisionId` from response.

### 5C Verify Tantou's assigned Mangaka list
```http
GET /api/user-assignments/to-me
Authorization: Bearer {tantouToken}
```
→ Returns the Mangaka row

---

## Phase 6 — Board Voting (3 × EditorialBoard)

Each board member casts one vote. Need ≥3 valid votes for quorum.

### 6.1 Board Member 1 — Approve
```http
POST /api/board-decisions/{boardDecisionId}/votes
Authorization: Bearer {board1Token}
{ "voteValue": true, "comment": null }
```

### 6.2 Board Member 2 — Approve
```http
POST /api/board-decisions/{boardDecisionId}/votes
Authorization: Bearer {board2Token}
{ "voteValue": true }
```

### 6.3 Board Member 3 — Reject (requires ≥50 char comment)
```http
POST /api/board-decisions/{boardDecisionId}/votes
Authorization: Bearer {board3Token}
{
  "voteValue": false,
  "comment": "The synopsis lacks originality and the genre selection doesn't match the described content."
}
```

→ After the 3rd vote, backend auto-recalculates. If approve > 50%: Status = `Approved`.

### 6.4 Check decision summary
```http
GET /api/board-decisions/{boardDecisionId}
Authorization: Bearer {mangakaToken}
```

### 6.5 Check all votes
```http
GET /api/board-decisions/{boardDecisionId}/votes
```

---

## Phase 7 — Activate Series (Tantou Editor)

Once `Series.Status == Approved`:

```http
POST /api/proposals/{seriesId}/activate
Authorization: Bearer {tantouToken}
```
→ Status = `Active` ✅

---

## Phase 8 — Verify & Check Notifications

### Check notifications (any actor)
```http
GET /api/notifications/my
Authorization: Bearer {board1Token}
```

### Mark as read
```http
PUT /api/notifications/{notificationId}/read
Authorization: Bearer {board1Token}
```

---

## Alternate Path — Tie / No Quorum → EiC Extension → Special Decision

Use this when the board ends in a tie or fewer than 3 votes after deadline.

```
Normal voting ends → Deadline passes → Worker auto-finalizes:
  - < 3 valid votes  → Status = Expired,  Result = NoQuorum  → EiC notified
  - tie              → Status = Tie,       Result = Tie       → EiC notified
```

### EiC: Extend deadline once
```http
POST /api/board-decisions/{boardDecisionId}/extend-deadline
Authorization: Bearer {eicToken}
{
  "newDeadline": "2026-06-20T00:00:00Z",
  "reason": "Extending due to no quorum. Board members have been reminded."
}
```

### (Board votes again during extended period... or not)

### EiC: Special decision after 2nd failed deadline
```http
POST /api/board-decisions/{boardDecisionId}/special-decision
Authorization: Bearer {eicToken}
{
  "decision": "Approved",
  "reason": "Approved by Editor-in-Chief after extended deliberation."
}
```
→ Status = `Approved`. Then Tantou can activate.

---

## Phase 9 — Logout

```http
POST /api/auth/logout
Authorization: Bearer {token}
```

---

## Negative / Validation Tests

| Test | Expected |
|---|---|
| Register Mangaka without `assignedFromUserId` | `400` |
| Register Mangaka with a non-TantouEditor user as `assignedFromUserId` | `400` |
| Create proposal with title > 100 chars | `400` |
| Create proposal with synopsis < 100 chars | `400` |
| Create proposal with < 5 sample pages | `400` |
| Submit-review with status ≠ Draft | `400` |
| Reject/SubmitToBoard as unassigned Tantou Editor | `403` |
| Reject/SubmitToBoard as Mangaka | `403` |
| Cast vote as Mangaka (not EditorialBoard) | `403` |
| Assigned Tantou Editor casts vote on own Mangaka's series | `403` |
| Duplicate vote from same board member | `409/400` |
| Reject vote with comment < 50 chars | `400` |
| Vote after deadline | `400` |
| Activate with status ≠ Approved | `400` |
| Activate as Mangaka (not Tantou) | `403` |
| Extend deadline more than once | `400` |

---

## All Implemented APIs Summary

### Authentication (`/api/auth`)
| Method | Endpoint | Role | Description |
|---|---|---|---|
| POST | `/api/auth/register` | (open) | Create user account |
| POST | `/api/auth/login` | Anonymous | Login, get tokens |
| GET | `/api/auth/me` | Any | Get current user profile |
| POST | `/api/auth/refresh` | Anonymous | Refresh access token |
| POST | `/api/auth/logout` | Any | Invalidate refresh token |
| PUT | `/api/auth/change-password` | Any | Change password |

### Users (`/api/users`)
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/users` | AdminOnly |
| DELETE | `/api/users/{id}/soft-delete` | AdminOnly |

### User Assignments
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/user-assignments/from-me` | MangakaOnly → returns assigned editor |
| GET | `/api/user-assignments/to-me` | TantouEditorOnly → returns assigned Mangaka(s) |
| POST | `/api/user-assignments` | MangakaOnly |
| PUT | `/api/user-assignments/{id}/unassign` | MangakaOnly |
| DELETE | `/api/user-assignments/{id}/soft-delete` | AdminOnly |

### Series (`/api/series`)
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/series` | Any (`?status=Draft`) |
| GET | `/api/series/{id}` | Any |
| GET | `/api/series/mangaka/{mangakaId}` | Any |
| POST | `/api/series` | MangakaOnly |
| PUT | `/api/series/{id}` | MangakaOnly |
| DELETE | `/api/series/{id}/soft-delete` | AdminOnly |

### Proposals (`/api/proposals`)
| Method | Endpoint | Role |
|---|---|---|
| POST | `/api/proposals/{seriesId}/submit-review` | MangakaOnly |
| POST | `/api/proposals/{seriesId}/reject` | TantouEditorOnly |
| POST | `/api/proposals/{seriesId}/submit-to-board` | TantouEditorOnly |
| POST | `/api/proposals/{seriesId}/submit-board` | TantouEditorOnly (alias) |
| POST | `/api/proposals/{seriesId}/activate` | TantouEditorOnly |

### Proposal Pages (`/api/series/{seriesId}/proposal-pages`)
| Method | Endpoint | Role |
|---|---|---|
| GET | `…/proposal-pages` | Any |
| GET | `…/proposal-pages/{id}` | Any |
| POST | `…/proposal-pages` | MangakaOnly |
| DELETE | `…/proposal-pages/{id}/soft-delete` | MangakaOnly |

### Files (`/api/files`)
| Method | Endpoint | Role |
|---|---|---|
| POST | `/api/files` | Any (multipart/form-data, `category` + `files`) |

### Board Decisions
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/series/{seriesId}/board-decisions` | Any |
| GET | `/api/board-decisions/{id}` | Any |
| POST | `/api/board-decisions` | AdminOnly |
| PUT | `/api/board-decisions/{id}` | AdminOnly |
| POST | `/api/board-decisions/{id}/extend-deadline` | EditorInChiefOnly |
| POST | `/api/board-decisions/{id}/special-decision` | EditorInChiefOnly |
| DELETE | `/api/board-decisions/{id}/soft-delete` | AdminOnly |

### Board Votes
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/board-decisions/{boardDecisionId}/votes` | Any |
| POST | `/api/board-decisions/{boardDecisionId}/votes` | EditorialBoardOnly |
| DELETE | `/api/board-votes/{id}/soft-delete` | AdminOnly |

### Notifications
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/notifications/my` | Any |
| PUT | `/api/notifications/{id}/read` | Any |
| POST | `/api/notifications` | AdminOnly |
| DELETE | `/api/notifications/{id}/soft-delete` | AdminOnly |

### Genres (`/api/genres`)
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/genres` | Any |
| POST | `/api/genres` | AdminOnly |
| PUT | `/api/genres/{id}` | AdminOnly |
| DELETE | `/api/genres/{id}/soft-delete` | AdminOnly |

### Roles (`/api/roles`)
| Method | Endpoint | Role |
|---|---|---|
| GET | `/api/roles` | AdminOnly |

---

> [!NOTE]
> **Checkpoint 12 (`activate`) status:** Per the plan, `activate` is marked `[ ]` (not done) but the `SeriesProposalController` endpoint exists and the `ActivateAsync` method is implemented in `SeriesProposalWorkflowService`. It should work — the checkbox may simply be outdated.

> [!WARNING]
> **Automatic deadline finalization** runs in the background every 1–5 minutes. For testing the tie/no-quorum path, either wait for the deadline or temporarily set `VotingDeadline` to a past date via a direct DB edit.
