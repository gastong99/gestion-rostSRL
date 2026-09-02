using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(ApplicationDbContext context, IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.ShowInactive = showInactive;

            var query = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!showInactive) {
                query = query.Where(p => p.IsActive);
            }

            var purchaseOrders = await query
                .OrderByDescending(p => p.OrderDate)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.Suppliers = new SelectList(
                await _context.Suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.BusinessName)
                    .ToListAsync(),
                "Id",
                "BusinessName");

            return View(purchaseOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null) return NotFound();

            ViewBag.Items = new SelectList(
                await _context.Items
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.Name)
                    .ToListAsync(),
                "Id",
                "Name");

            ViewBag.Warehouses = new SelectList(
                await _context.Warehouses
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync(),
                "Id",
                "Name");

            return View(purchaseOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrder purchaseOrder)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Los datos ingresados no son válidos.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _purchaseOrderService.CreateAsync(purchaseOrder);

                TempData["Success"] = "Orden de compra creada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrder purchaseOrder)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Los datos ingresados no son válidos.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _purchaseOrderService.UpdateAsync(id, purchaseOrder);

                TempData["Success"] = "Orden de compra actualizada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                await _purchaseOrderService.ConfirmAsync(id);
                TempData["Success"] = "Orden de compra confirmada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _purchaseOrderService.CancelAsync(id);

                TempData["Success"] = "Orden de compra cancelada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _purchaseOrderService.DeactivateAsync(id);

                TempData["Success"] = "Orden de compra desactivada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _purchaseOrderService.RestoreAsync(id);

                TempData["Success"] = "Orden de compra restaurada correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}