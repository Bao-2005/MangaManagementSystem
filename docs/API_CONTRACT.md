# MangaHub — API Contract

> **Version:** 1.0  
> **Base URL:** `http://localhost:3000/api` (hoặc cấu hình qua `NEXT_PUBLIC_API_URL`)  
> **Auth:** Bearer token — header `Authorization: Bearer <token>`  
> Tất cả endpoints (trừ `/auth/login` và `/auth/logout`) đều yêu cầu Authentication.

---

## Mục lục

1. [Enum & Type Values](#1-enum--type-values)
2. [Data Models](#2-data-models)
3. [Authentication](#3-authentication)
4. [Series & Proposals](#4-series--proposals)
5. [Chapters](#5-chapters)
6. [Manuscripts](#6-manuscripts)
7. [Page Tasks](#7-page-tasks)
8. [Reviews](#8-reviews)
9. [Rankings & Voting](#9-rankings--voting)
10. [Notifications](#10-notifications)
11. [Error Format](#11-error-format)
12. [Tóm tắt Endpoints](#12-tóm-tắt-endpoints)
13. [Ghi chú & Quy tắc nghiệp vụ](#13-ghi-chú--quy-tắc-nghiệp-vụ)

---

## 1. Enum & Type Values

### `UserRole`

| Value | Label | Mô tả |
|---|---|---|
| `Mangaka` | Tác giả | Tạo đề xuất series, giao việc cho Assistant, duyệt trang vẽ. |
| `Assistant` | Trợ lý | Nhận task vẽ trang, vẽ và nộp lại cho Mangaka duyệt. |
| `Tantou Editor` | BTV phụ trách | Xem và duyệt bản thảo (Manuscript), viết feedback/annotation. |
| `Editorial Board` | Ban biên tập | Bỏ phiếu đề xuất mới|
| `Editor-in-Chief` | Tổng biên tập | Vai trò quản trị cao nhất, đưa ra quyết định cuối cùng. |

### `SeriesStatus`

| Value | Ý nghĩa |
|---|---|
| `Draft` | Bản đề xuất đang soạn, chưa nộp cho Tantou Editor |
| `UnderReview` | Đã nộp, đang chờ Tantou Editor xem xét |
| `BoardVoting` | Tantou Editor đã chuyển lên, đang trong giai đoạn Editorial Board bỏ phiếu |
| `Approved` | Ban biên tập đã thông qua (hoặc TBT ra quyết định đặc biệt) — chờ Tantou kích hoạt |
| `Rejected` | Đề xuất bị Tantou Editor hoặc Editorial Board bác bỏ |
| `Expired` | Hết thời gian bỏ phiếu mà không đủ quorum |
| `Active` | Series đã được Tantou Editor kích hoạt, đang trong tiến trình sáng tác |
| `Cancelled` | Series đã bị hủy xuất bản theo quyết định của Board |

### `ChapterStatus`

| Value | Ý nghĩa |
|---|---|
| `Draft` | Chương nháp, đang chuẩn bị cốt truyện/storyboard |
| `In Progress` | Đang trong quá trình vẽ phác thảo/giao việc cho trợ lý |
| `Ready for Editor` | Hoàn thành vẽ thô, đã nộp cho Tantou Editor phê duyệt |
| `Published` | Biên tập viên đã thông qua bản thảo và tiến hành xuất bản |

### `TaskStatus`

| Value | Ý nghĩa |
|---|---|
| `Unassigned` | Trang vẽ chưa phân công cho trợ lý nào |
| `Pending` | Đang chờ trợ lý đồng ý nhận task vẽ |
| `In-Progress` | Trợ lý đang thực hiện vẽ trang |
| `Submitted` | Đã hoàn thành vẽ, nộp lại cho Mangaka review |
| `Approved` | Mangaka đồng ý và duyệt bản vẽ trang này |
| `Rejected` | Bản vẽ bị lỗi, Mangaka từ chối và bắt buộc sửa đổi |
| `Suspended` | Task vẽ bị tạm ngưng |

### `ManuscriptStatus`

| Value | Ý nghĩa |
|---|---|
| `SUBMITTED` | Bản thảo chương đã được Mangaka nộp, chờ BTV xem xét |
| `APPROVED` | Bản thảo được duyệt hoàn thành để xuất bản |
| `REVISION REQUIRED` | BTV yêu cầu chỉnh sửa/vẽ lại các chi tiết |

---

## 2. Data Models

```typescript
interface User {
  id: string
  name: string
  email: string
  role: 'Mangaka' | 'Assistant' | 'Tantou Editor' | 'Editorial Board' | 'Editor-in-Chief'
  avatarUrl: string
}

interface SeriesProposal {
  id: string
  title: string
  author: string
  genre: string[]
  type: 'Weekly' | 'Monthly' | 'One-shot'
  status: 'Active' | 'Proposed' | 'Deferred' | 'Rejected'
  description: string
  coverColor: string
  rating: number
}

interface Chapter {
  id: string
  seriesId: string
  number: number
  title: string
  status: 'Draft' | 'In Progress' | 'Ready for Editor' | 'Published'
  totalPages: number
  publicationDate: string // YYYY-MM-DD
  deadline: string        // YYYY-MM-DD (Tính tự động: publicationDate - 14 ngày)
  createdAt: string       // ISO 8601
  synopsis?: string
  notes?: string
  storyboardFiles?: any[]
  manuscriptFiles?: any[]
}

interface ManuscriptItem {
  id: string
  seriesId: string
  seriesTitle: string
  chapterNumber: number
  chapterTitle: string
  latestVersion: string  // v1, v2, v3...
  status: 'SUBMITTED' | 'APPROVED' | 'REVISION REQUIRED'
  progress: number       // 0 - 100%
}

interface TaskItem {
  id: string
  chapterId: string
  type: string           // e.g. "Line Art", "Coloring", "Background Art", "Screentoning"
  pages: string          // e.g. "1-3", "4-8"
  description: string
  assistantId: string    // "Unassigned" nếu chưa giao
  assistantName: string
  status: 'Unassigned' | 'Pending' | 'In-Progress' | 'Submitted' | 'Approved' | 'Rejected'
  submittedWorkUrl?: string
  feedback?: string      // Nhận xét từ Mangaka khi phê duyệt/từ chối
  assignedAt?: string
  updatedAt?: string
  dueDate?: string
  pageStart?: number
  pageEnd?: number
  attachments?: { name: string; size: string; type: string }[]
  submittedFiles?: { name: string; size: string; type: string }[]
  submitDescription?: string
}

interface ProposalReview {
  id: string
  seriesTitle: string
  mangakaName: string
  status: 'PENDING' | 'APPROVED' | 'REJECTED'
  submittedAt: string     // ISO 8601
}

interface RankingItem {
  id: string
  seriesTitle: string
  genre: string
  votes: number
  readers: number
  score: number           // Tỷ lệ bình chọn (votes / readers) * 100
  status: string          // Ví dụ: "TOP 3", ""
  rank: number
}

interface Notification {
  id: string
  title: string
  message: string
  read: boolean
  createdAt: string       // ISO 8601
}

interface VoteRecord {
  id: string
  seriesId: string
  votedAt: string         // ISO 8601
}
```

---

## 3. Authentication

### `POST /auth/login`

Đăng nhập vào hệ thống.

**Request body**

```json
{
  "email": "obata@mangaflow.com",
  "password": "password123"
}
```

**Response `200`**

```json
{
  "success": true,
  "token": "mock_token_jwt_xyz123",
  "user": {
    "id": "U01",
    "name": "Takeshi Obata",
    "email": "obata@mangaflow.com",
    "role": "Tantou Editor",
    "avatarUrl": "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100"
  }
}
```

---

### `POST /auth/logout`

Đăng xuất và hủy token hiện tại.

**Response `200`**

```json
{
  "success": true
}
```

---

### `GET /auth/me`

Lấy thông tin người dùng đang đăng nhập dựa trên token.

**Response `200`**

```json
{
  "id": "U01",
  "name": "Takeshi Obata",
  "email": "obata@mangaflow.com",
  "role": "Tantou Editor",
  "avatarUrl": "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100"
}
```

---

## 4. Series & Proposals

> **Base URL for proposal workflow:** `http://localhost:5151/api`

### `GET /api/series`

Lấy danh sách tất cả series (bao gồm đề xuất ở mọi trạng thái, tùy quyền hạn).

**Response `200`** — array of `SeriesResponse`

---

### `GET /api/series/{id}`

Lấy chi tiết một series (bao gồm `status`, `rejectReason`, thông tin board decision mới nhất).

---

### `POST /api/series` *(Mangaka — tạo bản nháp proposal)*

Tạo bản đề xuất series ở trạng thái `Draft`.

**Validation (BR-15):**
- `title` ≤ 100 ký tự, bắt buộc, không trùng với title series đang `Active` (BR-17).
- `synopsis` từ 100 đến 2000 ký tự.
- `publicationType` phải là `Weekly`, `Monthly`, hoặc `One-shot`.
- Ít nhất 1 genre hợp lệ.
- Mangaka không được có hơn 1 proposal ở `Draft`, `UnderReview`, hoặc `BoardVoting` (BR-19).

**Request body**

```json
{
  "title": "Sakura Knights",
  "synopsis": "In feudal Japan reimagined with magitech armor, five orphaned warriors...",
  "publicationType": "Weekly",
  "genreIds": ["<genre-uuid>"],
  "coverImageUrl": "https://..."
}
```

**Response `201`** — `SeriesResponse` với `status: "Draft"`

---

### `POST /api/proposals/{seriesId}/submit-review` *(Mangaka)*

Nộp bản nháp lên cho Tantou Editor xem xét. Chuyển `Draft → UnderReview`.

**Điều kiện:** Phải có ít nhất 5 `ProposalPage` chưa bị xóa.

**Response `200`** — `SeriesDetailResponse` với `status: "UnderReview"`

---

### `POST /api/proposals/{seriesId}/reject` *(TantouEditorOnly)*

Tantou Editor từ chối proposal đang ở trạng thái `UnderReview`. Chuyển `UnderReview → Rejected`.

**Object-level auth:** Chỉ Tantou Editor được gán cho Mangaka đó qua `UserAssignment`.

**Request body**

```json
{ "rejectReason": "Nội dung chưa phù hợp vì..." }
```

**Response `200`** — `SeriesDetailResponse` với `status: "Rejected"`, `rejectReason` được ghi vào `Series.RejectReason`.

---

### `POST /api/proposals/{seriesId}/submit-to-board` *(TantouEditorOnly)*

Tantou Editor chuyển proposal lên Editorial Board bỏ phiếu. Chuyển `UnderReview → BoardVoting`.

**Điều kiện:**
- Proposal phải ở `UnderReview`.
- Phải có ít nhất 5 sample pages.
- Không được có open `BoardDecision` trùng.
- Tạo `BoardDecision` với `DecisionType = "SeriesProposal"`, `Status = "Open"`, deadline = UtcNow + 7 ngày.
- Gửi notification đến tất cả `EditorialBoard` active. Nếu không có recipient nào → trả lỗi.

**Response `200`** — `BoardDecisionResponse`

---

### `GET /api/series/{seriesId}/board-decisions` *(Authenticated)*

Lấy danh sách `BoardDecision` của một series.

---

### `GET /api/board-decisions/{id}` *(Authenticated)*

Lấy chi tiết một `BoardDecision` (bao gồm vote summary, extension info, special decision info).

---

### `POST /api/board-decisions/{boardDecisionId}/votes` *(EditorialBoardOnly)*

Bỏ phiếu cho một `BoardDecision` đang mở.

**Quy tắc bỏ phiếu (BR-27, BR-28, BR-29, BR-30, BR-33, BR-35):**
- Chỉ `EditorialBoard` active mới được bỏ phiếu.
- Xung đột lợi ích: không được bỏ phiếu nếu là Mangaka, Tantou Editor được gán, Assistant được gán, người tạo proposal/decision.
- Không được bỏ phiếu sau deadline hoặc sau khi decision đã finalized.
- Không được bỏ phiếu trùng.
- Vote từ chối (`voteValue: false`) bắt buộc phải có `comment` tối thiểu 50 ký tự.

**Request body**

```json
{
  "voteValue": true,
  "comment": "Cốt truyện hấp dẫn, nét vẽ phù hợp..."
}
```

**Response `200`** — `BoardDecisionSummaryResponse` (approve count, reject count, quorum state, current result)

---

### `POST /api/board-decisions/{id}/extend-deadline` *(EditorInChiefOnly)*

Tổng biên tập gia hạn deadline bỏ phiếu sau khi decision kết thúc bằng `Tie` hoặc `NoQuorum`.

**Điều kiện:**
- `ExtensionCount` phải là 0 (chỉ được gia hạn 1 lần — BR-New-01).
- Decision phải ở trạng thái `Tie` hoặc `NoQuorum`.
- `newDeadline` phải ở tương lai.

**Request body**

```json
{
  "newDeadline": "2026-07-15T00:00:00Z",
  "reason": "Cần thêm thời gian để các thành viên bỏ phiếu đầy đủ."
}
```

**Response `200`** — `BoardDecisionResponse` với deadline mới, `status: "Open"`

---

### `POST /api/board-decisions/{id}/special-decision` *(EditorInChiefOnly)*

Tổng biên tập đưa ra quyết định đặc biệt sau khi deadline gia hạn vẫn không đủ điều kiện finalize (BR-New-02).

**Điều kiện:**
- `ExtensionCount >= 1`.
- Decision chưa có `SpecialDecisionAt`.
- Decision phải ở trạng thái `Tie` hoặc `NoQuorum` sau khi deadline gia hạn đã qua.

**Request body**

```json
{
  "decision": "Approved",
  "reason": "Sau khi xem xét toàn diện, đề xuất đủ điều kiện phát hành."
}
```

**Response `200`** — `BoardDecisionResponse`. Nếu `decision = "Rejected"`, `Series.RejectReason` được cập nhật.

---

### `POST /api/proposals/{seriesId}/activate` *(TantouEditorOnly)*

Tantou Editor kích hoạt series đã được phê duyệt. Chuyển `Approved → Active` (BR-24).

**Điều kiện:**
- Series phải ở `Approved`.
- Phải có `BoardDecision` finalized với `Result = "Approved"`.
- Chỉ Tantou Editor được gán (object-level auth). Mangaka không thể tự kích hoạt.

**Response `200`** — `SeriesDetailResponse` với `status: "Active"`

---

## 5. Chapters

### `GET /chapters`

Lấy danh sách tất cả các chương truyện trong toàn hệ thống.

**Response `200`**

```json
[
  {
    "id": "CH01",
    "seriesId": "S01",
    "number": 1,
    "title": "The Resonance of Blades",
    "status": "Published",
    "totalPages": 19,
    "publicationDate": "2026-05-15",
    "deadline": "2026-05-01",
    "createdAt": "2026-04-20T10:00:00Z"
  }
]
```

---

### `GET /chapters/series/:seriesId`

Lấy danh sách các chương thuộc một series cụ thể.

**Response `200`**

```json
[
  {
    "id": "CH01",
    "seriesId": "S01",
    "number": 1,
    "title": "The Resonance of Blades",
    "status": "Published",
    "totalPages": 19,
    "publicationDate": "2026-05-15",
    "deadline": "2026-05-01",
    "createdAt": "2026-04-20T10:00:00Z"
  }
]
```

---

### `POST /chapters`

Tạo một chương truyện mới (chỉ dùng cho Mangaka).

**Request body**

```json
{
  "seriesId": "S01",
  "number": 2,
  "title": "Cherry Blossom Magitech",
  "status": "Draft",
  "totalPages": 18,
  "publicationDate": "2026-06-15",
  "synopsis": "Tóm tắt cốt truyện chương 2...",
  "notes": "Ghi chú gửi biên tập viên...",
  "storyboardFiles": [],
  "manuscriptFiles": [
    { "name": "Ch02_Draft_p1-18.zip", "size": "15 MB", "type": "zip" }
  ]
}
```

> **Cách tính hạn chót (BR-03 & BR-42):**
> - Ngày hạn chót `deadline` nộp bản thảo tự động tính bằng: `publicationDate` trừ đi **14 ngày**.
> - Để đảm bảo thời gian vẽ tối thiểu 3 ngày cho studio, ngày `publicationDate` chọn phải cách ngày hiện tại ít nhất **17 ngày** (14 ngày deadline + 3 ngày sản xuất tối thiểu).

**Response `201`**

```json
{
  "success": true,
  "data": {
    "id": "CH02",
    "seriesId": "S01",
    "number": 2,
    "title": "Cherry Blossom Magitech",
    "status": "Draft",
    "totalPages": 18,
    "publicationDate": "2026-06-15",
    "deadline": "2026-06-01",
    "createdAt": "2026-05-15T09:00:00Z"
  }
}
```

---

### `PUT /chapters/:id`

Cập nhật thông tin hoặc trạng thái một chương cụ thể.

**Request body**

```json
{
  "status": "In Progress"
}
```

**Response `200`**

```json
{
  "success": true,
  "data": {
    "id": "CH02",
    "status": "In Progress"
  }
}
```

---

## 6. Manuscripts

### `GET /manuscripts`

Lấy danh sách tất cả các bản thảo.

**Response `200`**

```json
[
  {
    "id": "M01",
    "seriesId": "S01",
    "seriesTitle": "Demon Slayer: Chronicles",
    "chapterNumber": 15,
    "chapterTitle": "Iron Will",
    "latestVersion": "v1",
    "status": "APPROVED",
    "progress": 100
  }
]
```

---

### `GET /manuscripts/:id`

Lấy thông tin chi tiết một bản thảo theo `id`.

**Response `200`**

```json
{
  "id": "M01",
  "seriesId": "S01",
  "seriesTitle": "Demon Slayer: Chronicles",
  "chapterNumber": 15,
  "chapterTitle": "Iron Will",
  "latestVersion": "v1",
  "status": "APPROVED",
  "progress": 100
}
```

---

### `POST /manuscripts`

Mangaka nộp bản thảo thô hoàn thiện của chương để BTV xem xét duyệt xuất bản.

**Request body**

```json
{
  "seriesId": "S01",
  "chapterId": "CH02",
  "fileUrl": "https://api.mangahub.vn/manuscripts/ch02_v1.zip",
  "notes": "Đã hoàn thành nét vẽ và screentoning."
}
```

> **Điều kiện nộp bản thảo (BR-04):**
> - Chỉ được nộp bản thảo chương lên hệ thống khi **100% các page task của chương đó đã được Mangaka duyệt (Approved)**. 

**Response `201`**

```json
{
  "success": true,
  "data": {
    "id": "M04",
    "seriesId": "S01",
    "chapterId": "CH02",
    "version": "v1",
    "status": "SUBMITTED",
    "submittedAt": "2026-06-01T08:30:00Z"
  }
}
```

---

## 7. Page Tasks

### `GET /tasks`

Lấy danh sách các trang vẽ được phân công trong chương.

**Response `200`**

```json
[
  {
    "id": "T01",
    "chapterId": "CH02",
    "type": "Line Art",
    "pages": "1-3",
    "description": "Sketch and ink the opening battle",
    "assistantName": "Sato Takashi",
    "status": "Approved"
  }
]
```

---

### `POST /tasks/assign`

Phân công task vẽ trang cụ thể cho một trợ lý (chỉ dùng cho Mangaka).

**Request body**

```json
{
  "chapterId": "CH02",
  "pageStart": 4,
  "pageEnd": 8,
  "assignedToId": "A02",
  "deadline": "2026-05-30T17:00:00Z",
  "type": "Coloring",
  "description": "Apply sunset glow colors"
}
```

**Response `201`**

```json
{
  "success": true,
  "data": {
    "id": "T03",
    "chapterId": "CH02",
    "type": "Coloring",
    "pages": "4-8",
    "description": "Apply sunset glow colors",
    "assistantId": "A02",
    "assistantName": "Suzuki Mei",
    "status": "Pending"
  }
}
```

---

## 8. Reviews

### `GET /reviews`

Lấy danh sách đề cử series đang chờ phê duyệt.

**Response `200`**

```json
[
  {
    "id": "R01",
    "seriesTitle": "Jujutsu Kaisen: Culling Game",
    "mangakaName": "Gege Akutami",
    "status": "PENDING",
    "submittedAt": "2026-05-28T09:00:00Z"
  }
]
```

---

### `PUT /reviews/:id/decision`

Biên tập viên/Hội đồng đưa ra quyết định phê duyệt đề xuất.

**Request body**

```json
{
  "decision": "APPROVED",
  "feedback": "Cốt truyện hấp dẫn, nét vẽ cá tính phù hợp với thị hiếu độc giả hiện nay."
}
```

**Response `200`**

```json
{
  "success": true,
  "id": "R01",
  "decision": "APPROVED",
  "feedback": "Cốt truyện hấp dẫn..."
}
```

---

## 9. Rankings & Voting

### `GET /ranking`

Lấy danh sách xếp hạng các tác phẩm trong chu kỳ hiện tại.

**Response `200`**

```json
[
  {
    "id": "S01",
    "seriesTitle": "Demon Slayer: Chronicles",
    "genre": "Action, Fantasy",
    "votes": 12000,
    "readers": 15000,
    "score": 80.0,
    "status": "TOP 3",
    "rank": 1
  }
]
```

> **Công thức tính điểm xếp hạng (BR-02):**
> - $\text{Score} = \left(\frac{\text{votes}}{\text{readers}}\right) \times 100\%$.
> - Nhóm **20% có điểm thấp nhất** trong danh sách sẽ bị gắn cờ cảnh báo xem xét hủy xuất bản.

---

### `POST /ranking/confirm`

Xác nhận danh sách xếp hạng của quý hiện tại (chỉ Editorial Board).

**Request body**

```json
{
  "quarter": "2026-Q2"
}
```

**Response `200`**

```json
{
  "success": true,
  "quarter": "2026-Q2"
}
```

---

### `POST /votes/submit`

Độc giả thực hiện bình chọn cho tác phẩm.

**Request body**

```json
{
  "seriesId": "S01"
}
```

**Response `200`**

```json
{
  "success": true,
  "seriesId": "S01"
}
```

---

## 10. Notifications

### `GET /notifications`

Lấy toàn bộ thông báo của người dùng hiện tại.

**Response `200`**

```json
[
  {
    "id": "N01",
    "title": "New Manuscript Submitted",
    "message": "Chainsaw Man Chapter 9 has been submitted for review.",
    "read": false,
    "createdAt": "2026-06-01T08:00:00Z"
  }
]
```

---

### `PUT /notifications/:id/read`

Đánh dấu thông báo đã được xem.

**Response `200`**

```json
{
  "success": true
}
```

---

## 11. Error Format

Tất cả các lỗi trả về từ API đều tuân thủ cấu trúc lỗi tiêu chuẩn:

```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "The requested entity was not found.",
  "statusCode": 404
}
```

| HTTP Status Code | Code | Mô tả |
|---|---|---|
| `400` | `BAD_REQUEST` | Dữ liệu không hợp lệ hoặc vi phạm quy tắc validation (như `voteCount > readerCount`). |
| `401` | `UNAUTHORIZED` | Token bị thiếu hoặc không chính xác. |
| `403` | `FORBIDDEN` | Tài khoản hiện tại không có vai trò phù hợp để truy cập tài nguyên. |
| `404` | `RESOURCE_NOT_FOUND` | Không tìm thấy thực thể yêu cầu. |
| `500` | `INTERNAL_ERROR` | Lỗi phát sinh ngoài ý muốn trên hệ thống server. |

---

## 12. Tóm tắt Endpoints

### Authentication

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/api/auth/login` | POST | All | Đăng nhập hệ thống | 🔴 High |
| `/api/auth/logout` | POST | All | Đăng xuất hệ thống | 🔴 High |
| `/api/auth/me` | GET | All | Lấy thông tin user hiện tại | 🔴 High |

### Series & Proposal Workflow

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/api/series` | GET | All | Lấy danh sách series | 🔴 High |
| `/api/series/{id}` | GET | All | Lấy chi tiết series | 🔴 High |
| `/api/series` | POST | Mangaka | Tạo bản nháp đề xuất (`Draft`) | 🔴 High |
| `/api/proposals/{seriesId}/submit-review` | POST | Mangaka | Nộp bản nháp cho Tantou xem xét (`Draft→UnderReview`) | 🔴 High |
| `/api/proposals/{seriesId}/reject` | POST | TantouEditor | Từ chối proposal (`UnderReview→Rejected`) | 🔴 High |
| `/api/proposals/{seriesId}/submit-to-board` | POST | TantouEditor | Chuyển lên Editorial Board bỏ phiếu (`UnderReview→BoardVoting`) | 🔴 High |
| `/api/proposals/{seriesId}/activate` | POST | TantouEditor | Kích hoạt series đã được phê duyệt (`Approved→Active`) | 🔴 High |

### Board Decisions & Voting

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/api/series/{seriesId}/board-decisions` | GET | All | Lấy danh sách board decisions của series | 🔴 High |
| `/api/board-decisions/{id}` | GET | All | Lấy chi tiết board decision | 🔴 High |
| `/api/board-decisions/{boardDecisionId}/votes` | GET | All | Lấy danh sách phiếu bầu | 🟡 Medium |
| `/api/board-decisions/{boardDecisionId}/votes` | POST | EditorialBoard | Bỏ phiếu cho board decision | 🔴 High |
| `/api/board-decisions/{id}/extend-deadline` | POST | EditorInChief | Gia hạn deadline (1 lần, BR-New-01) | 🔴 High |
| `/api/board-decisions/{id}/special-decision` | POST | EditorInChief | Quyết định đặc biệt sau gia hạn (BR-New-02) | 🔴 High |

### Chapters

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/api/chapters` | GET | All | Lấy toàn bộ danh sách chương | 🔴 High |
| `/api/chapters/series/{seriesId}` | GET | All | Lấy danh sách chương của series | 🔴 High |
| `/api/chapters` | POST | Mangaka | Tạo chương mới | 🔴 High |
| `/api/chapters/{id}` | PUT | Mangaka | Cập nhật thông tin/trạng thái chương | 🟡 Medium |

### Manuscripts, Tasks & Others

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/api/manuscripts` | GET | All | Lấy danh sách bản thảo | 🔴 High |
| `/api/manuscripts/{id}` | GET | All | Xem chi tiết một bản thảo | 🟡 Medium |
| `/api/manuscripts` | POST | Mangaka | Nộp bản thảo chương lên BTV | 🔴 High |
| `/api/notifications` | GET | All | Lấy danh sách thông báo cá nhân | 🔴 High |
| `/api/notifications/{id}/read` | PUT | All | Đánh dấu thông báo đã đọc | 🟡 Medium |

---

## 13. Ghi chú & Quy tắc nghiệp vụ

### Proposal Workflow

* **Validation (BR-15, đã cập nhật):** `title` ≤ 100 ký tự; `synopsis` từ **100 đến 2000** ký tự; ít nhất 5 sample pages; genre hợp lệ; publication type hợp lệ.
* **Single active proposal (BR-19):** Một Mangaka không được có hơn 1 proposal ở `Draft`, `UnderReview`, hoặc `BoardVoting` cùng lúc.
* **Unique title (BR-17):** Title proposal không được trùng với title series đang `Active`.
* **Object-level auth:** Tantou Editor phải được gán cho Mangaka đó qua `UserAssignment` (`AssignmentType = "TantouEditor"`, active, không bị unassign).
* **Board submission:** Tạo `BoardDecision` với deadline 7 ngày; gửi notification đến tất cả `EditorialBoard` active. Không có recipient → lỗi.
* **Quorum (BR-29):** Tối thiểu 3 phiếu hợp lệ (không xung đột lợi ích).
* **Majority (BR-33):** Approve > 50% phiếu hợp lệ → Approved; Reject > 50% → Rejected; bằng nhau sau deadline → Tie → notify EditorInChief.
* **No quorum (BR-37):** Hết deadline mà chưa đủ 3 phiếu → `Series.Status = Expired`, notify EditorInChief.
* **Reject vote (BR-35):** Vote từ chối bắt buộc phải có `comment` ≥ 50 ký tự.
* **Xung đột lợi ích (BR-28):** Loại trừ Mangaka, Tantou Editor được gán, Assistant được gán, người tạo proposal/decision.
* **Deadline extension (BR-New-01):** EditorInChief chỉ được gia hạn 1 lần cho decision kết thúc bằng Tie/NoQuorum.
* **Special decision (BR-New-02):** Sau deadline gia hạn vẫn không finalize được, EditorInChief đưa ra quyết định cuối `Approved` hoặc `Rejected`.
* **Activation (BR-24):** Chỉ Tantou Editor được gán mới kích hoạt series đã `Approved`. Mangaka không được tự kích hoạt.

### Các quy tắc khác

* **Định dạng Datetime:** Chuẩn ISO 8601 UTC. Ví dụ: `"2026-06-01T08:30:00Z"`.
* **Deadline chương (BR-42):** Deadline = publicationDate − 14 ngày; publicationDate phải cách ngày tạo ít nhất 17 ngày.
* **Chapter submission (BR-46, BR-67):** 100% PageTask của chương phải `Approved` trước khi nộp bản thảo.
* **Quorum cancellation (BR-101):** Quyết định hủy series cũng yêu cầu tối thiểu 3 phiếu hợp lệ.
* **Finalization là backend-owned:** FE chỉ hiển thị trạng thái và gọi các action được phép. Backend tự động xử lý quorum, majority, deadline qua `BoardDecisionDeadlineWorker` (chạy mỗi 1–5 phút).
