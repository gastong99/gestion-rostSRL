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

        public async Task IncreaseStockAsync(int itemId, int warehouseId, decimal quantity)
        {
            ValidateIds(itemId, warehouseId);

            if (quantity <= 0) {
                throw new InvalidOperationException("La cantidad a ingresar debe ser mayor que cero.");
            }

            await ValidateItemAsync(itemId);
            await ValidateWarehouseAsync(warehouseId);

            var stock = await GetStockAsync(itemId, warehouseId);

            if (stock == null) {
                stock = new ItemStock
                {
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    Quantity = quantity,
                    ReservedQuantity = 0
                };

                _context.ItemStocks.Add(stock);
            }
            else {
                stock.Quantity += quantity;
            }
        }

        public async Task CreateInitialStockAsync(int itemId, int warehouseId, decimal quantity)
        {
            ValidateIds(itemId, warehouseId);

            if (quantity <= 0) {
                throw new InvalidOperationException("La cantidad inicial debe ser mayor que cero.");
            }

            await ValidateItemAsync(itemId);
            await ValidateWarehouseAsync(warehouseId);

            var exists = await _context.ItemStocks.AnyAsync(s => s.ItemId == itemId && s.WarehouseId == warehouseId);

            if (exists) {
                throw new InvalidOperationException("Ya existe stock para ese item en el depósito seleccionado.");
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
                "Carga inicial de stock");
        }

        public async Task AdjustStockAsync(int stockId, decimal adjustment)
        {
            if (stockId <= 0) {
                throw new InvalidOperationException("El registro de stock indicado no es válido.");
            }

            if (adjustment == 0) {
                throw new InvalidOperationException("El ajuste no puede ser cero.");
            }

            var stock = await _context.ItemStocks.FirstOrDefaultAsync(s => s.Id == stockId);

            if (stock == null) {
                throw new InvalidOperationException("El registro de stock no existe.");
            }

            await ValidateItemAsync(stock.ItemId);
            await ValidateWarehouseAsync(stock.WarehouseId);

            var newQuantity = stock.Quantity + adjustment;

            if (newQuantity < 0) {
                throw new InvalidOperationException("El ajuste dejaría el stock en una cantidad negativa.");
            }

            if (newQuantity < stock.ReservedQuantity) {
                throw new InvalidOperationException("El ajuste no puede dejar el stock por debajo de la cantidad reservada.");
            }

            stock.Quantity = newQuantity;

            await _stockMovementService.RegisterMovementAsync(
                stock.ItemId,
                stock.WarehouseId,
                MovementType.InventoryAdjustment,
                adjustment,
                "Ajuste manual de inventario");
        }

        // VALIDACIONES
        private static void ValidateIds(int itemId, int warehouseId)
        {
            if (itemId <= 0) {
                throw new InvalidOperationException("El item indicado no es válido.");
            }

            if (warehouseId <= 0) {
                throw new InvalidOperationException("El depósito indicado no es válido.");
            }
        }

        private async Task ValidateItemAsync(int itemId)
        {
            var exists = await _context.Items.AnyAsync(i => i.Id == itemId && i.IsActive);

            if (!exists) {
                throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");
            }
        }

        private async Task ValidateWarehouseAsync(int warehouseId)
        {
            var exists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive);

            if (!exists) {
                throw new InvalidOperationException("El depósito seleccionado no existe o está inactivo.");
            }
        }

        // HELPERS
        private async Task<ItemStock?> GetStockAsync(int itemId, int warehouseId)
        {
            return await _context.ItemStocks.FirstOrDefaultAsync(s => s.ItemId == itemId && s.WarehouseId == warehouseId);
        }
    }
}