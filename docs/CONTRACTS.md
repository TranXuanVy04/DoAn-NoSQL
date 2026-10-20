# Contract khởi tạo để bàn giao SV1 / SV2

Chưa có core/repo chung khi xây dựng. Đây là **đề xuất chạy được**, không khẳng định nhóm đã đồng ý. Bất kỳ thay đổi tên field, trạng thái hay công thức phải cập nhật cả Model, seed, validator, pipeline, test và tài liệu trong cùng PR.

## Phân lớp và quyền sở hữu

| Thành phần | Bàn giao cho | Trách nhiệm |
|---|---|---|
| `Models/Domain.cs`, `Models/Forms.cs` | SV1 + review cả nhóm | Domain, BSON, ViewModel input và output |
| `ICatalogService`, `MongoCatalogService` | SV1 | CRUD danh mục/sản phẩm, kiểm tra thuộc tính |
| `IOrderService`, `MongoOrderService` | SV1 | Tạo đơn, snapshot, tồn kho, chuyển trạng thái |
| `IDashboardService`, `MongoDashboardService` | SV2 | Báo cáo thật từ Aggregation |
| `Controllers/`, `Views/`, `wwwroot/css/` | SV3 chủ trì tích hợp | Catalog, form Admin, cart session, checkout và báo cáo |
| `tools/`, `tests/`, `.github/`, tài liệu demo | SV3 | Seed, đo, đối chiếu, đóng gói |

Các interface nằm trong `Services/Contracts.cs`. Đăng ký DI ở `Program.cs`; thay implementation tại đây nếu SV1/SV2 phát triển module riêng. Không thay View chỉ để làm khớp field khác mà bỏ qua việc thống nhất contract.

## Quy ước dữ liệu

- Mongo field camelCase; `_id`, `categoryId`, `items.productId` lưu BSON ObjectId. Model C# expose `string` với `[BsonRepresentation(ObjectId)]` để form sử dụng dễ.
- `price`, `unitPrice`, `lineTotal`, `total`: BSON Int64 / C# long, VND nguyên. Quantity/stock là Int32.
- `createdAt`: BSON Date lưu UTC. Lọc ngày và hiển thị UTC+7.
- Ngành: `electronics`, `fashion`, `home`; thuộc tính linh hoạt ở `attributes`.
- Trạng thái: Pending → Completed hoặc Pending → Cancelled. Trạng thái cuối không đảo ngược; cập nhật lặp cùng trạng thái không có tác dụng phụ.
- Xóa sản phẩm = isActive false. Không xóa vật lý sản phẩm đã có liên quan đến đơn.
- Danh mục còn sản phẩm (kể cả ngừng bán) không được xóa/đổi ngành.

## Luồng khách hàng và form

| Method / route | Input / output | Quy tắc |
|---|---|---|
| GET `/Catalog` | q, categoryId, page → ProductPage | Chỉ active, 12 sản phẩm/trang, sort name rồi id |
| GET `/Catalog/Details/{id}` | Product | Inactive/ID sai trả 404 |
| GET `/Cart` | CartView | Tính đơn giá/tổng bằng dữ liệu server |
| POST `/Cart/Add` | productId, quantity | 1–99; kiểm tra active, tồn; tối đa 50 dòng |
| POST `/Cart/Update` | productId, quantity | 0 = bỏ dòng; giá không nhận từ client |
| GET/POST `/Cart/Checkout` | CheckoutForm | Họ tên, điện thoại, địa chỉ, checkoutKey |
| GET `/Orders/Details/{id}` | Order snapshot | Đúng customerToken session hoặc Admin |

Cart session chỉ lưu `productId` và `quantity`. Đơn tạo xong cart được xóa. CheckoutKey do GET checkout sinh trong session; unique index trên hoadon đảm bảo retry không tạo hai đơn. Bản lặp được trả lại đơn cũ cho cùng customerToken. Trong transaction, đọc lại dữ liệu hàng, trừ kho có điều kiện `stock >= quantity`, nhúng snapshot rồi insert đơn. Thất bại sẽ rollback toàn bộ. Không chạy các tác vụ trong cùng transaction bằng Task.WhenAll.

## Quản trị

Mọi action Admin cần cookie đăng nhập; ghi dữ liệu chỉ POST + anti-forgery. Không bind trực tiếp Order/Product domain từ form.

| Route | Input chính |
|---|---|
| `/Admin/Products` | q/categoryId/page |
| `/Admin/ProductEdit/{id?}` | ProductForm: sku/name/categoryId/price/stock/isActive/attributesJson/originalStock |
| `/Admin/ProductDelete` POST | id; ngừng bán |
| `/Admin/Categories` | Danh sách |
| `/Admin/CategoryEdit/{id?}` | CategoryForm: name/kind |
| `/Admin/CategoryDelete` POST | id; chỉ danh mục rỗng |
| `/Admin/Orders` | page/status, 15 đơn/trang |
| `/Admin/OrderStatus` POST | id/status, chỉ Completed hoặc Cancelled |
| `/Admin/Dashboard` | from/to dạng yyyy-MM-dd, group=day/month |

`OriginalStock` là giá trị tại thời điểm tải form. Update sản phẩm kiểm tra stock DB chưa đổi để tránh form cũ ghi đè tồn kho vừa bị checkout trừ. Nếu khác phải tải lại form. Đây chưa phải cơ chế optimistic concurrency version đầy đủ cho mọi thuộc tính cùng lúc.

Validation JSON theo ngành ở cả service và DB. Razor encode nội dung hiển thị. Login dùng User Secrets/environment, tối thiểu 12 ký tự; chưa có rate limit/khóa tài khoản vì bản local.

## Ranh giới cần SV1/SV2 xác nhận

1. SV1 kiểm tra CRUD cạnh tranh danh mục/sản phẩm: kiểm tra tham chiếu hiện tại thực hiện ở service, chưa có cơ chế khóa quan hệ khi hai Admin sửa/xóa danh mục đồng thời.
2. SV1 xác nhận soft delete đáp ứng tiêu chí xóa; nếu thầy bắt buộc hard delete cần thiết kế giữ tombstone hoặc policy hoàn kho tương ứng.
3. SV2 xác nhận doanh thu theo createdAt của Completed và tie-break. Nếu muốn theo completedAt phải thêm field, seed và thay tất cả pipeline/test.
4. SV2 xác nhận các index với workload thật; tìm tên hiện dùng escaped regex không phân biệt hoa thường, không khẳng định compound index tối ưu regex.
5. Cả nhóm review/merge và xác nhận trên bản chạy chung trước khi đánh dấu Done trên Jira.
