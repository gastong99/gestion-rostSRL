using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Services.Interfaces
{
    public interface IStockMovementService
    {
        Task RegisterMovementAsync(int itemId, int warehouseId, MovementType movementType, decimal quantity, string? notes = null);
    }
}