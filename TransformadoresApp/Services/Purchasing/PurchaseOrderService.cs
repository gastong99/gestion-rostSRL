using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Services.Purchasing
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(PurchaseOrder purchaseOrder)
        {
            purchaseOrder.Number = purchaseOrder.Number.Trim();

            bool exists = await _context.PurchaseOrders.AnyAsync(p => p.Number == purchaseOrder.Number);

            if (exists) {
                throw new InvalidOperationException("Ya existe una orden de compra con ese número.");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Draft;
            purchaseOrder.CreatedAt = DateTime.UtcNow;
            purchaseOrder.IsActive = true;

            _context.PurchaseOrders.Add(purchaseOrder);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, PurchaseOrder purchaseOrder)
        {
            if (id != purchaseOrder.Id) {
                throw new InvalidOperationException("La orden de compra indicada no coincide.");
            }

            purchaseOrder.Number = purchaseOrder.Number.Trim();

            var existingOrder = await _context.PurchaseOrders.FindAsync(id);

            if (existingOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (!existingOrder.IsActive) {
                throw new InvalidOperationException("No es posible modificar una orden de compra desactivada.");
            }

            if (existingOrder.Status != PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("Solo se pueden modificar órdenes de compra en estado borrador.");
            }

            bool exists = await _context.PurchaseOrders.AnyAsync(p => p.Number == purchaseOrder.Number && p.Id != purchaseOrder.Id);

            if (exists) {
                throw new InvalidOperationException("Ya existe una orden de compra con ese número.");
            }

            existingOrder.Number = purchaseOrder.Number;
            existingOrder.SupplierId = purchaseOrder.SupplierId;
            existingOrder.OrderDate = purchaseOrder.OrderDate;
            existingOrder.ExpectedDate = purchaseOrder.ExpectedDate;
            existingOrder.Notes = purchaseOrder.Notes;

            await _context.SaveChangesAsync();
        }

        public async Task ConfirmAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.Include(po => po.Items).FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (!purchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible confirmar una orden de compra desactivada.");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("La orden de compra ya fue confirmada o no se encuentra en estado borrador.");
            }

            if (!purchaseOrder.Items.Any()) {
                throw new InvalidOperationException("No es posible confirmar una orden de compra sin items.");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Pending;

            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (!purchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible cancelar una orden de compra desactivada.");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft && purchaseOrder.Status != PurchaseOrderStatus.Pending) {
                throw new InvalidOperationException("Solo se pueden cancelar órdenes de compra en estado borrador o pendiente.");
            }

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (!purchaseOrder.IsActive) {
                throw new InvalidOperationException("La orden ya se encuentra desactivada.");
            }

            purchaseOrder.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (purchaseOrder.IsActive) {
                throw new InvalidOperationException("La orden ya se encuentra activa.");
            }

            purchaseOrder.IsActive = true;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.Include(po => po.Items).FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

            if (purchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra no existe.");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Draft) return;

            if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled) return;

            if (!purchaseOrder.Items.Any()) {
                purchaseOrder.Status = PurchaseOrderStatus.Draft;
                return;
            }

            var allCompleted = purchaseOrder.Items.All(i => i.ReceivedQuantity >= i.Quantity);

            var noneReceived = purchaseOrder.Items.All(i => i.ReceivedQuantity == 0);

            if (allCompleted) {
                purchaseOrder.Status = PurchaseOrderStatus.Completed;
            }
            else if (noneReceived) {
                purchaseOrder.Status = PurchaseOrderStatus.Pending;
            }
            else {
                purchaseOrder.Status = PurchaseOrderStatus.PartiallyReceived;
            }
        }
    }
}