using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PurchaseOrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseReceivingService _purchaseReceivingService;
        private readonly IPurchaseOrderItemService _purchaseOrderItemService;

        public PurchaseOrderItemsController(ApplicationDbContext context, IPurchaseReceivingService purchaseReceivingService, IPurchaseOrderItemService purchaseOrderItemService)
        {
            _context = context;
            _purchaseReceivingService = purchaseReceivingService;
            _purchaseOrderItemService = purchaseOrderItemService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderItem item)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Los datos ingresados no son válidos.";

                return RedirectToAction("Details", "PurchaseOrders", new { id = item.PurchaseOrderId });
            }

            try
            {
                await _purchaseOrderItemService.CreateAsync(item);

                TempData["Success"] = "Item agregado correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "PurchaseOrders", new { id = item.PurchaseOrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PurchaseOrderItem item)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Los datos ingresados no son válidos.";

                return RedirectToAction("Details", "PurchaseOrders", new { id = item.PurchaseOrderId });
            }

            try
            {
                await _purchaseOrderItemService.UpdateAsync(item);

                TempData["Success"] = "Item actualizado correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "PurchaseOrders", new { id = item.PurchaseOrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var purchaseOrderId = await _purchaseOrderItemService.DeleteAsync(id);

                TempData["Success"] = "Item eliminado correctamente.";

                return RedirectToAction("Details", "PurchaseOrders", new { id = purchaseOrderId });
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;

                return await RedirectToPurchaseOrderAsync(id);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, int warehouseId, decimal quantity)
        {
            try
            {
                var purchaseOrderId = await _purchaseReceivingService.ReceiveAsync(id, warehouseId, quantity);

                TempData["Success"] = "Mercadería recibida correctamente.";

                return RedirectToAction("Details", "PurchaseOrders", new { id = purchaseOrderId });
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;

                return await RedirectToPurchaseOrderAsync(id);
            }
            catch {
                TempData["Error"] = "Ocurrió un error al registrar la recepción de la mercadería.";

                return await RedirectToPurchaseOrderAsync(id);
            }
        }

        private async Task<IActionResult> RedirectToPurchaseOrderAsync(int itemId)
        {
            var purchaseOrderId =
                await _context.PurchaseOrderItems
                    .Where(i => i.Id == itemId)
                    .Select(i => i.PurchaseOrderId)
                    .FirstOrDefaultAsync();

            if (purchaseOrderId == 0) {
                return RedirectToAction("Index", "PurchaseOrders");
            }

            return RedirectToAction("Details", "PurchaseOrders", new { id = purchaseOrderId });
        }
    }
}