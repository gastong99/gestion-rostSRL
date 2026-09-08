using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Inventory
{
    public class StockTransferService : IStockTransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public StockTransferService(ApplicationDbContext context, IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
        }

        public async Task TransferAsync(int itemId, int sourceWarehouseId, int destinationWarehouseId, decimal quantity)
        {
            if (itemId <= 0) throw new InvalidOperationException("Debe seleccionar un item válido.");

            if (sourceWarehouseId <= 0) throw new InvalidOperationException("Debe seleccionar un depósito de origen válido.");

            if (destinationWarehouseId <= 0) throw new InvalidOperationException("Debe seleccionar un depósito de destino válido.");

            if (sourceWarehouseId == destinationWarehouseId) throw new InvalidOperationException("El depósito de origen y destino no pueden ser el mismo.");

            if (quantity <= 0) throw new InvalidOperationException("La cantidad a transferir debe ser mayor que cero.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await ValidateItemAsync(itemId);

                await ValidateWarehouseAsync(sourceWarehouseId, "origen");
                await ValidateWarehouseAsync(destinationWarehouseId, "destino");

                var sourceStock = await _context.ItemStocks.FirstOrDefaultAsync(s => s.ItemId == itemId && s.WarehouseId == sourceWarehouseId);

                if (sourceStock == null) throw new InvalidOperationException("No existe stock del item seleccionado en el depósito de origen.");

                if (sourceStock.AvailableQuantity < quantity) throw new InvalidOperationException("La cantidad a transferir supera el stock disponible en el depósito de origen.");

                var destinationStock = await _context.ItemStocks.FirstOrDefaultAsync(s => s.ItemId == itemId && s.WarehouseId == destinationWarehouseId);

                if (destinationStock == null) {
                    destinationStock = new ItemStock
                    {
                        ItemId = itemId,
                        WarehouseId = destinationWarehouseId,
                        Quantity = 0,
                        ReservedQuantity = 0
                    };

                    _context.ItemStocks.Add(destinationStock);
                }

                sourceStock.Quantity -= quantity;
                destinationStock.Quantity += quantity;

                await _stockMovementService.RegisterMovementAsync(
                    itemId,
                    sourceWarehouseId,
                    MovementType.TransferOut,
                    -quantity,
                    $"Transferencia a depósito {destinationWarehouseId}");

                await _stockMovementService.RegisterMovementAsync(
                    itemId,
                    destinationWarehouseId,
                    MovementType.TransferIn,
                    quantity,
                    $"Transferencia desde depósito {sourceWarehouseId}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ValidateItemAsync(int itemId)
        {
            var exists = await _context.Items.AnyAsync(i => i.Id == itemId && i.IsActive);

            if (!exists) throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");
        }

        private async Task ValidateWarehouseAsync(int warehouseId, string warehouseRole)
        {
            var exists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive);

            if (!exists) throw new InvalidOperationException($"El depósito de {warehouseRole} seleccionado no existe o está inactivo.");
        }
    }
}