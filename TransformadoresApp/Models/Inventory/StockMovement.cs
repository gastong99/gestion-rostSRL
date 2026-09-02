using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TransformadoresApp.Models.Catalogs;

namespace TransformadoresApp.Models.Inventory
{
    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        public int ItemId { get; set; }

        public Item? Item { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        [Required]
        public MovementType MovementType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    }
}
