using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Catalogs;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; }
        = new List<Category>();

    public bool IsActive { get; set; } = true;
}