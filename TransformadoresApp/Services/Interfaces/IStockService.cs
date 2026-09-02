namespace TransformadoresApp.Services.Interfaces
{
    public interface IStockService
    {
        Task IncreaseStockAsync(int itemId, int warehouseId, decimal quantity);
        Task AdjustStockAsync(int stockId, decimal adjustment);
        Task CreateInitialStockAsync(int itemId, int warehouseId, decimal quantity);
    }
}