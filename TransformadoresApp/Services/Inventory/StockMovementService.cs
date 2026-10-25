
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Inventory
{
    public class StockMovementService : IStockMovementService
    {
        private readonly ApplicationDbContext _context;

        public StockMovementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegisterMovementAsync(int itemId, int warehouseId, MovementType movementType, decimal quantity, string? notes = null, CancellationToken cancellationToken = default)
        {
            ValidateIds(itemId, warehouseId);

            if (!Enum.IsDefined(movementType)) {
                throw new InvalidOperationException("El tipo de movimiento indicado no es válido.");
            }

            if (quantity == 0) {
                throw new InvalidOperationException("La cantidad del movimiento no puede ser cero.");
            }

            await InventoryValidationHelper.ValidateItemAsync(_context, itemId, cancellationToken);

            await InventoryValidationHelper.ValidateWarehouseAsync(_context, warehouseId, cancellationToken: cancellationToken);

            var movement = new StockMovement
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                MovementType = movementType,
                Quantity = quantity,
                Notes = notes?.Trim(),
                MovementDate = DateTime.UtcNow
            };

            _context.StockMovements.Add(movement);
        }

        private static void ValidateIds(int itemId, int warehouseId)
        {
            if (itemId <= 0) {
                throw new InvalidOperationException("Debe seleccionar un item válido.");
            }

            if (warehouseId <= 0) {
                throw new InvalidOperationException("Debe seleccionar un depósito válido.");
            }
        }
    }
}