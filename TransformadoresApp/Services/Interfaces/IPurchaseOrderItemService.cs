using TransformadoresApp.Models.Purchasing;

namespace TransformadoresApp.Services.Interfaces
{
    public interface IPurchaseOrderItemService
    {
        Task CreateAsync(PurchaseOrderItem item, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseOrderItem item, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}