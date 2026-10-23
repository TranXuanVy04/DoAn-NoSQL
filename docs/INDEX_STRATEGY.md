# Chiến lược index

Định nghĩa hiện tại: database/indexes.mongosh.js. Unique name danh mục, SKU sản phẩm, number và checkoutKey đơn hàng.
Compound categoryId/isActive/price phù hợp equality rồi sort price. Catalog MVC đang sort name/id nên chưa được index đó hỗ trợ sort; regex tìm tên cũng chưa được tối ưu. Chỉ thêm index sau khi đo workload thật.
Compound status/createdAt phù hợp equality trạng thái và range ngày cho analytics. Dùng database/explain.mongosh.js để lấy executionStats; chưa có số đo mới chứng minh tăng hiệu năng.

Danh mục có index parentId để kiểm tra con và phục vụ truy vấn cây. Tên vẫn unique toàn hệ thống; schema danh mục không còn Kind.
