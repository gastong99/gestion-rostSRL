using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Purchasing
{
    public class PurchaseOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        [Required]
        public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateOnly? ExpectedDate { get; set; }

        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}