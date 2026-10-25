
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Inventory
{
    public class StockTransferService : IStockTransferService
    {
        // =====================
        // DEPENDENCIES
        // =====================

        private readonly ApplicationDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public StockTransferService(
            ApplicationDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
        }

        // =====================
        // TRANSFER
        // =====================

        public async Task TransferAsync(
            int itemId,
            int sourceWarehouseId,
            int destinationWarehouseId,
            decimal quantity,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(itemId, sourceWarehouseId, destinationWarehouseId);

            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "La cantidad a transferir debe ser mayor que cero.");
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await InventoryValidationHelper.ValidateItemAsync(
                    _context,
                    itemId,
                    cancellationToken);

                await InventoryValidationHelper.ValidateWarehouseAsync(
                    _context,
                    sourceWarehouseId,
                    "origen",
                    cancellationToken: cancellationToken);

                await InventoryValidationHelper.ValidateWarehouseAsync(
                    _context,
                    destinationWarehouseId,
                    "destino",
                    cancellationToken: cancellationToken);

                var sourceStock = await InventoryValidationHelper.GetStockAsync(
                    _context,
                    itemId,
                    sourceWarehouseId,
                    cancellationToken);

                if (sourceStock == null)
                {
                    throw new InvalidOperationException(
                        "No existe stock del item seleccionado en el depósito de origen.");
                }

                if (sourceStock.AvailableQuantity < quantity)
                {
                    throw new InvalidOperationException(
                        "La cantidad a transferir supera el stock disponible en el depósito de origen.");
                }

                var destinationStock = await InventoryValidationHelper.GetStockAsync(
                    _context,
                    itemId,
                    destinationWarehouseId,
                    cancellationToken);

                if (destinationStock == null)
                {
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

                await RegisterTransferMovementsAsync(
                    itemId,
                    sourceWarehouseId,
                    destinationWarehouseId,
                    quantity,
                    cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                // El rollback debe poder ejecutarse aunque se haya cancelado
                // el token original de la operación.
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        // =====================
        // TRANSFER MOVEMENTS
        // =====================

        private async Task RegisterTransferMovementsAsync(
            int itemId,
            int sourceWarehouseId,
            int destinationWarehouseId,
            decimal quantity,
            CancellationToken cancellationToken)
        {
            await _stockMovementService.RegisterMovementAsync(
                itemId,
                sourceWarehouseId,
                MovementType.TransferOut,
                -quantity,
                $"Transferencia a depósito {destinationWarehouseId}",
                cancellationToken);

            await _stockMovementService.RegisterMovementAsync(
                itemId,
                destinationWarehouseId,
                MovementType.TransferIn,
                quantity,
                $"Transferencia desde depósito {sourceWarehouseId}",
                cancellationToken);
        }

        // =====================
        // VALIDATIONS
        // =====================

        private static void ValidateIds(
            int itemId,
            int sourceWarehouseId,
            int destinationWarehouseId)
        {
            if (itemId <= 0)
            {
                throw new InvalidOperationException(
                    "Debe seleccionar un item válido.");
            }

            if (sourceWarehouseId <= 0)
            {
                throw new InvalidOperationException(
                    "Debe seleccionar un depósito de origen válido.");
            }

            if (destinationWarehouseId <= 0)
            {
                throw new InvalidOperationException(
                    "Debe seleccionar un depósito de destino válido.");
            }

            if (sourceWarehouseId == destinationWarehouseId)
            {
                throw new InvalidOperationException(
                    "El depósito de origen y destino no pueden ser el mismo.");
            }
        }
    }
}
