using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Web.Modules.Catalog;

public class CategoryRequest
{
    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [RegularExpression("^[0-9a-fA-F]{24}$")]
    public string? ParentId { get; set; }
}
