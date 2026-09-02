namespace TransformadoresApp.Services.Interfaces
{
    public interface IPurchaseReceivingService
    {
        Task<int> ReceiveAsync(int purchaseOrderItemId, int warehouseId, decimal quantity);
    }
}