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
| `Proposed` | Mới đề xuất, đang chờ Editorial Board bỏ phiếu |
| `Active` | Đang trong tiến trình sáng tác và xuất bản đều đặn |
| `Rejected` | Đề xuất bị Editorial Board bác bỏ |
| `Cancelled` | Series đã bị hủy xuất bản (sau khi rơi vào bottom 20% và được Board quyết định) |

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

### `GET /series`

Lấy danh sách các series và các bản đề xuất.

**Response `200`**

```json
[
  {
    "id": "S01",
    "title": "Demon Slayer: Chronicles",
    "author": "Koyoharu Gotouge",
    "genre": ["Action", "Fantasy"],
    "type": "Weekly",
    "status": "Active",
    "description": "A young man sets out to become a demon slayer...",
    "coverColor": "from-red-500 to-rose-700",
    "rating": 4.9
  }
]
```

---

### `GET /series/:id`

Lấy thông tin chi tiết một series theo `id`.

**Response `200`**

```json
{
  "id": "S01",
  "title": "Demon Slayer: Chronicles",
  "author": "Koyoharu Gotouge",
  "genre": ["Action", "Fantasy"],
  "type": "Weekly",
  "status": "Active",
  "description": "A young man sets out to become a demon slayer...",
  "coverColor": "from-red-500 to-rose-700",
  "rating": 4.9
}
```

---

### `POST /series/proposal`

Gửi bản đề xuất series truyện mới lên Ban biên tập (chỉ dùng cho Mangaka).

**Request body**

```json
{
  "title": "Sakura Knights",
  "genre": "Action, Romance",
  "publicationType": "Weekly",
  "synopsis": "In feudal Japan reimagined with magitech armor, five orphaned warriors...",
  "samplePages": 8,
  "mangakaId": "U01",
  "coverImageUrl": "https://api.mangahub.vn/covers/sakura-knights.jpg"
}
```

> **Ràng buộc kiểm tra đề xuất (BR-15 & BR-19):**
> - `synopsis` phải từ 200 đến 2000 ký tự.
> - Số trang vẽ thử mẫu `samplePages` phải $\ge 5$.
> - Mangaka không được phép gửi đề xuất mới nếu đang có đề xuất khác ở trạng thái `Pending Review` hoặc `Under Review`.

**Response `201`**

```json
{
  "success": true,
  "proposal": {
    "id": "PR02",
    "title": "Sakura Knights",
    "author": "Tanaka Yuki",
    "genre": ["Action", "Romance"],
    "type": "Weekly",
    "status": "Proposed",
    "description": "In feudal Japan reimagined with magitech armor...",
    "coverColor": "from-pink-500 to-purple-600",
    "rating": 0
  }
}
```

---

### `POST /series/:seriesId/vote`

Editorial Board bỏ phiếu phê duyệt đề xuất.

**Request body**

```json
{
  "vote": "Approved"
}
```

> **Quy tắc bỏ phiếu (BR-01 & BR-05):**
> - Tantou Editor trực tiếp quản lý series **KHÔNG ĐƯỢC PHÉP** bỏ phiếu cho series đó.
> - Đề xuất cần tối thiểu **3 phiếu bầu (quorum)** để thông qua trạng thái `Active`. Nếu dưới 3 phiếu, trạng thái sẽ là `Deferred`.

**Response `200`**

```json
{
  "success": true,
  "votes": 120
}
```

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

| Endpoint | Method | Role | Mô tả | Priority |
|---|---|---|---|---|
| `/auth/login` | POST | All | Đăng nhập hệ thống | 🔴 High |
| `/auth/logout` | POST | All | Đăng xuất hệ thống | 🔴 High |
| `/auth/me` | GET | All | Lấy thông tin user hiện tại | 🔴 High |
| `/series` | GET | All | Lấy danh sách series & đề cử | 🔴 High |
| `/series/:id` | GET | All | Lấy chi tiết tác phẩm | 🔴 High |
| `/series/proposal` | POST | Mangaka | Tạo đề xuất tác phẩm mới | 🔴 High |
| `/series/:seriesId/vote` | POST | Board/BTV | Bỏ phiếu đề xuất | 🟡 Medium |
| `/chapters` | GET | All | Lấy toàn bộ danh sách chương | 🔴 High |
| `/chapters/series/:seriesId` | GET | All | Lấy danh sách chương của series | 🔴 High |
| `/chapters` | POST | Mangaka | Tạo chương mới | 🔴 High |
| `/chapters/:id` | PUT | Mangaka | Cập nhật thông tin/trạng thái chương | 🟡 Medium |
| `/manuscripts` | GET | All | Lấy danh sách bản thảo | 🔴 High |
| `/manuscripts/:id` | GET | All | Xem chi tiết một bản thảo | 🟡 Medium |
| `/manuscripts` | POST | Mangaka | Nộp bản thảo chương lên BTV | 🔴 High |
| `/tasks` | GET | All | Xem danh sách task phân vẽ trang | 🔴 High |
| `/tasks/assign` | POST | Mangaka | Giao task vẽ trang cho Assistant | 🔴 High |
| `/reviews` | GET | Board/BTV | Xem danh sách đề cử cần quyết định | 🟡 Medium |
| `/reviews/:id/decision` | PUT | Board/BTV | Phê duyệt/từ chối đề xuất | 🟡 Medium |
| `/ranking` | GET | All | Lấy bảng xếp hạng series truyện | 🔴 High |
| `/ranking/confirm` | POST | Board | Xác nhận danh sách xếp hạng quý | 🟢 Low |
| `/votes/submit` | POST | Reader | Gửi phiếu bình chọn cho series | 🔴 High |
| `/notifications` | GET | All | Lấy danh sách thông báo cá nhân | 🔴 High |
| `/notifications/:id/read` | PUT | All | Đánh dấu thông báo đã đọc | 🟡 Medium |

---

## 13. Ghi chú & Quy tắc nghiệp vụ

### Đã thống nhất giữa FE và BE
* **Định dạng Datetime:** Chuẩn ISO 8601 UTC. Ví dụ: `"2026-06-01T08:30:00Z"`.
* **Ràng buộc nộp đề xuất (BR-15):** Phải kiểm tra tính hợp lệ của `synopsis` (từ 200 đến 2000 ký tự) và số lượng `samplePages` vẽ nháp (tối thiểu 5 trang).
* **Ràng buộc trễ hạn chương (BR-03):** Hạn chót nộp bản thảo (`deadline`) được hệ thống tính tự động lùi **14 ngày** so với ngày dự kiến xuất bản (`publicationDate`). Trễ deadline sẽ tự động kích hoạt notification cảnh báo cho tác giả và BTV phụ trách.
* **Tiến trình hoàn thành chương (BR-04):** Để có thể bấm nút nộp bản thảo chương (`POST /manuscripts`), 100% các page task của chương đó bắt buộc phải có trạng thái là `Approved` từ Mangaka.
* **Hạn chế xung đột lợi ích (BR-01):** BTV phụ trách chính của một series không được tham gia bỏ phiếu duyệt đề cử của series đó.
* **Cơ chế biểu quyết quorum (BR-05):** Yêu cầu tối thiểu 3 phiếu từ Editorial Board để đưa ra quyết định duyệt đề cử hoặc chấm dứt hợp đồng xuất bản series.
