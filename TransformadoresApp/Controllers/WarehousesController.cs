using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.ShowInactive = showInactive;

            var query = _context.Warehouses.AsQueryable();

            if (!showInactive) {
                query = query.Where(w => w.IsActive);
            }

            var warehouses = await query
                .OrderBy(w => w.Name)
                .ToListAsync();

            return View(warehouses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            warehouse.Code = warehouse.Code.Trim();
            warehouse.Name = warehouse.Name.Trim();

            var codeExists = await _context.Warehouses
                .AnyAsync(w => w.Code == warehouse.Code);

            if (codeExists)
            {
                TempData["Error"] =
                    "Ya existe un depósito con ese código.";

                return RedirectToAction(nameof(Index));
            }

            _context.Warehouses.Add(warehouse);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Depósito creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            warehouse.Code = warehouse.Code.Trim();
            warehouse.Name = warehouse.Name.Trim();

            var existingWarehouse = await _context.Warehouses.FindAsync(id);

            if (existingWarehouse == null)
                return NotFound();

            var codeExists = await _context.Warehouses
                .AnyAsync(w => w.Code == warehouse.Code &&
                               w.Id != warehouse.Id);

            if (codeExists)
            {
                TempData["Error"] =
                    "Ya existe un depósito con ese código.";

                return RedirectToAction(nameof(Index));
            }

            existingWarehouse.Code = warehouse.Code;
            existingWarehouse.Name = warehouse.Name;
            existingWarehouse.Description = warehouse.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Depósito actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null)
                return NotFound();

            if (!warehouse.IsActive)
            {
                TempData["Warning"] =
                    "El depósito ya se encuentra desactivado.";

                return RedirectToAction(nameof(Index));
            }

            warehouse.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Depósito desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);

            if (warehouse == null)
                return NotFound();

            if (warehouse.IsActive)
            {
                TempData["Warning"] =
                    "El depósito ya se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }

            warehouse.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Depósito restaurado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}