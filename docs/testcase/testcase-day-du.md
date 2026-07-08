# 🧪 TESTCASE ĐẦY ĐỦ — MangaHub (bản cuối cùng)

> Test case chuẩn: mỗi case có **Điều kiện đầu vào → Thao tác → Kết quả mong đợi**.
> Dùng song song với `checklist-test-cuoi-cung.md` (checklist nhanh) — file này chi tiết hơn, dùng khi cần test kỹ hoặc viết báo cáo test.
> Ký hiệu: 🟢 Happy path · 🟡 Edge case · 🔴 Negative case (phải BÁO LỖI đúng)

---

## NHÓM 1 — CHAPTER

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 1.1 | 🟢 | Series đang Active, là chủ sở hữu | Tạo chapter, ngày xuất bản = hôm nay+20 | Tạo thành công, deadline = +6 (20-14), lên đầu danh sách |
| 1.2 | 🔴 | — | Ngày xuất bản = hôm nay hoặc quá khứ | Báo lỗi "phải nằm trong tương lai" |
| 1.3 | 🔴 | — | Ngày xuất bản chỉ cách 10 ngày (< 17 tối thiểu) | Báo lỗi "cách ít nhất 17 ngày" |
| 1.4 | 🟡 | — | Không đính kèm file bản thảo | Tạm thời vẫn tạo được; `ReferenceFileAssetIds` chưa bắt buộc |
| 1.5 | 🔴 | Series KHÔNG phải của mình / không Active | Tạo chapter | Báo lỗi quyền, không tạo được |
| 1.6 | 🟡 | Đã có chapter số 3 | Tạo chapter số 3 lần nữa (trùng) | Báo lỗi 409 "đã tồn tại", không tạo trùng |
| 1.7 | 🟢 | Có 3 chapter (1,2,3) tạo lần lượt | Xem danh sách | Thứ tự: 3 → 2 → 1 (mới nhất đầu) |
| 1.8 | 🟢 | Chapter đang Draft | Sửa tiêu đề/deadline | Cập nhật thành công |

---

## NHÓM 2 — TASK (giao việc)

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 2.1 | 🟢 | Chapter bất kỳ trạng thái | Giao task trang 1-10, đơn giá 50000, đính file | Tạo thành công, task lên đầu list |
| 2.2 | 🟡 | Chapter đang Draft | Giao task | Vẫn tạo được (không lỗi 409 chapter) |
| 2.3 | 🔴 | Đã có task trang 1-10 (chưa Approved) | Giao task trang 5-15 (chồng lấn) | 409 "trùng trang", không tạo |
| 2.4 | 🟢 | Task trang 1-10 đã Approved | Giao task mới trang 1-10 | Tạo được (task Approved không tính overlap) |
| 2.5 | 🟢 | — | Để trống Task Type | Vẫn tạo được |
| 2.6 | 🟡 | Ô "Đơn giá" đang có số | Bấm vào ô, Backspace xóa hết | Ô trống rỗng, KHÔNG tự về số 0/1 |
| 2.7 | 🟡 | Ô "Trang bắt đầu/kết thúc" đang có số | Backspace xóa hết | Ô trống rỗng, gõ số mới bình thường |
| 2.8 | 🔴 | — | Trang bắt đầu > trang kết thúc | Báo lỗi, không cho submit |
| 2.9 | 🔴 | — | Không chọn assistant | Báo lỗi "vui lòng chọn assistant" |
| 2.10 | 🟢 | Task đã có submission | Sửa mô tả/hạn nộp (KHÔNG đổi assistant) | 409 "cannot be updated after it has submissions" (đúng theo thiết kế — không phải bug) |

---

## NHÓM 3 — ASSISTANT NHẬN & NỘP BÀI

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 3.1 | 🟢 | Task vừa giao | Assistant vào /dashboard/assistant | Thấy task, "Lần nộp 0/3" |
| 3.2 | 🔴 | Đăng nhập Assistant | Vào nhầm /dashboard/chapters | 403 khi gọi API Mangaka-only — do vào nhầm trang, phải dùng /dashboard/assistant |
| 3.3 | 🟢 | Task Pending | Bấm "Bắt đầu vẽ" | Chuyển "Đang thực hiện" |
| 3.4 | 🟢 | Đang thực hiện | Chọn 1 file → Nộp | Nộp OK, "Lần nộp 1/3", BE nhận 1 zip |
| 3.5 | 🟢 | Đang thực hiện | Chọn 3 file (chọn 1 → chọn thêm 2) → Nộp | 3 file cộng dồn hiện danh sách, nộp gộp thành 1 zip |
| 3.6 | 🟡 | Đã chọn 3 file | Bấm ✕ xóa 1 file | Còn 2 file đúng, nộp vẫn OK |
| 3.7 | 🟡 | Đã nộp 3 lần, cả 3 bị reject | Bấm nộp lần 4 | Nút "Hết lượt nộp" bị disabled, không nộp được |
| 3.8 | 🔴 | Modal nộp mở | Không chọn file nào, bấm Nộp | Báo lỗi "vui lòng chọn file" |

---

## NHÓM 4 — REVIEW, GHIM COMMENT, REJECT

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 4.1 | 🟢 | Task Submitted | Mangaka bấm "Review Submission" | Thấy card file nén (vì luôn zip) |
| 4.2 | 🟢 | Đang review | Bấm "Mở to góp ý" | Overlay mở, giải nén, hiện trang 1 |
| 4.3 | 🟢 | Zip nhiều trang | Bấm ‹ Trước / Sau › | Chuyển trang đúng, pin của từng trang giữ riêng |
| 4.4 | 🟢 | Overlay mở | Click lên ảnh | Pin đỏ số thứ tự xuất hiện đúng vị trí click |
| 4.5 | 🟢 | Đã có pin | Gõ nội dung góp ý cho pin | Lưu vào state, hiện trong danh sách bên phải |
| 4.6 | 🟢 | Có feedback text + có pin | Bấm "Yêu cầu sửa lại" | Không lỗi 400; BE lưu feedback + lưu từng pin annotation |
| 4.7 | 🔴 | — | Bấm Reject mà KHÔNG nhập feedback VÀ không có pin | Báo lỗi "vui lòng điền phản hồi hoặc góp ý" |
| 4.8 | 🟢 | Task vừa bị reject (có pin) | Assistant mở lại task đó | Thấy: text feedback + ảnh bài nộp + pin đỏ đúng vị trí Mangaka ghim |
| 4.9 | 🟢 | Hover vào pin (phía assistant) | Di chuột vào pin đỏ | Hiện tooltip nội dung góp ý |
| 4.10 | 🟡 | Bài nộp không có pin (chỉ text feedback) | Assistant xem | Chỉ hiện text, không hiện pin (không lỗi) |

---

## NHÓM 5 — SO SÁNH PHIÊN BẢN

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 5.1 | 🟢 | Đã nộp v1, reject, nộp v2 (cả 2 tự động thành zip) | Bấm "So sánh với lần nộp trước" | Ra % khác + ảnh diff (vùng đỏ) |
| 5.2 | 🟢 | % khác < 5% | Xem kết quả | Màu xanh (ít thay đổi) |
| 5.3 | 🟢 | % khác 5-20% | Xem kết quả | Màu vàng |
| 5.4 | 🟢 | % khác > 20% | Xem kết quả | Màu đỏ (thay đổi nhiều) |
| 5.5 | 🟡 | Chỉ có 1 lần nộp (chưa có lần trước) | Mở review | KHÔNG hiện nút so sánh (ẩn) |
| 5.6 | 🔴 | Trường hợp cũ: 1 lần ảnh đơn, 1 lần zip (đã fix bằng luôn-zip) | So sánh | Không còn xảy ra do luôn-zip; nếu gặp lại → báo lỗi rõ "khác định dạng", không crash "central directory" |
| 5.7 | 🟡 | Cả 2 lần đều nhiều trang (zip 3 trang vs zip 4 trang) | So sánh | So theo từng cặp trang cùng thứ tự, trang dư được đánh dấu added/removed |
| 5.8 | 🔴 | Supabase chưa bật CORS cho storage | So sánh / mở to góp ý zip | Báo lỗi tải file / giải nén — báo Bảo cấu hình CORS, không phải bug FE |

---

## NHÓM 6 — GIAO LẠI TASK (đổi assistant)

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 6.1 | 🟢 | Task đã bị reject đủ 3 lần | Mở "Sửa" → chọn assistant khác ở "Giao cho" → Lưu | Không báo lỗi trùng trang; toast "Đã giao lại nhiệm vụ cho trợ lý mới..." |
| 6.2 | 🟢 | Vừa đổi assistant | Assistant MỚI vào xem task | Task hiện "Chờ nhận việc", "Lần nộp 0/3" (đã reset) |
| 6.3 | 🟢 | Vừa đổi assistant | Assistant CŨ vào xem | Task không còn trong danh sách của họ nữa |
| 6.4 | 🔴 | Task đang có submission Submitted (chưa reject/approve) | Cố đổi assistant | BE chặn: "Cannot reassign while a submission is waiting for review" — đúng thiết kế, phải reject hoặc approve trước |
| 6.5 | 🟢 | Sửa task nhưng KHÔNG đổi assistant (chỉ sửa mô tả, task chưa có submission) | Lưu | Cập nhật đầy đủ field (page/desc/rate/date) bình thường |

---

## NHÓM 7 — APPROVE + LƯƠNG

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 7.1 | 🟢 | Task có đơn giá 50000, 10 trang, đã Submitted | Approve | Task "Đã duyệt"; tiến độ chapter tăng |
| 7.2 | 🟢 | Vừa Approve | Xem bảng "Lương phải trả" (Mangaka) | Amount = 50000 × 10 = 500.000đ |
| 7.3 | 🟢 | Vừa Approve | Assistant vào tab "Lịch sử lương" | Thấy dòng mới: pages=10, rate=50000, amount=500.000, có ngày duyệt |
| 7.4 | 🟡 | Task Approve nhưng đơn giá = 0 (chưa nhập) | Xem Lịch sử lương | Có record nhưng amount = 0 (đúng — không phải bug, do quên nhập giá) |
| 7.5 | 🟢 | Sau khi Approve, Mangaka đổi đơn giá task khác (chưa approve) | Approve task đó | Amount tính theo giá MỚI lúc approve (snapshot đúng thời điểm) |
| 7.6 | 🟡 | Đổi đơn giá của task ĐÃ Approve trước đó | Xem lại Lịch sử lương record cũ | Amount CŨ không đổi (vì đã snapshot rateAtApproval) |
| 7.7 | 🟢 | Trang Assistant chính (không phải tab lương) | Vào Dashboard Trợ lý | KHÔNG còn bảng lương ở đây (đã tách qua tab riêng) |

---

## NHÓM 8 — MANUSCRIPT

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 8.1 | 🟢 | Tất cả task Approved (100%) | Nút "Gửi bản thảo" | Hiện nút |
| 8.2 | 🟡 | Chưa đủ 100% task Approved | Xem chapter | Nút "Gửi bản thảo" KHÔNG hiện |
| 8.3 | 🟢 | 100%, gửi manuscript_v1.zip | Gửi | Không lỗi 409, status → "Submitted" |
| 8.4 | 🟢 | Vừa gửi | Xem chapter | Nút "Gửi bản thảo" ẨN đi |
| 8.5 | 🟢 | Đã gửi v1 | Gửi thêm v2.zip | Tạo version 2, không lỗi |
| 8.6 | 🟢 | Có nhiều manuscript | Vào trang Manuscripts | Sort mới→cũ |

---

## NHÓM 9 — NOTIFICATION (SignalR)

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 9.1 | 🟢 | Vào dashboard | F12 Console | "SignalR: đã kết nối realtime" |
| 9.2 | 🟡 | Kết nối lần đầu fail | Console | Có thể thấy "kết nối thất bại" rồi tự retry → "đã kết nối realtime" (bình thường, có auto-reconnect) |
| 9.3 | 🟢 | 2 cửa sổ mở (Mangaka + Assistant) | Mangaka approve/reject task | BE tạo notification cho Assistant; chuông realtime nhảy ngay cần verify SignalR/runtime |

---

## NHÓM 10 — RANKING

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 10.1 | 🔴 | readerCount=1000 | Nhập voteCount=1500 | Chặn "Vote count cannot exceed total readers" (BR-89) |
| 10.2 | 🟢 | readerCount=1000, voteCount=800, đúng kỳ | Confirm vote | Lên bảng, score=80% |
| 10.3 | 🟢 | Nhiều series có điểm khác nhau | Xem bảng | Sort giảm dần theo score |
| 10.4 | 🟢 | Top 3 điểm cao nhất | Xem bảng | Nền màu vàng/bạc/cam + "TOP 1/2/3" |
| 10.5 | 🟢 | Series thuộc bottom 20% | Xem bảng | Có flag cảnh báo |
| 10.6 | 🟡 | Vote thuộc kỳ KHÁC kỳ đang xem | Xem bảng hiện tại | Vote đó KHÔNG hiện (chỉ hiện đúng kỳ) |

---

## NHÓM 11 — WHITELIST FILE

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 11.1 | 🔴 | — | Upload file_sai_loai.txt | BE chặn, báo lỗi loại file |
| 11.2 | 🟢 | — | Upload .png/.jpg/.zip | Upload thành công |

---

## NHÓM 12 — LỖI HỆ THỐNG / EDGE CASE CHUNG

| # | Loại | Điều kiện | Thao tác | Kết quả mong đợi |
|---|---|---|---|---|
| 12.1 | 🟢 | 1 API phụ lỗi (VD getAssistants 403) trong hàm tải nhiều dữ liệu | Vào trang | Các phần dữ liệu KHÁC vẫn load bình thường (không bị kéo sập theo) |
| 12.2 | 🔴 | Token hết hạn | Gọi bất kỳ API nào | 401, yêu cầu đăng nhập lại |
| 12.3 | 🟡 | Mất mạng BE giữa chừng | Thao tác bất kỳ | Toast lỗi rõ ràng, không crash trắng trang |
| 12.4 | 🔴 | Supabase Storage down | Xem ảnh/tải file | Ảnh không hiện / lỗi tải — báo Bảo, không phải bug FE |

---

## 📊 BẢNG TỔNG HỢP KẾT QUẢ (điền khi test xong)

| Nhóm | Tổng case | Pass | Fail | Ghi chú |
|---|---|---|---|---|
| 1. Chapter | 8 | | | |
| 2. Task | 10 | | | |
| 3. Assistant nộp | 8 | | | |
| 4. Review/Ghim/Reject | 10 | | | |
| 5. So sánh | 8 | | | |
| 6. Giao lại task | 5 | | | |
| 7. Lương | 7 | | | |
| 8. Manuscript | 6 | | | |
| 9. SignalR | 3 | | | |
| 10. Ranking | 6 | | | |
| 11. Whitelist | 2 | | | |
| 12. Edge case chung | 4 | | | |
| TỔNG | 77 | | | |

---

## 🐞 GHI LỖI KHI TEST FAIL

| # case | Mô tả lỗi thực tế | Console/Network | Trạng thái |
|---|---|---|---|
| | | | |

---

## KET QUA BACKEND DA DOI CHIEU

> Section nay duoc dien dua tren `docs/testcase/backend-testcase.md`.
> Da chay `dotnet build MangaManagementSystem.sln`: PASS, 2 warning, 0 error.
> Chua the chay full API runtime vi moi truong bi chan boi DataProtection/EventLog permission,
> database connection, token va seed data.

Quy uoc trang thai:

| Trang thai | Y nghia |
|---|---|
| `PASS-BY-CODE` | Da doi chieu controller/service/DTO/policy va build pass. |
| `FAIL-BY-CODE` | Da doi chieu code va thay backend hien tai lech expected. |
| `BLOCKED-RUNTIME` | Can chay API runtime voi DB/token/seed that de ket luan. |
| `N/A-FE` | Case thuoc UI/FE, khong tinh la backend fail. |
| `N/A-INFRA` | Case thuoc ha tang/runtime nhu CORS, SignalR connection, network. |
| `REVIEW` | Can team thong nhat expected truoc khi chot PASS/FAIL. |

### Bang tong hop backend

| Nhom | Tong case | PASS-BY-CODE | FAIL-BY-CODE | BLOCKED | N/A-FE | N/A-INFRA | REVIEW | Ghi chu |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| 1. Chapter | 8 | 8 | 0 | 0 | 0 | 0 | 0 | Publication date, optional reference files, va sort order da khop code hien tai. |
| 2. Task | 10 | 8 | 0 | 0 | 2 | 0 | 0 | Backend PageTask khop phan lon testcase. |
| 3. Assistant nop | 8 | 5 | 0 | 0 | 3 | 0 | 0 | Role policy da doi chieu code; runtime token test chua chay duoc do env. |
| 4. Review/Ghim/Reject | 10 | 7 | 0 | 0 | 3 | 0 | 0 | Reject va annotation la 2 API rieng; backend ho tro ca hai. |
| 5. So sanh | 8 | 0 | 0 | 0 | 7 | 1 | 0 | Chu yeu FE/client; Supabase CORS la infra. |
| 6. Giao lai task | 5 | 5 | 0 | 0 | 0 | 0 | 0 | Reassign logic khop code. |
| 7. Luong | 7 | 6 | 0 | 0 | 1 | 0 | 0 | Salary snapshot dung code. |
| 8. Manuscript | 6 | 5 | 0 | 0 | 1 | 0 | 0 | Sort manuscript moi -> cu da khop code. |
| 9. SignalR | 3 | 1 | 0 | 0 | 0 | 2 | 0 | Approve/reject da dispatch persisted notification; realtime can runtime verify. |
| 10. Ranking | 6 | 1 | 3 | 0 | 1 | 0 | 1 | Ranking con CRUD/snapshot thu cong; bottom flag can thong nhat cach tao snapshot. |
| 11. Whitelist | 3 | 2 | 0 | 0 | 0 | 0 | 1 | Whitelist thuc te rong hon testcase goc. |
| 12. Edge case chung | 4 | 1 | 0 | 1 | 1 | 1 | 0 | Token auth pass by code; API independence can runtime verify. |
| TONG | 78 | 49 | 3 | 1 | 19 | 4 | 2 | Build pass 2 warning/0 error; API runtime blocked by env/infra. |

### Ghi loi backend

| # case | Mo ta loi thuc te | Console/Network | Trang thai |
|---|---|---|---|
| Build | `dotnet build MangaManagementSystem.sln` thanh cong, 2 warning, 0 error. Hai warning hien tai nam o `ProblemDetail.cs`, khong lien quan cac thay doi testcase nay. | CLI build | PASS-BY-CODE |
| Runtime | API start duoc va listen `http://localhost:5151`, nhung full API flow bi chan do DataProtection/EventLog permission, database connection, token va seed data. | `dotnet run --project MangaManagementSystem.WebApi --no-build --launch-profile http` | BLOCKED-RUNTIME |
| 1.2 | Backend da chan publication date bang hom nay hoac qua khu bang rule `Publication date must be in the future.` | `ChapterService.CreateAsync` | PASS-BY-CODE |
| 1.4 | Theo quyet dinh tam thoi, tao chapter khong bat buoc reference/manuscript file; `ReferenceFileAssetIds` van optional. | `CreateChapterRequest.ReferenceFileAssetIds` | PASS-BY-CODE |
| 1.7 | Chapter list da sort giam dan theo `ChapterNo`, fallback `CreatedAt`, khop expected 3 -> 2 -> 1. | `ChapterService.GetBySeriesAsync` | PASS-BY-CODE |
| 8.6 | Manuscript list da sort moi -> cu theo `VersionNo` giam dan, fallback `SubmittedAt`. | `ManuscriptService.GetByChapterAsync` | PASS-BY-CODE |
| 9.3 | Approve/reject PageTask submission da dispatch persisted notification cho Assistant. SignalR/realtime chuong nhay ngay van can runtime verify. | `PageTaskService.ApproveSubmissionAsync`, `RejectSubmissionAsync` | PASS-BY-CODE |
| 10.1 | VoteRecord chua validate `voteCount <= readerCount` va gia tri am. | `VoteRecordService.CreateAsync` | FAIL-BY-CODE |
| 10.2 | Confirm vote chi set status `Confirmed`, chua tao/cap nhat ranking snapshot/score. | `VoteRecordService.ConfirmAsync` | FAIL-BY-CODE |
| 10.3 | Ranking response khong co score va sort theo `RankNo`, khong theo calculated score. | `RankingSnapshotService.GetAllByPeriodAsync` | FAIL-BY-CODE |
| 10.5 | Backend response co `IsBottom20Percent`, nhung auto-calc bottom 20% chua co bang chung trong service. Can team thong nhat expected. | `RankingSnapshotService.GetAllByPeriodAsync` | REVIEW |
| 11.3 | Whitelist thuc te rong hon testcase goc: mot so category cho phep them `.pdf/.rar/.psd/.clip/.ai`. | `FileUploadService` rules | REVIEW |

> Test xong: gửi lại bảng tổng hợp + bảng ghi lỗi (nếu có case fail) để sửa tiếp.
