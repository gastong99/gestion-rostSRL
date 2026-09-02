using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Services.Purchasing
{
    public class PurchaseReceivingService : IPurchaseReceivingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockService _stockService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseReceivingService(ApplicationDbContext context, IStockService stockService, IStockMovementService stockMovementService, IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _stockService = stockService;
            _stockMovementService = stockMovementService;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<int> ReceiveAsync(int purchaseOrderItemId, int warehouseId, decimal quantity)
        {
            if (purchaseOrderItemId <= 0) {
                throw new InvalidOperationException("El item de la orden de compra indicado no es válido.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchaseOrderItem = await GetPurchaseOrderItemAsync(purchaseOrderItemId);

                var purchaseOrder = purchaseOrderItem.PurchaseOrder!;

                ValidatePurchaseOrder(purchaseOrder);

                await ValidateWarehouseAsync(warehouseId);

                ValidateQuantity(purchaseOrderItem, quantity);

                await _stockService.IncreaseStockAsync(purchaseOrderItem.ItemId, warehouseId, quantity);

                await _stockMovementService.RegisterMovementAsync(
                    purchaseOrderItem.ItemId,
                    warehouseId,
                    MovementType.Purchase,
                    quantity,
                    $"Recepción OC {purchaseOrder.Number}");

                UpdateReceivedQuantity(purchaseOrderItem, quantity);

                await _purchaseOrderService.UpdateStatusAsync(purchaseOrder.Id);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return purchaseOrder.Id;
            }
            catch {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static void ValidatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            if (!purchaseOrder.IsActive) {
                throw new InvalidOperationException("No es posible recibir mercadería de una orden de compra desactivada.");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Draft) {
                throw new InvalidOperationException("Debe confirmar la orden antes de recibir mercadería.");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Completed) {
                throw new InvalidOperationException("La orden ya fue recibida completamente.");
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled) {
                throw new InvalidOperationException("No es posible recibir mercadería de una orden cancelada.");
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Pending && purchaseOrder.Status != PurchaseOrderStatus.PartiallyReceived) {
                throw new InvalidOperationException("La orden de compra no se encuentra en un estado válido para recibir mercadería.");
            }
        }

        private static void ValidateQuantity(PurchaseOrderItem purchaseOrderItem, decimal quantity)
        {
            if (quantity <= 0) {
                throw new InvalidOperationException("La cantidad a recibir debe ser mayor que cero.");
            }

            var pendingQuantity = purchaseOrderItem.PendingQuantity;

            if (pendingQuantity <= 0) {
                throw new InvalidOperationException("Este item ya fue recibido completamente.");
            }

            if (quantity > pendingQuantity) {
                throw new InvalidOperationException("La cantidad ingresada supera la cantidad pendiente de recibir.");
            }
        }

        private static void UpdateReceivedQuantity(PurchaseOrderItem purchaseOrderItem, decimal quantity)
        {
            purchaseOrderItem.ReceivedQuantity += quantity;
        }

        private async Task ValidateWarehouseAsync(int warehouseId)
        {
            if (warehouseId <= 0) {
                throw new InvalidOperationException("Debe seleccionar un depósito.");
            }

            var exists = await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && w.IsActive);

            if (!exists) {
                throw new InvalidOperationException("El depósito seleccionado no es válido.");
            }
        }

        private async Task<PurchaseOrderItem> GetPurchaseOrderItemAsync(int purchaseOrderItemId)
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems.Include(i => i.PurchaseOrder).FirstOrDefaultAsync(i => i.Id == purchaseOrderItemId);

            if (purchaseOrderItem == null) {
                throw new InvalidOperationException("El item de la orden de compra no existe.");
            }

            if (purchaseOrderItem.PurchaseOrder == null) {
                throw new InvalidOperationException("La orden de compra asociada no existe.");
            }

            return purchaseOrderItem;
        }
    }
}