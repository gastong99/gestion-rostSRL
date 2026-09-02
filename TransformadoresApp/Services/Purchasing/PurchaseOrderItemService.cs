using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Purchasing
{
    public class PurchaseOrderItemService
        : IPurchaseOrderItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderItemService(ApplicationDbContext context, IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _purchaseOrderService = purchaseOrderService;
        }

        // CREATE
        public async Task CreateAsync(PurchaseOrderItem item)
        {
            // VALIDAR ORDEN
            var purchaseOrder = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == item.PurchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            // VALIDAR ESTADO DE LA ORDEN
            if (!purchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible agregar items a una orden de compra desactivada.");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("Solo es posible agregar items a una orden en estado borrador.");
            }

            // VALIDAR ITEM
            var catalogItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.ItemId && i.IsActive);

            if (catalogItem == null) {
                throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");
            }

            // VALIDAR DUPLICADO
            var exists = await _context.PurchaseOrderItems.AnyAsync(i => i.PurchaseOrderId == item.PurchaseOrderId && i.ItemId == item.ItemId);

            if (exists) {
                throw new InvalidOperationException("Ese item ya existe en la orden.");
            }

            // DATOS INICIALES
            item.ReceivedQuantity = 0;

            _context.PurchaseOrderItems.Add(item);

            await _context.SaveChangesAsync();
        }

        // UPDATE
        public async Task UpdateAsync(PurchaseOrderItem item)
        {
            // VALIDAR DATOS
            if (item.Quantity <= 0) {
                throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
            }

            if (item.UnitPrice < 0) {
                throw new InvalidOperationException("El precio unitario no puede ser negativo.");
            }

            // OBTENER ITEM EXISTENTE
            var existingItem = await _context.PurchaseOrderItems.Include(i => i.PurchaseOrder).FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existingItem == null) {
                throw new InvalidOperationException("El item de la orden no existe.");
            }

            if (existingItem.PurchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            var purchaseOrderId = existingItem.PurchaseOrderId;

            // VALIDAR ORDEN
            if (item.PurchaseOrderId != purchaseOrderId) {
                throw new InvalidOperationException("La orden de compra indicada no coincide con el item.");
            }

            if (!existingItem.PurchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible modificar items de una orden de compra desactivada.");
            }

            if (existingItem.PurchaseOrder.Status != PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("Solo es posible modificar items de una orden en estado borrador.");
            }

            // VALIDAR ITEM
            var catalogItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == item.ItemId && i.IsActive);

            if (catalogItem == null) {
                throw new InvalidOperationException("El item seleccionado no existe o está inactivo.");
            }

            // NO CAMBIAR ITEM
            if (item.ItemId != existingItem.ItemId) {
                throw new InvalidOperationException("No es posible cambiar el item de una línea existente.");
            }

            // VALIDAR RECIBIDO
            if (item.Quantity < existingItem.ReceivedQuantity) {
                throw new InvalidOperationException($"La cantidad no puede ser menor que la cantidad ya recibida ({existingItem.ReceivedQuantity:N2}).");
            }

            // ACTUALIZAR
            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = item.UnitPrice;

            await _context.SaveChangesAsync();

            // ACTUALIZAR ESTADO
            await _purchaseOrderService.UpdateStatusAsync(purchaseOrderId);

            await _context.SaveChangesAsync();
        }

        // DELETE
        public async Task<int> DeleteAsync(int id)
        {
            // OBTENER ITEM
            var item = await _context.PurchaseOrderItems.Include(i => i.PurchaseOrder).FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) {
                throw new InvalidOperationException("El item de la orden no existe.");
            }

            if (item.PurchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            var purchaseOrderId = item.PurchaseOrderId;

            // VALIDAR ORDEN
            if (!item.PurchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible eliminar items de una orden de compra desactivada.");
            }

            if (item.PurchaseOrder.Status != PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("Solo es posible eliminar items de una orden en estado borrador.");
            }

            // VALIDAR RECIBIDO
            if (item.ReceivedQuantity > 0) {
                throw new InvalidOperationException("No es posible eliminar un item que ya tiene mercadería recibida.");
            }

            // ELIMINAR
            _context.PurchaseOrderItems.Remove(item);

            await _context.SaveChangesAsync();

            // ACTUALIZAR ESTADO
            await _purchaseOrderService.UpdateStatusAsync(purchaseOrderId);

            await _context.SaveChangesAsync();

            return purchaseOrderId;
        }
    }
}