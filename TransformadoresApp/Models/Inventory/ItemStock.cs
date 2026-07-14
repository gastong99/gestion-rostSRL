using System.ComponentModel.DataAnnotations.Schema;
using TransformadoresApp.Models.Catalogs;

namespace TransformadoresApp.Models.Inventory
{
    public class ItemStock
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public int WarehouseId { get; set; }

        public Warehouse? Warehouse { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ReservedQuantity { get; set; }

        [NotMapped]
        public decimal AvailableQuantity => Quantity - ReservedQuantity;
    }
}