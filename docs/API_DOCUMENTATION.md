# Manga Management System API Documentation

Tài liệu này là nguồn mô tả API chính của Manga Management System.

> Trạng thái tài liệu: đang được xây dựng. Hiện tại nhóm **Page Task Workflow APIs**
> đã được mô tả; các nhóm còn lại sẽ được bổ sung dần theo code hiện hành.

## 1. Tổng quan

### 1.1. Base URL

```text
https://<host>/api
```

Khi chạy local, thay `<host>` bằng địa chỉ được cấu hình trong launch profile của
Web API.

### 1.2. Xác thực

Các API trong tài liệu này sử dụng JWT Bearer Token, trừ khi endpoint ghi rõ là
không yêu cầu xác thực.

```http
Authorization: Bearer <access-token>
```

### 1.3. Vai trò

| Role | Mô tả |
|---|---|
| `Admin` | Quản trị viên hệ thống |
| `Mangaka` | Tác giả manga |
| `TantouEditor` | Biên tập viên phụ trách Mangaka |
| `Assistant` | Trợ lý thực hiện page task |
| `EditorialBoard` | Thành viên ban biên tập |
| `EditorInChief` | Tổng biên tập |

### 1.4. Success response chung

Các API thành công thường trả về cấu trúc:

```json
{
  "data": {},
  "message": "Success"
}
```

| Field | Type | Mô tả |
|---|---|---|
| `data` | object, array hoặc null | Dữ liệu trả về của API |
| `message` | string hoặc null | Thông báo kết quả |

### 1.5. Error response chung

Lỗi nghiệp vụ được trả về theo `ProblemDetails`:

```json
{
  "title": "Lỗi hệ thống",
  "status": 404,
  "detail": "Page task not found.",
  "instance": "/api/page-tasks/00000000-0000-0000-0000-000000000000/submissions"
}
```

| HTTP status | Ý nghĩa |
|---|---|
| `400 Bad Request` | Request hoặc dữ liệu nghiệp vụ không hợp lệ |
| `401 Unauthorized` | Thiếu token, token không hợp lệ hoặc không sở hữu tài nguyên |
| `403 Forbidden` | Đã xác thực nhưng không có role được yêu cầu |
| `404 Not Found` | Không tìm thấy tài nguyên |
| `409 Conflict` | Trạng thái hiện tại không cho phép thực hiện thao tác |
| `500 Internal Server Error` | Lỗi hệ thống chưa được xử lý cụ thể |

> Lưu ý: theo implementation hiện tại, một số lỗi ownership được ánh xạ thành
> `401 Unauthorized` thay vì `403 Forbidden`.

### 1.6. Quy ước dữ liệu

- ID sử dụng UUID/GUID.
- Thời gian sử dụng ISO 8601 và nên gửi theo UTC, ví dụ
  `2026-06-20T10:00:00Z`.
- JSON request và response sử dụng tên field dạng `camelCase`.
- Enum trong JSON response hiện được serialize dưới dạng số.

## 2. Authentication APIs

Chưa được mô tả.

## 3. User and Assignment APIs

Chưa được mô tả.

## 4. Series and Proposal APIs

Chưa được mô tả.

## 5. Chapter and Manuscript APIs

Chưa được mô tả.

## 6. Page Task Workflow APIs

### 6.1. Mục đích

Nhóm API này hỗ trợ luồng:

1. Mangaka tạo page task và giao cho Assistant.
2. Assistant xem danh sách task được giao.
3. Assistant upload file sản phẩm.
4. Assistant submit sản phẩm để Mangaka review.
5. Mangaka approve hoặc reject submission.
6. Nếu bị reject, Assistant chỉnh sửa và submit phiên bản mới.

### 6.2. Sơ đồ trạng thái

```text
PageTask: Assigned
        |
        | Assistant submit
        v
PageTask: Completed
Submission: Submitted
        |
        +-------------------------+
        |                         |
        | Mangaka approve         | Mangaka reject
        v                         v
PageTask: Approved        PageTask: InProgress
Submission: Approved      Submission: Rejected
                                  |
                                  | Assistant submit lại
                                  v
                         PageTask: Completed
                         Submission: Submitted
```

### 6.3. Business rules

- Mangaka chỉ được tạo task cho chapter thuộc series của chính mình.
- Chapter phải tồn tại và chưa bị xóa.
- Chapter phải có ít nhất một manuscript; task tự động dùng manuscript version
  mới nhất.
- `pageStart` phải nhỏ hơn hoặc bằng `pageEnd`.
- `pageEnd` không được vượt quá tổng số trang của chapter.
- Người được giao task phải có role `Assistant`.
- Assistant chỉ được submit task được giao cho chính mình.
- Task đã `Approved` không thể submit lại.
- Mỗi task chỉ được có một submission ở trạng thái `Submitted` tại một thời điểm.
- Mangaka chỉ được review submission thuộc series của chính mình.
- Chỉ submission ở trạng thái `Submitted` mới được approve hoặc reject.
- Reject bắt buộc phải có `rejectReason`.
- Mỗi lần submit lại, `versionNo` tự động tăng thêm một.

### 6.4. API summary

| # | Method | Endpoint | Role | Mô tả |
|---|---|---|---|---|
| 1 | `POST` | `/api/page-tasks` | Mangaka | Tạo và giao page task |
| 2 | `GET` | `/api/page-tasks/mangaka` | Mangaka | Xem task thuộc các series của Mangaka |
| 3 | `GET` | `/api/page-tasks/assistant` | Assistant | Xem task được giao cho Assistant |
| 4 | `POST` | `/api/files` | Authenticated | Upload file sản phẩm |
| 5 | `POST` | `/api/page-tasks/{pageTaskId}/submissions` | Assistant | Submit kết quả làm việc |
| 6 | `POST` | `/api/page-tasks/submissions/{submissionId}/approve` | Mangaka | Approve submission |
| 7 | `POST` | `/api/page-tasks/submissions/{submissionId}/reject` | Mangaka | Reject submission |

### 6.5. Tạo và giao page task

Tạo page task cho một khoảng trang trong chapter và giao cho Assistant.

**Endpoint**

```http
POST /api/page-tasks
```

**Authorization:** `Mangaka`

**Request body**

```json
{
  "chapterId": "11111111-1111-1111-1111-111111111111",
  "assistantId": "22222222-2222-2222-2222-222222222222",
  "pageStart": 1,
  "pageEnd": 5,
  "taskType": "Line Art",
  "description": "Hoàn thiện line art cho trang 1 đến trang 5.",
  "dueDate": "2026-06-20T10:00:00Z"
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `chapterId` | UUID | Có | Chapter phải thuộc series của Mangaka |
| `assistantId` | UUID | Có | User phải có role `Assistant` |
| `pageStart` | integer | Có | Lớn hơn hoặc bằng `1` |
| `pageEnd` | integer | Có | Lớn hơn hoặc bằng `pageStart`, không vượt tổng số trang |
| `taskType` | string | Có | Tối đa 50 ký tự |
| `description` | string | Không | Tối đa 1000 ký tự |
| `dueDate` | datetime | Không | ISO 8601 |

**Success response: `200 OK`**

```json
{
  "data": {
    "pageTaskId": "33333333-3333-3333-3333-333333333333",
    "chapterId": "11111111-1111-1111-1111-111111111111",
    "manuscriptId": "44444444-4444-4444-4444-444444444444",
    "assistantId": "22222222-2222-2222-2222-222222222222",
    "assistantName": "Assistant Name",
    "pageStart": 1,
    "pageEnd": 5,
    "taskType": "Line Art",
    "description": "Hoàn thiện line art cho trang 1 đến trang 5.",
    "dueDate": "2026-06-20T10:00:00Z",
    "status": 0,
    "createdAt": "2026-06-12T08:00:00Z",
    "approvedAt": null,
    "updatedAt": null,
    "submissions": []
  },
  "message": "Task assigned successfully."
}
```

**State transition:** tạo mới với `PageTaskStatus.Assigned`.

**Error cases**

| Status | Trường hợp |
|---|---|
| `400` | Page range không hợp lệ, vượt tổng số trang hoặc user không phải Assistant |
| `401` | Thiếu thông tin user hoặc chapter không thuộc Mangaka |
| `403` | User không có role Mangaka |
| `404` | Không tìm thấy chapter, manuscript hoặc Assistant |

### 6.6. Lấy danh sách task của Mangaka

Trả về các page task thuộc chapter trong series của Mangaka đang đăng nhập.

**Endpoint**

```http
GET /api/page-tasks/mangaka
```

**Authorization:** `Mangaka`

**Request:** không có path parameter, query parameter hoặc request body.

**Success response: `200 OK`**

```json
{
  "data": [
    {
      "pageTaskId": "33333333-3333-3333-3333-333333333333",
      "chapterId": "11111111-1111-1111-1111-111111111111",
      "manuscriptId": "44444444-4444-4444-4444-444444444444",
      "assistantId": "22222222-2222-2222-2222-222222222222",
      "assistantName": "Assistant Name",
      "pageStart": 1,
      "pageEnd": 5,
      "taskType": "Line Art",
      "description": "Hoàn thiện line art cho trang 1 đến trang 5.",
      "dueDate": "2026-06-20T10:00:00Z",
      "status": 2,
      "createdAt": "2026-06-12T08:00:00Z",
      "approvedAt": null,
      "updatedAt": "2026-06-13T08:00:00Z",
      "submissions": [
        {
          "submissionId": "55555555-5555-5555-5555-555555555555",
          "pageTaskId": "33333333-3333-3333-3333-333333333333",
          "versionNo": 1,
          "submittedFileAssetId": "66666666-6666-6666-6666-666666666666",
          "originalFileName": "chapter-1-pages-1-5.zip",
          "objectPath": "task-submissions/example.zip",
          "status": 0,
          "note": "Đã hoàn thành.",
          "rejectReason": null,
          "submittedAt": "2026-06-13T08:00:00Z",
          "reviewedAt": null
        }
      ]
    }
  ],
  "message": "Success"
}
```

Task được sắp xếp theo `createdAt` mới nhất. Submissions trong từng task được sắp
xếp theo `versionNo` giảm dần.

**Error cases**

| Status | Trường hợp |
|---|---|
| `401` | Thiếu hoặc token không hợp lệ |
| `403` | User không có role Mangaka |

### 6.7. Lấy danh sách task của Assistant

Trả về các page task được giao cho Assistant đang đăng nhập.

**Endpoint**

```http
GET /api/page-tasks/assistant
```

**Authorization:** `Assistant`

**Request:** không có path parameter, query parameter hoặc request body.

**Success response: `200 OK`**

Response sử dụng cùng cấu trúc `PageTaskResponse` như
[`GET /api/page-tasks/mangaka`](#66-lấy-danh-sách-task-của-mangaka), nhưng chỉ
bao gồm task có `assistantId` là user hiện tại.

```json
{
  "data": [],
  "message": "Success"
}
```

**Error cases**

| Status | Trường hợp |
|---|---|
| `401` | Thiếu hoặc token không hợp lệ |
| `403` | User không có role Assistant |

### 6.8. Upload file sản phẩm

Upload file trước khi Assistant submit task. Client lấy `fileAssetId` trong
response để truyền vào API submit.

**Endpoint**

```http
POST /api/files
Content-Type: multipart/form-data
```

**Authorization:** bất kỳ user đã đăng nhập; trong workflow này user gọi là
`Assistant`.

**Form data**

| Field | Type | Required | Giá trị |
|---|---|---|---|
| `category` | enum/string | Có | `TaskSubmission` |
| `files` | file array | Có | Một hoặc nhiều file |

Các định dạng được chấp nhận cho `TaskSubmission`:

- Image: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- Document: `.pdf`
- Archive: `.zip`

Giới hạn:

- Tối đa 20 file trong một request.
- Tổng dung lượng request tối đa 100 MB.
- Mỗi file thuộc category `TaskSubmission` tối đa 50 MB.

**cURL example**

```bash
curl -X POST "https://<host>/api/files" \
  -H "Authorization: Bearer <access-token>" \
  -F "category=TaskSubmission" \
  -F "files=@chapter-1-pages-1-5.zip"
```

**Success response: `200 OK`**

```json
{
  "data": {
    "category": "TaskSubmission",
    "files": [
      {
        "fileAssetId": "66666666-6666-6666-6666-666666666666",
        "bucketName": "manga-files",
        "objectPath": "task-submissions/example.zip",
        "originalFileName": "chapter-1-pages-1-5.zip",
        "storedFileName": "example.zip",
        "extension": ".zip",
        "fileSizeBytes": 1048576,
        "mimeType": "application/zip",
        "publicUrl": "https://<storage-host>/task-submissions/example.zip"
      }
    ]
  },
  "message": "Files uploaded."
}
```

**Error cases**

| Status | Trường hợp |
|---|---|
| `400` | Không có file, quá số lượng, quá dung lượng hoặc sai định dạng |
| `401` | Thiếu hoặc token không hợp lệ |

### 6.9. Assistant submit kết quả

Tạo một submission mới cho task. `versionNo` được server tự động xác định.

**Endpoint**

```http
POST /api/page-tasks/{pageTaskId}/submissions
```

**Authorization:** `Assistant`

**Path parameter**

| Name | Type | Required | Mô tả |
|---|---|---|---|
| `pageTaskId` | UUID | Có | ID của task cần submit |

**Request body**

```json
{
  "submittedFileAssetId": "66666666-6666-6666-6666-666666666666",
  "note": "Đã hoàn thành line art trang 1 đến trang 5."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `submittedFileAssetId` | UUID | Có | File asset phải tồn tại và chưa bị xóa |
| `note` | string | Không | Tối đa 1000 ký tự |

**Success response: `200 OK`**

Response trả về toàn bộ `PageTaskResponse`, bao gồm submission vừa tạo.

```json
{
  "data": {
    "pageTaskId": "33333333-3333-3333-3333-333333333333",
    "status": 2,
    "submissions": [
      {
        "submissionId": "55555555-5555-5555-5555-555555555555",
        "pageTaskId": "33333333-3333-3333-3333-333333333333",
        "versionNo": 1,
        "submittedFileAssetId": "66666666-6666-6666-6666-666666666666",
        "status": 0,
        "note": "Đã hoàn thành line art trang 1 đến trang 5.",
        "rejectReason": null,
        "submittedAt": "2026-06-13T08:00:00Z",
        "reviewedAt": null
      }
    ]
  },
  "message": "Task submitted successfully."
}
```

Các field còn lại của `PageTaskResponse` vẫn được trả về nhưng được lược bớt
trong ví dụ.

**State transition**

```text
PageTask: Assigned hoặc InProgress -> Completed
Submission: tạo mới với Submitted
```

**Error cases**

| Status | Trường hợp |
|---|---|
| `401` | Assistant không phải người được giao task |
| `403` | User không có role Assistant |
| `404` | Không tìm thấy task hoặc file asset |
| `409` | Task đã Approved hoặc đã có submission chờ review |

### 6.10. Mangaka approve submission

Approve sản phẩm do Assistant submit.

**Endpoint**

```http
POST /api/page-tasks/submissions/{submissionId}/approve
```

**Authorization:** `Mangaka`

**Path parameter**

| Name | Type | Required | Mô tả |
|---|---|---|---|
| `submissionId` | UUID | Có | ID của submission cần approve |

**Request body:** không có.

**Success response: `200 OK`**

```json
{
  "data": {
    "pageTaskId": "33333333-3333-3333-3333-333333333333",
    "status": 3,
    "approvedAt": "2026-06-14T08:00:00Z",
    "submissions": [
      {
        "submissionId": "55555555-5555-5555-5555-555555555555",
        "status": 1,
        "rejectReason": null,
        "reviewedAt": "2026-06-14T08:00:00Z"
      }
    ]
  },
  "message": "Submission approved successfully."
}
```

Các field còn lại của task và submission vẫn được trả về nhưng được lược bớt
trong ví dụ.

**State transition**

```text
PageTask: Completed -> Approved
Submission: Submitted -> Approved
```

**Error cases**

| Status | Trường hợp |
|---|---|
| `401` | Submission không thuộc series của Mangaka |
| `403` | User không có role Mangaka |
| `404` | Không tìm thấy task hoặc submission |
| `409` | Submission không còn ở trạng thái Submitted |

### 6.11. Mangaka reject submission

Reject sản phẩm và trả task về trạng thái đang thực hiện để Assistant chỉnh sửa.

**Endpoint**

```http
POST /api/page-tasks/submissions/{submissionId}/reject
```

**Authorization:** `Mangaka`

**Path parameter**

| Name | Type | Required | Mô tả |
|---|---|---|---|
| `submissionId` | UUID | Có | ID của submission cần reject |

**Request body**

```json
{
  "rejectReason": "Line art ở trang 3 chưa đúng với storyboard."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `rejectReason` | string | Có | Không được rỗng, tối đa 1000 ký tự |

**Success response: `200 OK`**

```json
{
  "data": {
    "pageTaskId": "33333333-3333-3333-3333-333333333333",
    "status": 1,
    "approvedAt": null,
    "submissions": [
      {
        "submissionId": "55555555-5555-5555-5555-555555555555",
        "status": 2,
        "rejectReason": "Line art ở trang 3 chưa đúng với storyboard.",
        "reviewedAt": "2026-06-14T08:00:00Z"
      }
    ]
  },
  "message": "Submission rejected successfully."
}
```

Các field còn lại của task và submission vẫn được trả về nhưng được lược bớt
trong ví dụ.

**State transition**

```text
PageTask: Completed -> InProgress
Submission: Submitted -> Rejected
```

Sau đó Assistant có thể upload file mới và gọi lại API submit. Submission mới có
`versionNo` lớn hơn version trước.

**Error cases**

| Status | Trường hợp |
|---|---|
| `400` | `rejectReason` rỗng hoặc dài quá 1000 ký tự |
| `401` | Submission không thuộc series của Mangaka |
| `403` | User không có role Mangaka |
| `404` | Không tìm thấy task hoặc submission |
| `409` | Submission không còn ở trạng thái Submitted |

### 6.12. Page Task data models

#### PageTaskStatus

| JSON value | Enum | Mô tả |
|---|---|---|
| `0` | `Assigned` | Task đã được giao cho Assistant |
| `1` | `InProgress` | Submission trước bị reject, Assistant cần chỉnh sửa |
| `2` | `Completed` | Assistant đã submit và đang chờ Mangaka review |
| `3` | `Approved` | Mangaka đã approve task |

#### PageTaskSubmissionStatus

| JSON value | Enum | Mô tả |
|---|---|---|
| `0` | `Submitted` | Submission đang chờ review |
| `1` | `Approved` | Submission đã được approve |
| `2` | `Rejected` | Submission bị reject |

#### PageTaskResponse

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `pageTaskId` | UUID | Không | ID của task |
| `chapterId` | UUID | Không | Chapter chứa task |
| `manuscriptId` | UUID | Không | Manuscript được dùng khi tạo task |
| `assistantId` | UUID | Không | Assistant được giao |
| `assistantName` | string | Có | Tên hiển thị của Assistant |
| `pageStart` | integer | Không | Trang bắt đầu |
| `pageEnd` | integer | Không | Trang kết thúc |
| `taskType` | string | Không | Loại công việc |
| `description` | string | Có | Mô tả công việc |
| `dueDate` | datetime | Có | Hạn hoàn thành |
| `status` | integer | Không | `PageTaskStatus` |
| `createdAt` | datetime | Không | Thời điểm tạo |
| `approvedAt` | datetime | Có | Thời điểm approve |
| `updatedAt` | datetime | Có | Thời điểm cập nhật gần nhất |
| `submissions` | array | Không | Danh sách submission theo version mới nhất |

#### PageTaskSubmissionResponse

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `submissionId` | UUID | Không | ID của submission |
| `pageTaskId` | UUID | Không | Task chứa submission |
| `versionNo` | integer | Không | Số phiên bản, bắt đầu từ 1 |
| `submittedFileAssetId` | UUID | Không | ID file đã upload |
| `originalFileName` | string | Có | Tên file gốc |
| `objectPath` | string | Có | Đường dẫn object trong storage |
| `status` | integer | Không | `PageTaskSubmissionStatus` |
| `note` | string | Có | Ghi chú của Assistant |
| `rejectReason` | string | Có | Lý do reject của Mangaka |
| `submittedAt` | datetime | Có | Thời điểm submit |
| `reviewedAt` | datetime | Có | Thời điểm review |

### 6.13. API cũ không dùng cho workflow này

Các endpoint dưới đây tồn tại trong `PageTaskSubmissionController` nhưng không
nên được client sử dụng cho Page Task workflow:

| Method | Endpoint |
|---|---|
| `GET` | `/api/tasks/{pageTaskId}/submissions` |
| `GET` | `/api/submissions/{id}` |
| `POST` | `/api/submissions` |
| `PUT` | `/api/submissions/{id}` |

Lý do:

- Service không kiểm tra Assistant có phải người được giao task hay không.
- Service không kiểm tra Mangaka có sở hữu series hay không.
- Create/update submission không đồng bộ trạng thái của `PageTask`.
- Client có thể truyền trực tiếp `versionNo` và trạng thái submission.

Những endpoint này cần được deprecated hoặc loại bỏ sau khi xác nhận không còn
consumer phụ thuộc.

## 7. Review and Board Decision APIs

Chưa được mô tả.

## 8. Notification APIs

Chưa được mô tả.

## 9. File APIs

Hiện tại API upload dùng trong Page Task workflow được mô tả tại
[`POST /api/files`](#68-upload-file-sản-phẩm). Các File API khác sẽ được bổ sung
sau.

## 10. Common Data Models

Sẽ được bổ sung khi tài liệu hóa các nhóm API tiếp theo.

## 11. Deprecated APIs

Danh sách endpoint cũ hoặc không còn được khuyến nghị sử dụng sẽ được tập hợp tại
đây. Hiện tại xem thêm
[API cũ không dùng cho Page Task workflow](#613-api-cũ-không-dùng-cho-workflow-này).

