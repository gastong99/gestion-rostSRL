using Microsoft.EntityFrameworkCore;
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

        public async Task RegisterMovementAsync(int itemId, int warehouseId, MovementType movementType, decimal quantity, string? notes = null)
        {
            if (itemId <= 0) throw new InvalidOperationException("Debe seleccionar un item válido.");

            if (warehouseId <= 0) throw new InvalidOperationException("Debe seleccionar un depósito válido.");

            if (quantity == 0) throw new InvalidOperationException("La cantidad del movimiento no puede ser cero.");

            var itemExists = await _context.Items.AnyAsync(i => i.Id == itemId && i.IsActive);

            if (!itemExists) throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");

            var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive);

            if (!warehouseExists) throw new InvalidOperationException("El depósito seleccionado no existe o está inactivo.");

            _context.StockMovements.Add(
                new StockMovement
                {
                    ItemId = itemId,
                    WarehouseId = warehouseId,
                    MovementType = movementType,
                    Quantity = quantity,
                    Notes = notes?.Trim(),
                    MovementDate = DateTime.UtcNow
                });
        }
    }
}