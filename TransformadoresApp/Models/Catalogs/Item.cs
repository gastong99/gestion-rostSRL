using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransformadoresApp.Models;

namespace TransformadoresApp.Models.Catalogs
{
    public class Item
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Display(Name = "Tipo")]
        public ItemType ItemType { get; internal set; }

        [NotMapped]
        public string ItemTypeName => Category?.ItemType?.ToString() ?? "-";

        [Display(Name = "Unidad de medida")]
        public int? UnitOfMeasureId { get; set; }

        public UnitOfMeasure? UnitOfMeasure { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Stock mínimo")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumStock { get; set; }

        public ICollection<ItemAttribute> Attributes { get; set; }
            = new List<ItemAttribute>();

        public bool IsActive { get; set; } = true;
    }
}