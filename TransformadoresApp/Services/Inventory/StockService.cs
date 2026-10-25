using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Inventory
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockMovementService _stockMovementService;

        public StockService(
            ApplicationDbContext context,
            IStockMovementService stockMovementService)
        {
            _context = context;
            _stockMovementService = stockMovementService;
        }

        // =====================
        // INCREASE STOCK
        // =====================

        public async Task IncreaseStockAsync(
            int itemId,
            int warehouseId,
            decimal quantity,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(itemId, warehouseId);

            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "La cantidad a ingresar debe ser mayor que cero.");
            }

            await InventoryValidationHelper.ValidateItemAsync(
                _context,
                itemId,
                cancellationToken);

            await InventoryValidationHelper.ValidateWarehouseAsync(
                _context,
                warehouseId,
                cancellationToken: cancellationToken);

            var stock = await InventoryValidationHelper.GetStockAsync(
                _context,
                itemId,
                warehouseId,
                cancellationToken);

            if (stock == null)
            {
                stock = new ItemStock
                {
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    ReservedQuantity = 0
                };

                _context.ItemStocks.Add(stock);
            }
            else
            {
                stock.Quantity += quantity;
            }
        }

        // =====================
        // CREATE INITIAL STOCK
        // =====================

        public async Task CreateInitialStockAsync(
            int itemId,
            int warehouseId,
            decimal quantity,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(itemId, warehouseId);

            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "La cantidad inicial debe ser mayor que cero.");
            }

            await InventoryValidationHelper.ValidateItemAsync(
                _context,
                itemId,
                cancellationToken);

            await InventoryValidationHelper.ValidateWarehouseAsync(
                _context,
                warehouseId,
                cancellationToken: cancellationToken);

            var exists = await _context.ItemStocks.AnyAsync(
                s => s.ItemId == itemId && s.WarehouseId == warehouseId,
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Ya existe stock para ese item en el depósito seleccionado.");
            }

            var stock = new ItemStock
            {
                ItemId = itemId,
                WarehouseId = warehouseId,
                Quantity = quantity,
                ReservedQuantity = 0
            };

            _context.ItemStocks.Add(stock);

            await _stockMovementService.RegisterMovementAsync(
                itemId,
                warehouseId,
                MovementType.InitialLoad,
                quantity,
                "Carga inicial de stock",
                cancellationToken);
        }

        // =====================
        // ADJUST STOCK
        // =====================

        public async Task AdjustStockAsync(
            int stockId,
            decimal adjustment,
            CancellationToken cancellationToken = default)
        {
            if (stockId <= 0)
            {
                throw new InvalidOperationException(
                    "El registro de stock indicado no es válido.");
            }

            if (adjustment == 0)
            {
                throw new InvalidOperationException(
                    "El ajuste no puede ser cero.");
            }

            var stock = await _context.ItemStocks.FirstOrDefaultAsync(
                s => s.Id == stockId,
                cancellationToken);

            if (stock == null)
            {
                throw new InvalidOperationException(
                    "El registro de stock no existe.");
            }

            await InventoryValidationHelper.ValidateItemAsync(
                _context,
                stock.ItemId,
                cancellationToken);

            await InventoryValidationHelper.ValidateWarehouseAsync(
                _context,
                stock.WarehouseId,
                cancellationToken: cancellationToken);

            var newQuantity = stock.Quantity + adjustment;

            if (newQuantity < 0)
            {
                throw new InvalidOperationException(
                    "El ajuste dejaría el stock en una cantidad negativa.");
            }

            if (newQuantity < stock.ReservedQuantity)
            {
                throw new InvalidOperationException(
                    "El ajuste no puede dejar el stock por debajo de la cantidad reservada.");
            }

            stock.Quantity = newQuantity;

            await _stockMovementService.RegisterMovementAsync(
                stock.ItemId,
                stock.WarehouseId,
                MovementType.InventoryAdjustment,
                adjustment,
                "Ajuste manual de inventario",
                cancellationToken);
        }

        // =====================
        // LOCAL VALIDATIONS
        // =====================

        private static void ValidateIds(int itemId, int warehouseId)
        {
            if (itemId <= 0)
            {
                throw new InvalidOperationException(
                    "El item indicado no es válido.");
            }

            if (warehouseId <= 0)
            {
                throw new InvalidOperationException(
                    "El depósito indicado no es válido.");
            }
        }
    }
}
