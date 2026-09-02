using TransformadoresApp.Models.Purchasing;

namespace TransformadoresApp.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task CreateAsync(PurchaseOrder purchaseOrder);
        Task UpdateAsync(int id, PurchaseOrder purchaseOrder);
        Task ConfirmAsync(int purchaseOrderId);
        Task CancelAsync(int purchaseOrderId);
        Task DeactivateAsync(int purchaseOrderId);
        Task RestoreAsync(int purchaseOrderId);
        Task UpdateStatusAsync(int purchaseOrderId);
    }
}