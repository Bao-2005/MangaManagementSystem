# MangaHub — API Specification

> **Version:** 1.0  
> **Base URL:** `https://api.mangahub.vn/api/v1` (Hoặc `http://localhost:3000/api/v1` cho môi trường Local)  
> **Auth:** Bearer token — header `Authorization: Bearer <token>`  
> Tất cả endpoints (trừ `/auth/login`) đều yêu cầu Authentication.

---

## Mục lục

1. [Enum Values](#1-enum-values)
2. [Data Models](#2-data-models)
3. [Authentication Endpoints](#3-authentication-endpoints)
4. [Series & Proposals Endpoints](#4-series--proposals-endpoints)
5. [Chapters Endpoints](#5-chapters-endpoints)
6. [Page Tasks Endpoints](#6-page-tasks-endpoints)
7. [Manuscripts Endpoints](#7-manuscripts-endpoints)
8. [Reader Votes & Rankings Endpoints](#8-reader-votes--rankings-endpoints)
9. [Dashboard & Audit Endpoints](#9-dashboard--audit-endpoints)
10. [Error & Response Format](#10-error--response-format)

---

## 1. Enum Values

### `UserRole`

| Value | Label | Mô tả |
|---|---|---|
| `Mangaka` | Tác giả | Tạo đề xuất series, giao việc cho Assistant, duyệt trang vẽ. |
| `Assistant` | Trợ lý | Nhận task vẽ trang, vẽ và nộp lại cho Mangaka duyệt. |
| `Tantou Editor` | BTV phụ trách | Xem và duyệt bản thảo (Manuscript), viết feedback/annotation. |
| `Editorial Board` | Ban biên tập | Bỏ phiếu đề xuất mới, nhập vote độc giả, duyệt quyết định hủy/đổi lịch. |

### `Genre` (Thể loại)
`Action` | `Drama` | `Romance` | `Fantasy` | `Sci-Fi` | `Comedy` | `Thriller` | `Horror` | `Slice of Life` | `Mystery`

### `PublicationType` (Chu kỳ xuất bản)
`Weekly` (Hàng tuần) | `Monthly` (Hàng tháng) | `One-shot` (Một tập duy nhất)

### `SeriesStatus`

| Value | Ý nghĩa |
|---|---|
| `Proposed` | Mới đề xuất, đang chờ Editorial Board bỏ phiếu |
| `Active` | Đang trong tiến trình sáng tác và xuất bản đều đặn |
| `Rejected` | Đề xuất bị Editorial Board bác bỏ |
| `Deferred` | Đề xuất bị hoãn bỏ phiếu (khi chưa đủ quorum 3 phiếu) |
| `Cancelled` | Series đã bị hủy xuất bản (sau khi rơi vào bottom 20% và được Board quyết định) |

### `TaskStatus` (Trạng thái của PageTask)
`Unassigned` (Chưa giao) | `Pending` (Đang chờ nhận) | `In-Progress` (Đang vẽ) | `Submitted` (Đã nộp trang, chờ Mangaka duyệt) | `Approved` (Trang vẽ đã được Mangaka thông qua) | `Rejected` (Trang vẽ bị lỗi, bắt buộc vẽ lại) | `Suspended` (Đã tạm dừng)

### `ManuscriptStatus` (Trạng thái của Bản thảo chương)
`Pending` (Chờ Tantou Editor duyệt) | `Approved` (Đã duyệt xuất bản) | `Revision Required` (Yêu cầu chỉnh sửa lại)

---

## 2. Data Models

### `User`
```typescript
interface User {
  id: string
  email: string
  name: string
  role: "Mangaka" | "Assistant" | "Tantou Editor" | "Editorial Board"
}
```

### `Series`
```typescript
interface Series {
  id: string
  title: string
  genre: string
  publicationType: "Weekly" | "Monthly" | "One-shot"
  description: string
  coverImageUrl?: string
  status: "Proposed" | "Active" | "Rejected" | "Deferred" | "Cancelled"
  createdBy: string // ID của Mangaka tạo
  assignedEditorId?: string // ID của BTV phụ trách
  votes?: Record<string, "Approved" | "Rejected"> // Danh sách phiếu của ban biên tập
}
```

### `Chapter`
```typescript
interface Chapter {
  id: string
  seriesId: string
  title: string
  chapterNo: number
  publicationDate: string // ISO date string
  deadline: string // ISO date string (PubDate - 14 ngày)
  overdue: boolean // Tự động tính toán nếu quá deadline mà chưa có bản thảo Approved
  progress: number // Tỷ lệ phần trăm số trang Approved trong chương (0-100%)
}
```

### `PageTask`
```typescript
interface PageTask {
  id: string
  chapterId: string
  pageRange: string // Ví dụ: "1-5", "6-10" hoặc "Page 3"
  pageNumber: number // Trang cụ thể bắt đầu
  status: "Unassigned" | "Pending" | "In-Progress" | "Submitted" | "Approved" | "Rejected" | "Suspended"
  assignedToId?: string // ID của Assistant
  rejectionReason?: string // Lý do reject của Mangaka
}
```

### `ManuscriptVersion`
```typescript
interface ManuscriptVersion {
  id: string
  seriesId: string
  chapterId: string
  versionLabel: string // Tự động tăng v1, v2, v3...
  fileUrl: string // Đường dẫn tải bản thảo (PDF, ZIP, v.v.)
  submittedAt: string // ISO datetime
  status: "Pending" | "Approved" | "Revision Required"
  revisionNotes?: string // Feedback chú thích từ Tantou Editor
}
```

### `VoteRecord`
```typescript
interface VoteRecord {
  id: string
  seriesId: string
  chapterId: string
  readerCount: number // Số lượng độc giả đọc chương
  voteCount: number // Số lượng bình chọn
  confirmed: boolean // Trạng thái xác nhận từ Editorial Board
  calculatedScore?: number // Điểm vote: (voteCount / readerCount) * 100%
  enteredAt: string // ISO datetime
}
```

---

## 3. Authentication Endpoints

### `POST /auth/login`
Đăng nhập hệ thống.

**Request Body:**
```json
{
  "email": "mangaka@mangahub.vn",
  "password": "password123"
}
```

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "token": "jwt_token_here",
    "user": {
      "id": "u-001",
      "email": "mangaka@mangahub.vn",
      "name": "Tác giả A",
      "role": "Mangaka"
    }
  }
}
```

---

### `GET /auth/me`
Lấy thông tin user hiện tại dựa trên Bearer token.

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "u-001",
    "email": "mangaka@mangahub.vn",
    "name": "Tác giả A",
    "role": "Mangaka"
  }
}
```

---

## 4. Series & Proposals Endpoints

### `GET /series`
Lấy danh sách các tác phẩm/đề xuất.

**Query Params:**
* `search` (string): Tìm theo tiêu đề hoặc mô tả.
* `genre` (string): Lọc theo thể loại.
* `status` (SeriesStatus): Lọc theo trạng thái đề xuất/xuất bản.
* `page` (number, default: 1): Trang hiện tại.
* `limit` (number, default: 10): Số lượng dòng mỗi trang.

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "content": [
      {
        "id": "ser-001",
        "title": "Hành trình Đại Việt",
        "genre": "Action",
        "publicationType": "Weekly",
        "description": "Câu chuyện phiêu lưu dã sử...",
        "coverImageUrl": "https://api.mangahub.vn/covers/dai-viet.jpg",
        "status": "Active",
        "createdBy": "u-001",
        "assignedEditorId": "u-editor-01"
      }
    ],
    "totalElements": 1,
    "totalPages": 1,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

---

### `GET /series/:id`
Lấy chi tiết một tác phẩm kèm danh sách phiếu bầu nếu người dùng có quyền (Editorial Board).

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "id": "ser-001",
    "title": "Hành trình Đại Việt",
    "genre": "Action",
    "publicationType": "Weekly",
    "description": "Câu chuyện phiêu lưu dã sử...",
    "coverImageUrl": "https://api.mangahub.vn/covers/dai-viet.jpg",
    "status": "Proposed",
    "createdBy": "u-001",
    "votes": {
      "board-member-1": "Approved",
      "board-member-2": "Approved"
    }
  }
}
```

---

### `POST /series/proposals`
Mangaka nộp đề xuất bộ truyện mới.

**Request Body:**
```json
{
  "title": "Hành trình Đại Việt",
  "genre": "Action",
  "publicationType": "Weekly",
  "description": "Mô tả cốt truyện chi tiết cho bộ truyện...",
  "coverImageUrl": "https://api.mangahub.vn/covers/dai-viet.jpg"
}
```

**Response `201`:** Trả về đối tượng `Series` vừa tạo kèm `status: "Proposed"`.

---

### `POST /series/:id/vote`
Thành viên Ban biên tập bỏ phiếu đồng ý/không đồng ý phê duyệt xuất bản bộ truyện đề xuất.
> **Quy tắc (BR-01):** BTV phụ trách trực tiếp (`assignedEditorId`) **KHÔNG** được bỏ phiếu cho series do mình quản lý.

**Request Body:**
```json
{
  "vote": "Approved" // Hoặc "Rejected"
}
```

**Response `200`:**
```json
{
  "success": true,
  "message": "Bỏ phiếu thành công",
  "data": {
    "seriesId": "ser-001",
    "approvalsCount": 3,
    "rejectionsCount": 0,
    "status": "Active" // Tự động cập nhật thành "Active" khi đủ quorum >= 3 votes đồng ý, hoặc "Rejected" / "Deferred".
  }
}
```

---

## 5. Chapters Endpoints

### `GET /series/:seriesId/chapters`
Lấy danh sách các chương của một series.

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ch-101",
      "seriesId": "ser-001",
      "title": "Chương 1: Khởi đầu mới",
      "chapterNo": 1,
      "publicationDate": "2026-06-15",
      "deadline": "2026-06-01",
      "overdue": false,
      "progress": 80
    }
  ]
}
```

---

### `POST /series/:seriesId/chapters`
Mangaka tạo chương mới cho series.
> **Quy tắc (BR-03):** Deadline của chương sẽ tự động được tính là **ngày xuất bản trừ đi 14 ngày**.

**Request Body:**
```json
{
  "title": "Chương 1: Khởi đầu mới",
  "chapterNo": 1,
  "publicationDate": "2026-06-15"
}
```

**Response `201`:** Trả về thông tin chương mới tạo kèm `deadline` đã tự động tính.

---

## 6. Page Tasks Endpoints

### `GET /chapters/:chapterId/tasks`
Lấy danh sách các trang vẽ đã được phân công/chưa phân công trong chương.

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "task-501",
      "chapterId": "ch-101",
      "pageRange": "Trang 1-5",
      "pageNumber": 1,
      "status": "In-Progress",
      "assignedToId": "u-assistant-01"
    }
  ]
}
```

---

### `POST /tasks`
Mangaka phân công đầu việc vẽ trang cho Assistant.

**Request Body:**
```json
{
  "chapterId": "ch-101",
  "pageStart": 1,
  "pageEnd": 5,
  "assignedToId": "u-assistant-01",
  "deadline": "2026-05-30T17:00:00Z"
}
```

**Response `201`:** Trả về thông tin PageTask đã tạo.

---

### `PUT /tasks/:id/status`
Cập nhật trạng thái trang vẽ (Assistant nộp trang hoặc Mangaka Duyệt/Từ chối).

**Request Body:**
```json
{
  "status": "Submitted", // Hoặc "Approved" / "Rejected"
  "rejectionReason": "Đường line nét vẽ quá mờ, cần tô đậm hơn" // Gửi khi reject
}
```

**Response `200`:** Trả về đối tượng `PageTask` sau khi thay đổi trạng thái.

---

## 7. Manuscripts Endpoints

### `GET /chapters/:chapterId/manuscripts`
Lấy danh sách lịch sử các phiên bản bản thảo của chương.

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ms-001",
      "seriesId": "ser-001",
      "chapterId": "ch-101",
      "versionLabel": "v1",
      "fileUrl": "https://api.mangahub.vn/manuscripts/ch101_v1.zip",
      "submittedAt": "2026-05-27T08:00:00Z",
      "status": "Revision Required",
      "revisionNotes": "Cần sửa lại khung thoại ở trang số 3."
    }
  ]
}
```

---

### `POST /manuscripts`
Mangaka nộp bản thảo chương lên hệ thống cho Biên tập viên xem xét.
> **Quy tắc (BR-04 / BR-05):** Chỉ được phép nộp bản thảo khi **100% các PageTasks trong chương đó đạt trạng thái "Approved"**. Hệ thống sẽ tự động sinh version mới (v1, v2, v3...).

**Request Body:**
```json
{
  "seriesId": "ser-001",
  "chapterId": "ch-101",
  "fileUrl": "https://api.mangahub.vn/manuscripts/ch101_v2.zip",
  "notes": "Đã chỉnh sửa nét vẽ theo yêu cầu."
}
```

**Response `201`:** Trả về thông tin bản thảo `ManuscriptVersion` vừa nộp với trạng thái `Pending`.

---

### `PUT /manuscripts/:id/review`
Tantou Editor duyệt bản thảo chương hoặc phản hồi yêu cầu sửa đổi.

**Request Body:**
```json
{
  "status": "Approved", // Hoặc "Revision Required"
  "revisionNotes": "Tranh vẽ xuất sắc, duyệt xuất bản!" // Bắt buộc nhập nếu status là Revision Required
}
```

**Response `200`:** Trả về bản thảo đã được cập nhật trạng thái review.

---

## 8. Reader Votes & Rankings Endpoints

### `POST /votes`
Ban biên tập nhập số lượng bình chọn của độc giả cho một chương truyện đã phát hành.

**Request Body:**
```json
{
  "seriesId": "ser-001",
  "chapterId": "ch-101",
  "readerCount": 5000,
  "voteCount": 3500
}
```

**Response `201`:**
```json
{
  "success": true,
  "data": {
    "id": "vote-901",
    "seriesId": "ser-001",
    "chapterId": "ch-101",
    "readerCount": 5000,
    "voteCount": 3500,
    "calculatedScore": 70, // Tự động tính: (3500 / 5000) * 100
    "confirmed": true,
    "enteredAt": "2026-05-28T09:40:00Z"
  }
}
```

---

### `GET /rankings`
Lấy danh sách bảng xếp hạng tự động của các tác phẩm dựa trên chỉ số vote.
> **Quy tắc (BR-02):** Điểm đánh giá = `(voteCount / readerCount) * 100%`.
> Nhóm **Bottom 20%** có điểm thấp nhất sẽ tự động nhận flag cảnh báo hủy xuất bản (`belowThreshold: true`).

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "seriesId": "ser-001",
      "title": "Hành trình Đại Việt",
      "score": 70.0,
      "rank": 1,
      "belowThreshold": false
    },
    {
      "seriesId": "ser-002",
      "title": "Ký ức thời gian",
      "score": 12.5,
      "rank": 5,
      "belowThreshold": true // Nằm trong nhóm bottom 20%
    }
  ]
}
```

---

## 9. Dashboard & Audit Endpoints

### `GET /dashboard/summary`
Lấy thông tin tổng hợp hiển thị màn hình chính tùy theo vai trò của user (tự trích xuất từ token JWT).

**Response `200`:**
```json
{
  "success": true,
  "data": {
    "role": "Mangaka",
    "summary": {
      "totalSeries": 2,
      "pendingReviews": 1,
      "overdueChapters": 0,
      "earningsEstimate": 450.0 // Ước tính thu nhập của studio/trợ lý (nếu có)
    }
  }
}
```

---

### `GET /audit-logs`
Danh sách lưu vết thao tác hệ thống (dành cho Admin / Editorial Board).

**Response `200`:**
```json
{
  "success": true,
  "data": [
    {
      "id": "log-7001",
      "actorId": "u-001",
      "action": "SUBMIT_PROPOSAL",
      "targetType": "Series",
      "targetId": "ser-001",
      "details": "Mangaka nộp đề xuất truyện mới: Hành trình Đại Việt",
      "createdAt": "2026-05-28T02:00:00Z"
    }
  ]
}
```

---

## 10. Error & Response Format

### Định dạng phản hồi chuẩn
Tất cả các API kết quả trả về luôn bọc trong 1 wrapper JSON:
* **Thành công:** `{ "success": true, "data": ... }`
* **Lỗi:** `{ "success": false, "error": "Mô tả chi tiết nguyên nhân lỗi" }`

### Http Status Codes thông dụng
* `400 Bad Request`: Thông tin gửi lên không đúng định dạng hoặc vi phạm ràng buộc dữ liệu (ví dụ: `voteCount > readerCount`).
* `401 Unauthorized`: Chưa truyền header Bearer token hoặc token đã hết hạn.
* `403 Forbidden`: Người dùng không có quyền truy cập chức năng này (ví dụ: Assistant cố ý vote duyệt đề xuất truyện).
* `404 Not Found`: Không tìm thấy tài nguyên tương ứng (Chương, Nhân vật, Tác phẩm).
* `500 Internal Server Error`: Lỗi phát sinh từ server.
