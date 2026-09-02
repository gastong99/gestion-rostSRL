using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Purchasing
{
    public enum PurchaseOrderStatus
    {
        [Display(Name = "Borrador")]
        Draft = 0,

        [Display(Name = "Pendiente")]
        Pending = 1,

        [Display(Name = "Recibida Parcialmente")]
        PartiallyReceived = 2,

        [Display(Name = "Completada")]
        Completed = 3,

        [Display(Name = "Cancelada")]
        Cancelled = 4
    }
}