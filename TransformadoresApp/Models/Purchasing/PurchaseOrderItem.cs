using System.ComponentModel.DataAnnotations;
using TransformadoresApp.Models.Catalogs;

namespace TransformadoresApp.Models.Purchasing
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseOrderId { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }

        [Required]
        public int ItemId { get; set; }

        public Item? Item { get; set; }

        [Required]
        public decimal Quantity { get; set; }
        public decimal ReceivedQuantity { get; set; } = 0;
        public decimal PendingQuantity => Quantity - ReceivedQuantity;

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal Subtotal => Quantity * UnitPrice;
    }
}