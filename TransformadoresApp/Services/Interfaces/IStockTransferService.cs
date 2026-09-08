namespace TransformadoresApp.Services.Interfaces
{
    public interface IStockTransferService
    {
        Task TransferAsync(int itemId, int sourceWarehouseId, int destinationWarehouseId, decimal quantity);
    }
}