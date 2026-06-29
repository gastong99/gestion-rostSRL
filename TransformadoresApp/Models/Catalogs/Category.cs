using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Catalogs
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Categoría padre")]
        public int? ParentId { get; set; }

        public Category? Parent { get; set; }

        public ICollection<Category> Children { get; set; }
            = new List<Category>();

        public ICollection<Item> Items { get; set; }
            = new List<Item>();

        public bool IsActive { get; set; } = true;
    }
}