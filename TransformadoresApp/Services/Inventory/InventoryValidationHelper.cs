
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Services.Inventory
{
    internal static class InventoryValidationHelper
    {
        internal static async Task ValidateItemAsync(ApplicationDbContext context, int itemId, CancellationToken cancellationToken = default)
        {
            var exists = await context.Items.AnyAsync(i => i.Id == itemId && i.IsActive, cancellationToken);

            if (!exists) {
                throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");
            }
        }

        internal static async Task ValidateWarehouseAsync(ApplicationDbContext context, int warehouseId, string? warehouseRole = null, string? invalidIdMessage = null, string? notFoundMessage = null, CancellationToken cancellationToken = default)
        {
            if (warehouseId <= 0) {
                throw new InvalidOperationException(invalidIdMessage ?? "El depósito indicado no es válido.");
            }

            var exists = await context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive, cancellationToken);

            if (!exists) {
                var message = notFoundMessage;

                if (string.IsNullOrWhiteSpace(message)) {
                    message = string.IsNullOrWhiteSpace(warehouseRole) ? "El depósito seleccionado no existe o está inactivo." : $"El depósito de {warehouseRole} seleccionado no existe o está inactivo.";
                }

                throw new InvalidOperationException(message);
            }
        }

        internal static async Task<ItemStock?> GetStockAsync(ApplicationDbContext context, int itemId, int warehouseId, CancellationToken cancellationToken = default)
        {
            return await context.ItemStocks.FirstOrDefaultAsync(s => s.ItemId == itemId && s.WarehouseId == warehouseId, cancellationToken);
        }
    }
}