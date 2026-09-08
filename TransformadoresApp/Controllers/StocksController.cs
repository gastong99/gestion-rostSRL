using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;
using TransformadoresApp.Services.Interfaces;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class StocksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockService _stockService;

        public StocksController(ApplicationDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public async Task<IActionResult> Index()
        {
            var stock = await _context.ItemStocks
                .Include(s => s.Item)
                    .ThenInclude(i => i.Category)
                .Include(s => s.Warehouse)
                .OrderBy(s => s.Item!.Name)
                .ThenBy(s => s.Warehouse!.Name)
                .ToListAsync();

            ViewBag.Items = await _context.Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .Select(i => new
                {
                    i.Id,
                    i.Code,
                    i.Name
                })
                .ToListAsync();

            ViewBag.Warehouses = await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new
                {
                    w.Id,
                    w.Code,
                    w.Name
                })
                .ToListAsync();

            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemStock stock)
        {
            if (!ModelState.IsValid) {
                TempData["Error"] = "Los datos ingresados no son válidos.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _stockService.CreateInitialStockAsync(stock.ItemId, stock.WarehouseId, stock.Quantity);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Stock inicial cargado correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int itemId, int warehouseId, decimal adjustment)
        {
            try
            {
                var stock = await _context.ItemStocks.FirstOrDefaultAsync(s => s.Id == id);

                if (stock == null) return NotFound();

                if (stock.ItemId != itemId || stock.WarehouseId != warehouseId) {
                    TempData["Error"] = "No es posible modificar el Item o el Depósito desde un ajuste de stock.";

                    return RedirectToAction(nameof(Index));
                }

                await _stockService.AdjustStockAsync(id, adjustment);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Ajuste de stock registrado correctamente.";
            }
            catch (InvalidOperationException ex) {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetItemsList()
        {
            var items = await _context.Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .Select(i => new
                {
                    id = i.Id,
                    code = i.Code,
                    name = i.Name
                })
                .ToListAsync();

            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetWarehousesList()
        {
            var warehouses = await _context.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new
                {
                    id = w.Id,
                    code = w.Code,
                    name = w.Name
                })
                .ToListAsync();

            return Json(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableStock(int itemId, int warehouseId)
        {
            if (itemId <= 0 || warehouseId <= 0) {
                return Json(new
                {
                    success = false,
                    message = "El item y el depósito son obligatorios."
                });
            }

            var stock = await _context.ItemStocks.FirstOrDefaultAsync(s => s.ItemId == itemId && s.WarehouseId == warehouseId);

            if (stock == null) {
                return Json(new
                {
                    success = true,
                    availableQuantity = 0
                });
            }

            return Json(new
            {
                success = true,
                availableQuantity = stock.AvailableQuantity
            });
        }
    }
}