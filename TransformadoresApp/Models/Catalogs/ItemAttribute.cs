using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Catalogs;

public class ItemAttribute
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public Item? Item { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}