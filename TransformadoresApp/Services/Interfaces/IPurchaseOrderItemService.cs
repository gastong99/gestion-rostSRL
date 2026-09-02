using TransformadoresApp.Models.Purchasing;

namespace TransformadoresApp.Services.Interfaces
{
    public interface IPurchaseOrderItemService
    {
        Task CreateAsync(PurchaseOrderItem item);
        Task UpdateAsync(PurchaseOrderItem item);
        Task<int> DeleteAsync(int id);
    }
}