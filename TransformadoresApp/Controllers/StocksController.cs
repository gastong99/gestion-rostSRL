using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Controllers
{
    public class StocksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StocksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            var stock = await _context.ItemStocks
                .Include(s => s.Item)
                    .ThenInclude(i => i.Category)
                .Include(s => s.Warehouse)
                .OrderBy(s => s.Item!.Name)
                .ThenBy(s => s.Warehouse!.Name)
                .ToListAsync();

            return View(stock);
        }

        // =========================
        // CREATE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemStock stock)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            var exists = await _context.ItemStocks
                .AnyAsync(s =>
                    s.ItemId == stock.ItemId &&
                    s.WarehouseId == stock.WarehouseId);

            if (exists)
            {
                TempData["Error"] =
                    "Ya existe stock para ese item en el depósito seleccionado.";

                return RedirectToAction(nameof(Index));
            }

            if (stock.Quantity < 0)
            {
                TempData["Error"] =
                    "La cantidad no puede ser negativa.";

                return RedirectToAction(nameof(Index));
            }

            stock.ReservedQuantity = 0;

            _context.ItemStocks.Add(stock);

            await _context.SaveChangesAsync();

            _context.StockMovements.Add(new StockMovement
            {
                ItemId = stock.ItemId,
                WarehouseId = stock.WarehouseId,
                MovementType = MovementType.InitialLoad,
                Quantity = stock.Quantity,
                Notes = "Carga inicial de stock",
                MovementDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Stock inicial cargado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemStock stock)
        {
            if (id != stock.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            var existingStock = await _context.ItemStocks
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStock == null)
                return NotFound();

            var duplicate = await _context.ItemStocks
                .AnyAsync(s =>
                    s.ItemId == stock.ItemId &&
                    s.WarehouseId == stock.WarehouseId &&
                    s.Id != stock.Id);

            if (duplicate)
            {
                TempData["Error"] =
                    "Ya existe stock para ese item en el depósito seleccionado.";

                return RedirectToAction(nameof(Index));
            }

            if (stock.Quantity < 0)
            {
                TempData["Error"] =
                    "La cantidad no puede ser negativa.";

                return RedirectToAction(nameof(Index));
            }

            // Guardamos el contexto original para la auditoría
            var originalItemId = existingStock.ItemId;
            var originalWarehouseId = existingStock.WarehouseId;

            // Calculamos la diferencia antes de modificar el registro
            var quantityDifference =
                stock.Quantity - existingStock.Quantity;

            existingStock.ItemId = stock.ItemId;
            existingStock.WarehouseId = stock.WarehouseId;
            existingStock.Quantity = stock.Quantity;

            await _context.SaveChangesAsync();

            // Registramos solamente si realmente hubo un cambio
            if (quantityDifference != 0)
            {
                _context.StockMovements.Add(new StockMovement
                {
                    ItemId = originalItemId,
                    WarehouseId = originalWarehouseId,
                    MovementType = MovementType.InventoryAdjustment,
                    Quantity = quantityDifference,
                    Notes = "Ajuste manual de inventario",
                    MovementDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            TempData["Success"] =
                "Stock actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // GET ITEMS LIST
        // =========================

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

        // =========================
        // GET WAREHOUSES LIST
        // =========================

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
    }
}