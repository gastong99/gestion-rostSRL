namespace TransformadoresApp.Services.Interfaces
{
    public interface IStockService
    {
        Task IncreaseStockAsync(
            int itemId,
            int warehouseId,
            decimal quantity,
            CancellationToken cancellationToken = default);

        Task AdjustStockAsync(
            int stockId,
            decimal adjustment,
            CancellationToken cancellationToken = default);

        Task CreateInitialStockAsync(
            int itemId,
            int warehouseId,
            decimal quantity,
            CancellationToken cancellationToken = default);
    }
}