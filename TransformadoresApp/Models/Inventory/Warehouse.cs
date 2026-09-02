using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Inventory
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Código")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        public ICollection<ItemStock> Stocks { get; set; } = new List<ItemStock>();

        public ICollection<StockMovement> StockMovements { get; set; }  = new List<StockMovement>();

        public bool IsActive { get; set; } = true;
    }
}