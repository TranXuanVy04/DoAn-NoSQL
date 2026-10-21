# Contract v1 (đề xuất)
Nguồn chung: `ecommerce.v1.json`. Mở bằng Swagger Editor hoặc Swagger UI hỗ trợ OpenAPI 3.0.
`x-implementation-status` phân biệt implemented/planned; chưa coi contract là được cả nhóm duyệt.
Không đổi DTO âm thầm. PR breaking change phải cập nhật mock, generated types, test và được cả 3 SV review.
Generate TypeScript: `cd src/storefront && npm run contract:types`.
Giữ tên field theo dữ liệu hiện tại: number/total/items.name/items.attributes; không tự đổi BSON.
