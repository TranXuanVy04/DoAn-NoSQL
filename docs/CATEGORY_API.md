# API danh mục

Code chính: src/Ecommerce.Web/Modules/Catalog/CategoriesController.cs.
CategoryRequest chỉ nhận Name/Kind; Id do server tạo hoặc lấy từ URL. Controller kiểm tra request, gọi ICatalogService rồi trả HTTP response. MongoCatalogService kiểm tra nghiệp vụ rồi gọi CategoryRepository/ProductRepository để truy vấn DB; MVC Admin vẫn dùng cùng service.

| Method | URL | Kết quả |
|---|---|---|
| GET | /api/categories | 200: danh sách |
| GET | /api/categories/{id} | 200: chi tiết; 404 nếu không có |
| POST | /api/categories | 201: danh mục mới + Location |
| PUT | /api/categories/{id} | 200: danh mục đã sửa |
| DELETE | /api/categories/{id} | 204: xóa thành công |
| GET | /api/categories/csrf | Token dùng cho thao tác ghi, yêu cầu Admin |

Body cho POST/PUT:

```json
{"name":"Điện tử","kind":"electronics"}
```

name: bắt buộc, tối đa 80 ký tự; kind: electronics, fashion hoặc home. Sai input/ID trả 400, chưa đăng nhập trả 401, không có role Admin không được ghi. Trùng tên, xóa danh mục còn sản phẩm hoặc đổi ngành danh mục còn sản phẩm trả 409.

## Thử ngay bằng browser

1. Chạy ứng dụng, đăng nhập Admin qua /Account/Login.
2. Mở DevTools Console ngay trên trang ứng dụng (cùng origin).
3. Lấy CSRF token rồi tạo danh mục thử:

```javascript
const csrfResponse = await fetch('/api/categories/csrf');
const csrf = await csrfResponse.json();
const headers = { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf.token };
const result = await fetch('/api/categories', {
  method: 'POST', headers,
  body: JSON.stringify({ name: 'Danh mục thử', kind: 'home' })
});
const category = await result.json();
console.log(result.status, category);
```

Sửa và xóa chính danh mục vừa tạo (thao tác ghi thật):

```javascript
await fetch('/api/categories/' + category.id, {
  method: 'PUT', headers,
  body: JSON.stringify({ name: 'Danh mục thử đã sửa', kind: 'home' })
});
await fetch('/api/categories/' + category.id, { method: 'DELETE', headers });
```

Có thể dùng Postman với cùng cookie đăng nhập, cookie antiforgery và header X-CSRF-TOKEN. Không tắt antiforgery để bỏ qua bước token.
Tests HTTP dùng fake: tests/Ecommerce.UnitTests/CategoryCrudTests.cs. Chưa kiểm chứng CRUD mới trên Atlas thật trong lần sửa này.
