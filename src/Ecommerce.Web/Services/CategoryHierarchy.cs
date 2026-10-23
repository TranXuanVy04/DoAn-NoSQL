using Ecommerce.Web.Models;
using MongoDB.Bson;

namespace Ecommerce.Web.Services;

public static class CategoryHierarchy
{
    public static Category GetRoot(string categoryId, IReadOnlyList<Category> categories)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentId = categoryId;
        while (true)
        {
            if (!visited.Add(currentId)) throw new BusinessException("Cây danh mục có vòng lặp.");
            var current = categories.FirstOrDefault(c => string.Equals(c.Id, currentId, StringComparison.OrdinalIgnoreCase))
                ?? throw new BusinessException("Danh mục không tồn tại.");
            if (current.ParentId == null) return current;
            currentId = current.ParentId;
        }
    }

    public static void ValidateParent(string? categoryId, string? parentId, IReadOnlyList<Category> categories)
    {
        if (parentId == null)
            return;

        if (!ObjectId.TryParse(parentId, out var parsedId))
            throw new BusinessException("ID danh mục cha không hợp lệ.");

        string? currentId = parsedId.ToString();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (categoryId != null)
            visited.Add(categoryId);

        // Đi từ cha lên gốc; gặp lại một ID nghĩa là danh mục đang tạo vòng lặp.
        while (currentId != null)
        {
            if (!visited.Add(currentId))
                throw new BusinessException("Không thể chọn chính mình hoặc danh mục con làm danh mục cha.");

            var parent = categories.FirstOrDefault(x => string.Equals(x.Id, currentId, StringComparison.OrdinalIgnoreCase));
            if (parent == null)
                throw new BusinessException("Danh mục cha không tồn tại.");

            currentId = parent.ParentId;
        }
    }
}
