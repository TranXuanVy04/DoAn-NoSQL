# Tạo repo chung, nhánh, PR và CI

Gói này chưa tạo repo từ xa hoặc gửi lời mời. Các thao tác dưới đây dành cho chủ nhóm thực hiện khi đã quyết định chủ sở hữu.

## 1. Tạo remote

Trên GitHub đăng nhập tài khoản nhóm/chủ repo → New repository:

- Name: `DoAn-NoSQL`.
- Chọn Private/Public theo quy định môn; private phù hợp khi chưa công bố bài.
- Không khởi tạo README/.gitignore/license vì source đã có.
- Sau tạo, copy đúng HTTPS clone URL, không dùng nguyên URL placeholder bên dưới.

## 2. Đưa bản khởi tạo lên

Tại thư mục `Ecommerce-SV3`:

```powershell
git init -b main
git add .
git commit -m "chore: initialize ecommerce MVC and SV3 handoff"
git remote add origin https://github.com/CHU_REPO/DoAn-NoSQL.git
git push -u origin main
```

GitHub có thể mở trình duyệt xác thực. Không lưu token/mật khẩu vào remote URL hoặc source. Nếu Git yêu cầu identity, cấu hình tên/email tác giả đúng tài khoản của mình; không giả danh người khác.

## 3. Mời đúng người

Settings → Collaborators (hoặc Manage access) → Add people. Nhờ Tín và Tuấn cung cấp **GitHub username thật** rồi mời; tên hiển thị Jira không đủ để xác định tài khoản GitHub. Chưa có username thì để chờ, không đoán email/tài khoản.

Mọi người clone cùng URL, mỗi người dùng DB local riêng. Git lưu script/data mẫu, không lưu Docker volume, mật khẩu, .venv, bin/obj.

## 4. Nhánh chức năng

```powershell
git switch main
git pull --ff-only
git switch -c feature/vy-sv3-integration
```

Tín: `feature/tin-catalog-orders`; Tuấn: `feature/tuan-aggregation-indexes`. Chia ownership theo CONTRACTS. Trước PR đồng bộ main, giải quyết conflict và chạy bộ verify. Không force-push vào main.

Bản đầu cần vào main trước để nhóm có nền. Các sửa đổi tiếp theo của Vỹ làm trên nhánh feature và PR. Mẫu `.github/pull_request_template.md` có sẵn; PR cần mô tả thay đổi, link Jira, log và người review.

## 5. CI và mốc

Workflow `.github/workflows/ci.yml` sẵn sàng cho push/PR: .NET10/Python, Mongo replica set Docker, build/test/E2E, roundtrip và benchmark, lưu evidence. **Có file workflow chưa có nghĩa GitHub Actions đã chạy thành công**; kiểm tra tab Actions sau push thật.

Tạo milestone:

| Mốc | Nội dung | Điều kiện |
|---|---|---|
| v0.1.0 | Schema/contract, CRUD nền, data, giao diện catalog | Nhóm review và chạy được |
| v0.2.0 | Giỏ/đơn/Dashboard tích hợp | Kiểm thử snapshot và số liệu đạt |
| v1.0.0 | Đóng gói bảo vệ | Test, Compass, máy mới, diễn tập và cả nhóm xác nhận |

Nếu tài khoản cho phép, bật rule main cần PR và status CI, yêu cầu review thành viên khác. Không khóa rule khi chưa xác định workflow/check thực tế, tránh nhóm không merge được.

Sau chấp thuận release:

```powershell
git switch main
git pull --ff-only
git tag -a v1.0.0 -m "E-commerce NoSQL demo release"
git push origin v1.0.0
```

Chỉ chạy lệnh tag khi điều kiện release đã được xác nhận. Gắn link PR và bằng chứng test vào đúng ticket; không chuyển Done khi chỉ mới có source chưa review/merge.
