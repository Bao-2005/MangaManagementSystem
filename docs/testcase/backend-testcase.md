# BACKEND TESTCASE - MangaHub

> File nay chi dung de test Backend/API hien tai. File testcase day du van giu nguyen tai
> `docs/testcase/testcase-day-du.md` de lam checklist tong the cho FE + BE.

Quy uoc:

| Trang thai | Y nghia |
|---|---|
| `PASS` | Backend/API dung expected: status code, response, database/state change dung. |
| `FAIL` | Backend/API sai expected hoac con thieu logic so voi target behavior. |
| `BLOCKED` | Chua test runtime duoc vi thieu data, token, database, Supabase bucket, env, hoac service phu. |
| `N/A-FE` | Thuoc UI/FE, khong tinh vao backend testcase. |
| `N/A-INFRA` | Thuoc ha tang/runtime nhu Supabase CORS, SignalR connection, network. |
| `PASS-BY-CODE` | Da doi chieu code backend va thay khop, chua goi API runtime. |
| `FAIL-BY-CODE` | Da doi chieu code backend va thay lech expected, chua can runtime de xac nhan. |
| `BLOCKED-RUNTIME` | Can goi API runtime voi database/token/seed that de ket luan. |
| `BLOCKED-ENV` | API runtime bi chan boi env/permission/database/Supabase, khong phai ket luan nghiep vu. |
| `REVIEW` | Code co hanh vi can team thong nhat lai expected truoc khi ket luan PASS/FAIL. |

Moi case nen ghi theo 2 lop:

- `Target Expected`: ky vong nghiep vu/san pham cuoi.
- `Current Backend Behavior`: backend hien tai dang lam gi.

## Verification run

Lan kiem tra gan nhat:

| Hang muc | Lenh/cach kiem | Ket qua |
|---|---|---|
| Build solution | `dotnet build MangaManagementSystem.sln` | PASS, 0 warning, 0 error. |
| Unit/integration test project | `dotnet test MangaManagementSystem.sln --no-build` | Khong co test project/output dang ke de xac nhan luong nghiep vu. |
| API runtime startup | `dotnet run --project MangaManagementSystem.WebApi --no-build --launch-profile http` | API bat dau listen `http://localhost:5151`, nhung runtime bi chan boi DataProtection/EventLog permission va ket noi database Supabase/Postgres. |
| Runtime endpoint execution | Goi tung endpoint bang token/seed that | BLOCKED-ENV: chua co moi truong DB/token/seed on dinh trong sandbox. |
| Static code-path verification | Doc controller/service/DTO/policy | Da cap nhat cac dong `PASS-BY-CODE`, `FAIL-BY-CODE`, `REVIEW`, `N/A-*` ben duoi. |

---

## NHOM 1 - CHAPTER

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 1.1 | Happy path | `POST /api/chapters` | Series thuoc Mangaka, status `Active` hoac `Approved` | Tao chapter voi publication date hop le | Tao thanh cong, deadline = publication date - 14 ngay | Code cho phep series `Approved` hoac `Active`, auto tinh deadline neu khong gui | PASS-BY-CODE |
| 1.2 | Negative | `POST /api/chapters` | Mangaka owner | Gui publication date hom nay hoac qua khu | Bao loi publication date phai nam trong tuong lai | Code chi chan ngay nho hon hom nay; ngay hom nay co the fail gian tiep do deadline auto < now+3 | FAIL-BY-CODE |
| 1.3 | Negative | `POST /api/chapters` | Mangaka owner | Publication date chi cach 10 ngay | Bao loi deadline khong dat toi thieu | Code auto deadline = publication - 14, sau do chan deadline < now+3 | PASS-BY-CODE |
| 1.4 | Negative | `POST /api/chapters` | Mangaka owner | Khong gui `referenceFileAssetIds` | Target yeu cau toi thieu 1 file ban thao/reference | `ReferenceFileAssetIds` optional, backend van cho tao | FAIL-BY-CODE |
| 1.5 | Negative | `POST /api/chapters` | Khong phai owner hoac series khong hop le | Tao chapter | 401/403 voi non-owner; khong tao cho series chua san sang | Code chan non-owner; chi cho status `Approved` hoac `Active` | PASS-BY-CODE |
| 1.6 | Edge | `POST /api/chapters` | Da co chapterNo trong series | Tao trung chapterNo | 409/loi nghiep vu, khong tao trung | Code check duplicate `ChapterNo` theo series | PASS-BY-CODE |
| 1.7 | Happy path | `GET /api/series/{seriesId}/chapters` | Co chapter 1,2,3 | Lay danh sach | Sort moi nhat truoc hoac 3 -> 2 -> 1 | Code dang `OrderBy(c => c.ChapterNo)`, tuc 1 -> 2 -> 3 | FAIL-BY-CODE |
| 1.8 | Happy path | `PUT /api/chapters/{id}` | Chapter chua bi xoa | Sua title/deadline | Cap nhat thanh cong | Code cho sua title, totalPages, publicationDate, submissionDeadline; khong cho sua status truc tiep | PASS-BY-CODE |

---

## NHOM 2 - PAGE TASK

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 2.1 | Happy path | `POST /api/page-tasks` | Chapter thuoc series cua Mangaka, Assistant active | Giao task trang hop le, co rate/file reference tuy chon | Tao task `Assigned` | Code validate owner, assistant role, page range, overlap, optional refs | PASS-BY-CODE |
| 2.2 | Edge | `POST /api/page-tasks` | Chapter `Draft` | Giao task | Van tao duoc | PageTask service khong chan chapter Draft | PASS-BY-CODE |
| 2.3 | Negative | `POST /api/page-tasks` | Da co active task trang 1-10 | Tao task trang 5-15 | Loi overlap | Code chan overlap voi task chua `Approved` | PASS-BY-CODE |
| 2.4 | Happy path | `POST /api/page-tasks` | Task cu trang 1-10 da `Approved` | Tao task moi cung range | Tao duoc | Overlap query bo qua task `Approved` | PASS-BY-CODE |
| 2.5 | Edge | `POST /api/page-tasks` | Request hop le | De trong `taskType` | Tao duoc | Code trim va set null neu whitespace | PASS-BY-CODE |
| 2.6 | FE-only | N/A | Input UI rate | Backspace xoa het | UI cho de trong | Backend khong test duoc hanh vi input | N/A-FE |
| 2.7 | FE-only | N/A | Input UI page range | Backspace xoa het | UI cho de trong | Backend khong test duoc hanh vi input | N/A-FE |
| 2.8 | Negative | `POST /api/page-tasks` | Request co pageStart > pageEnd | Tao task | Bao loi | Code throw `PageStart must be less than or equal to PageEnd.` | PASS-BY-CODE |
| 2.9 | Negative | `POST /api/page-tasks` | Thieu/invalid assistant | Tao task | Bao loi assistant | Model binding/Guid va service check assistant not found/role | PASS-BY-CODE |
| 2.10 | Negative | `PUT /api/page-tasks/{id}` | Task da co submission | Sua detail, khong doi assistant | 409/loi khong cho sua detail | Code chan detail change sau khi co submission | PASS-BY-CODE |

---

## NHOM 3 - ASSISTANT SUBMISSION

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 3.1 | Happy path | `GET /api/page-tasks/assistant` | Assistant co task duoc giao | Lay danh sach | Thay task cua minh | Code filter `AssistantId == current user` | PASS-BY-CODE |
| 3.2 | Negative | Mangaka-only API | User role Assistant | Goi API sai role | 403 | Controller dung policy role, `AssistantOnly`/`MangakaOnly` duoc khai bao trong `Program.cs` | PASS-BY-CODE |
| 3.3 | FE-only | N/A | UI dashboard | Bam "Bat dau ve" | Doi UI state | Backend khong co endpoint rieng cho start task | N/A-FE |
| 3.4 | Happy path | `POST /api/page-tasks/{id}/submissions` | Assistant owner, task chua approved, co fileAsset | Submit 1 file asset | Tao submission v1, task `Completed` | Code tao submission, version +1, task `Completed` | PASS-BY-CODE |
| 3.5 | FE-only | `POST /api/files` + zip UI | UI chon nhieu file va zip | Submit file zip | BE chi nhan file upload/submittedFileAssetId | Viec gom thanh zip thuoc FE | N/A-FE |
| 3.6 | FE-only | N/A | UI danh sach file | Xoa 1 file truoc submit | UI cap nhat list | Backend khong test duoc | N/A-FE |
| 3.7 | Edge | `POST /api/page-tasks/{id}/submissions` | Da co 3 active attempts chua approved | Submit lan 4 | Bi chan het luot | Code gioi han `MaxActiveSubmissionAttempts = 3` | PASS-BY-CODE |
| 3.8 | Negative | `POST /api/page-tasks/{id}/submissions` | Khong co submittedFileAssetId hop le | Submit | Bao loi | Guid/model binding hoac file asset not found | PASS-BY-CODE |

---

## NHOM 4 - REVIEW, REJECT, ANNOTATION

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 4.1 | Happy path | `GET /api/page-tasks/mangaka` | Task co submission | Lay task de review | Response co submission/file info | Code map submitted file asset/url neu co Supabase URL | PASS-BY-CODE |
| 4.2 | FE-only | N/A | Overlay review | Mo overlay/giai nen/hien trang | UI hien anh | Backend khong xu ly overlay/giai nen UI | N/A-FE |
| 4.3 | FE-only | N/A | Zip nhieu trang | Chuyen trang | UI state dung | Backend khong test duoc | N/A-FE |
| 4.4 | Backend + FE | `POST /api/submissions/{id}/annotations` | User co quyen annotate submission | Tao pin voi pageNo, x/y, content | Luu annotation dung vi tri | Backend validate x/y 0..1, content required, pageNo trong task range | PASS-BY-CODE |
| 4.5 | Backend + FE | `POST /api/submissions/{id}/annotations` | Co pin content | Luu annotation | Response tra content/position | Code map content/position/author | PASS-BY-CODE |
| 4.6 | Happy path | `POST /api/page-tasks/submissions/{id}/reject` + annotation API | Submission `Submitted`, Mangaka owner | Reject co feedback va co annotation | Luu feedback + annotation | Reject va annotation la 2 API rieng; backend ho tro ca hai | PASS-BY-CODE |
| 4.7 | Negative | `POST /api/page-tasks/submissions/{id}/reject` | Khong feedback | Reject | Bao loi feedback required | Code require feedback non-empty | PASS-BY-CODE |
| 4.8 | Happy path | `GET /api/submissions/{id}/annotations` | Task bi reject co pin | Assistant xem annotation | Tra annotations neu Assistant la owner task | Code cho Assistant annotate/view submission cua task minh | PASS-BY-CODE |
| 4.9 | FE-only | N/A | Hover pin | Hien tooltip | UI behavior | Backend chi tra content/position | N/A-FE |
| 4.10 | Edge | `GET /api/submissions/{id}/annotations` | Submission khong co pin | Lay annotations | Tra mang rong | Query filter annotation, khong co thi empty list | PASS-BY-CODE |

---

## NHOM 5 - VERSION COMPARE

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 5.1 | FE-only | N/A | Co v1/v2 | So sanh file/anh | Tra diff/% khac | Backend khong co compare API | N/A-FE |
| 5.2 | FE-only | N/A | Diff < 5% | Hien mau xanh | UI behavior | Backend khong tra classification | N/A-FE |
| 5.3 | FE-only | N/A | Diff 5-20% | Hien mau vang | UI behavior | Backend khong tra classification | N/A-FE |
| 5.4 | FE-only | N/A | Diff > 20% | Hien mau do | UI behavior | Backend khong tra classification | N/A-FE |
| 5.5 | FE-only | N/A | Chi co 1 submission | An nut compare | UI behavior | Backend chi tra submissions | N/A-FE |
| 5.6 | FE-only | N/A | File format khac nhau | Bao loi compare | UI/client compare | Backend khong compare | N/A-FE |
| 5.7 | FE-only | N/A | Zip so trang khac nhau | Danh dau added/removed | UI/client compare | Backend khong compare | N/A-FE |
| 5.8 | Infra | Supabase Storage/CORS | Tai file truc tiep | Bao loi ha tang ro rang | CORS/storage la infra/FE | Khong tinh BE fail neu metadata/url dung | N/A-INFRA |

---

## NHOM 6 - REASSIGN PAGE TASK

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 6.1 | Happy path | `PUT /api/page-tasks/{id}` | Task bi reject/het luot, khong co Submitted pending | Doi assistantId | Reassign thanh cong, old submissions soft-delete | Code soft-delete active submissions, reset status `Assigned` | PASS-BY-CODE |
| 6.2 | Happy path | `GET /api/page-tasks/assistant` | Vua reassign | Assistant moi lay task | Thay task, attempts active reset | Old submissions DeletedAt nen response moi khong tinh attempts | PASS-BY-CODE |
| 6.3 | Happy path | `GET /api/page-tasks/assistant` | Vua reassign | Assistant cu lay task | Khong con thay task | Filter theo AssistantId moi | PASS-BY-CODE |
| 6.4 | Negative | `PUT /api/page-tasks/{id}` | Co submission `Submitted` | Doi assistantId | Bi chan | Code throw cannot reassign while waiting review | PASS-BY-CODE |
| 6.5 | Happy path | `PUT /api/page-tasks/{id}` | Task chua co submission | Sua detail khong doi assistant | Cap nhat duoc | Code cho update detail neu chua co submissions | PASS-BY-CODE |

---

## NHOM 7 - APPROVE VA SALARY

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 7.1 | Happy path | `POST /api/page-tasks/submissions/{id}/approve` | Submission `Submitted`, Mangaka owner | Approve | Task `Approved`, submission `Approved` | Code set status va approvedAt | PASS-BY-CODE |
| 7.2 | Happy path | `GET /api/salary-records` | Vua approve task co rate | Mangaka xem salary | Amount = pages * rate | Code tao `SalaryRecord` snapshot khi approve | PASS-BY-CODE |
| 7.3 | Happy path | `GET /api/salary-records` | Assistant co salary | Assistant xem cua minh | Chi thay salary cua minh | Salary service filter theo role Assistant | PASS-BY-CODE |
| 7.4 | Edge | Approve task rate null/0 | Xem salary | Amount = 0 | Code `rateAtApproval = RatePerPage ?? 0` | PASS-BY-CODE |
| 7.5 | Happy path | Approve task sau khi sua rate truoc approval | Approve | Snapshot rate moi tai luc approve | Code lay `task.RatePerPage` luc approve | PASS-BY-CODE |
| 7.6 | Edge | Salary da tao | Doi task/rate sau do | Salary cu khong doi | Salary record snapshot khong auto update | PASS-BY-CODE |
| 7.7 | FE-only | N/A | Dashboard Assistant | Khong hien bang salary o tab chinh | UI layout | Backend khong test duoc | N/A-FE |

---

## NHOM 8 - MANUSCRIPT

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 8.1 | Backend + FE | `POST /api/manuscripts` | Tat ca PageTask approved | Submit manuscript | Tao manuscript | Code chi cho submit khi khong co unapproved task | PASS-BY-CODE |
| 8.2 | Backend + FE | `POST /api/manuscripts` | Con task chua approved | Submit manuscript | Bi chan | Code check `hasUnapproved` | PASS-BY-CODE |
| 8.3 | Happy path | `POST /api/manuscripts` | Chapter 100% approved | Gui manuscript v1 | Status `Submitted` | Code set manuscript status `Submitted`, chapter status `Submitted` | PASS-BY-CODE |
| 8.4 | FE-only | N/A | Vua gui manuscript | An nut UI | UI behavior | Backend khong test duoc | N/A-FE |
| 8.5 | Happy path | `POST /api/manuscripts` | Da co v1 | Gui v2 | Tao version 2 | Code `lastVersion + 1` | PASS-BY-CODE |
| 8.6 | Happy path | `GET /api/chapters/{chapterId}/manuscripts` | Co nhieu manuscript | Lay danh sach | Sort moi -> cu | Code chua sort, chi select list | FAIL-BY-CODE |

---

## NHOM 9 - NOTIFICATION / SIGNALR

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 9.1 | Infra | SignalR hub | Start API + FE connect | Kiem tra connected | Hub connect thanh cong | Can runtime/browser verify | N/A-INFRA |
| 9.2 | Infra | SignalR client | Ket noi fail lan dau | Auto reconnect | Client reconnect | Thuoc FE/runtime | N/A-INFRA |
| 9.3 | Backend gap | Approve/reject task | Mangaka approve/reject | Assistant nhan notification/realtime | Backend dispatch notification cho Assistant | `PageTaskService` approve/reject chua goi notification dispatch/realtime | FAIL-BY-CODE |

---

## NHOM 10 - RANKING

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 10.1 | Negative | `POST /api/vote-records` | readerCount=1000, voteCount=1500 | Tao vote record | Chan voteCount > readerCount | Service dang gan truc tiep ReaderCount/VoteCount, chua validate | FAIL-BY-CODE |
| 10.2 | Happy path | Confirm vote record | Vote record pending | Confirm | Tao/cap nhat ranking score = vote/reader*100 | Confirm chi set status `Confirmed`, chua tinh ranking | FAIL-BY-CODE |
| 10.3 | Happy path | `GET /api/rankings` | Co nhieu series | Lay ranking | Sort giam dan theo score | Code sort `RankNo`, response khong co score | FAIL-BY-CODE |
| 10.4 | FE-only | N/A | Top 3 | Hien mau TOP | UI behavior | Backend khong tra style | N/A-FE |
| 10.5 | Backend | `GET /api/rankings` | Snapshot co `IsBottom20Percent` | Lay ranking | Tra flag bottom 20% | Response co `IsBottom20Percent`, nhung auto-calc bottom 20% chua co bang chung trong service | REVIEW |
| 10.6 | Happy path | `GET /api/rankings?period=` | Co vote/snapshot khac period | Lay period hien tai | Chi tra dung period | Ranking service filter period neu co | PASS-BY-CODE |

---

## NHOM 11 - FILE WHITELIST

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 11.1 | Negative | `POST /api/files` | File `.txt` | Upload | Bi chan | Rules khong allow `.txt`; validate extension/MIME/signature | PASS-BY-CODE |
| 11.2 | Happy path | `POST /api/files` | `.png`, `.jpg`, `.zip` voi MIME/signature dung | Upload | Thanh cong | Code allow cac file nay o category phu hop | PASS-BY-CODE |
| 11.3 | Note | `POST /api/files` | Category `TaskSubmission`/reference | Upload `.pdf/.rar/.psd/.clip/.ai` | Can thong nhat whitelist target | Code hien allow them cac extension nay tuy category | REVIEW |

---

## NHOM 12 - EDGE CASE CHUNG

| # | Loai | API/Service | Dieu kien | Thao tac backend | Target Expected | Current Backend Behavior | Trang thai |
|---|---|---|---|---|---|---|---|
| 12.1 | Backend + FE | Nhieu API doc lap | Mot API phu loi | API khac van hoat dong | Backend endpoint doc lap | Can runtime/integration verify voi API server + DB that | BLOCKED-RUNTIME |
| 12.2 | Negative | Bat ky protected API | Token thieu/het han | Goi API | 401 | Controllers co `[Authorize]`, JWT auth/policies duoc cau hinh trong `Program.cs` | PASS-BY-CODE |
| 12.3 | FE/Infra | N/A | Mat network giua chung | Toast loi | UI/network behavior | Backend khong nhan request | N/A-FE |
| 12.4 | Infra | Supabase Storage | Storage down/CORS loi | Xem/tai file | Bao loi ha tang | Khong tinh BE fail neu BE luu metadata/url dung | N/A-INFRA |

---

## BANG TONG HOP TAM THOI

> Tong hop nay dua tren static code check + `dotnet build MangaManagementSystem.sln`.
> Chua thay the cho API runtime test voi database seed that.

| Nhom | Tong case | PASS/PASS-BY-CODE | FAIL/FAIL-BY-CODE | BLOCKED | N/A-FE | N/A-INFRA | REVIEW | Ghi chu |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| 1. Chapter | 8 | 5 | 3 | 0 | 0 | 0 | 0 | Lech publication-date expected, required file, sort order. |
| 2. Page Task | 10 | 8 | 0 | 0 | 2 | 0 | 0 | Backend PageTask khop phan lon testcase. |
| 3. Assistant Submission | 8 | 5 | 0 | 0 | 3 | 0 | 0 | Role policy da doi chieu code; runtime token test chua chay duoc do env. |
| 4. Review/Annotation | 10 | 7 | 0 | 0 | 3 | 0 | 0 | Reject va annotation la 2 API rieng. |
| 5. Version Compare | 8 | 0 | 0 | 0 | 7 | 1 | 0 | Chu yeu FE/client. |
| 6. Reassign | 5 | 5 | 0 | 0 | 0 | 0 | 0 | Khop code. |
| 7. Salary | 7 | 6 | 0 | 0 | 1 | 0 | 0 | Salary snapshot dung code. |
| 8. Manuscript | 6 | 4 | 1 | 0 | 1 | 0 | 0 | Sort manuscript la backend gap. |
| 9. Notification/SignalR | 3 | 0 | 1 | 0 | 0 | 2 | 0 | Approve/reject chua dispatch notification. |
| 10. Ranking | 6 | 1 | 3 | 0 | 1 | 0 | 1 | Ranking con CRUD/snapshot thu cong; bottom flag can team thong nhat cach tao snapshot. |
| 11. File Whitelist | 3 | 2 | 0 | 0 | 0 | 0 | 1 | Whitelist thuc te rong hon testcase goc. |
| 12. Edge chung | 4 | 1 | 0 | 1 | 1 | 1 | 0 | Token auth pass by code; API independence can runtime verify. |
| TONG | 78 | 44 | 8 | 1 | 19 | 4 | 2 | Build pass 0 warning/0 error; API runtime blocked by env/infra. |

---

## GHI LOI / BACKLOG BACKEND HIEN TAI

| Case | Pham vi | Mo ta | Evidence | Trang thai |
|---|---|---|---|---|
| 1.2 | BE | Publication date hom nay khong bi chan truc tiep boi rule future date; fail co the do deadline auto. Can thong nhat expected message/rule. | `ChapterService.CreateAsync` | FAIL-BY-CODE |
| 1.4 | BE | Tao chapter khong bat buoc reference/manuscript file. | `CreateChapterRequest.ReferenceFileAssetIds` optional | FAIL-BY-CODE |
| 1.7 | BE | Chapter list sort tang dan theo `ChapterNo`, khong phai moi nhat/giam dan. | `ChapterService.GetBySeriesAsync` | FAIL-BY-CODE |
| 8.6 | BE | Manuscript list chua sort moi -> cu. | `ManuscriptService.GetByChapterAsync` | FAIL-BY-CODE |
| 9.3 | BE | Approve/reject PageTask submission chua tao notification/realtime cho Assistant. | `PageTaskService.ApproveSubmissionAsync`, `RejectSubmissionAsync` | FAIL-BY-CODE |
| 10.1 | BE | VoteRecord chua validate `voteCount <= readerCount` va gia tri am. | `VoteRecordService.CreateAsync` | FAIL-BY-CODE |
| 10.2 | BE | Confirm vote chua tao/cap nhat ranking snapshot/score. | `VoteRecordService.ConfirmAsync` | FAIL-BY-CODE |
| 10.3 | BE | Ranking response khong co score va sort theo `RankNo`, khong theo calculated score. | `RankingSnapshotService.GetAllByPeriodAsync` | FAIL-BY-CODE |

---

## RUNTIME BLOCKERS

| Hang muc | Ket qua thuc te | Anh huong |
|---|---|---|
| API startup | `dotnet run --project MangaManagementSystem.WebApi --no-build --launch-profile http` da listen `http://localhost:5151`. | Co the xac nhan app khoi dong, nhung chua du dieu kien goi full API flow on dinh. |
| DataProtection key ring | Runtime bao `Access to the path ... DataProtection-Keys is denied`. | Anh huong moi truong sandbox/permission; can chay o user context co quyen hoac cau hinh key path khac. |
| Windows EventLog logger | Runtime bao `Cannot open log for source '.NET Runtime'`. | Anh huong permission ghi log trong sandbox. |
| Database connection | Runtime bao loi ket noi database `aws-1-ap-southeast-1.pooler.supabase.com:5432`. | Can connection string/DB/network/seed on dinh de chay API execution. |
| Auth/token/seed data | Chua co token va seed data role Mangaka/Assistant/Tantou/Board/Admin trong sandbox. | Cac case runtime theo role va object ownership tam thoi chi verified by code. |
