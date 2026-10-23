# MONGODB_SCHEMA

Giữ schema BSON hiện tại. Validator thật: database/validators.json; models: src/Ecommerce.Web/Models/Domain.cs. Các trường thiếu theo master prompt được ghi trong REPOSITORY_AUDIT; không tuyên bố đã migration.

Xem tài liệu chi tiết hiện có: [DATA_MODEL.md](DATA_MODEL.md).

Danh mục đã chuyển sang ParentId, xem CATEGORY_HIERARCHY.md cho quan hệ cha–con và migration Atlas. Kind còn lại trên sản phẩm/snapshot là discriminator tương thích, không phải trường danh mục.
