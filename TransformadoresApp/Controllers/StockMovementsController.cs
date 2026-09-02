using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Controllers
{
    public class StockMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockMovementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? itemId, int? warehouseId, MovementType? movementType, DateTime? dateFrom, DateTime? dateTo)
        {
            var query = _context.StockMovements
                .Include(m => m.Item)
                    .ThenInclude(i => i.Category)
                .Include(m => m.Warehouse)
                .AsQueryable();

            if (itemId.HasValue) {
                query = query.Where(m => m.ItemId == itemId.Value);
            }

            if (warehouseId.HasValue) {
                query = query.Where(m => m.WarehouseId == warehouseId.Value);
            }

            if (movementType.HasValue) {
                query = query.Where(m => m.MovementType == movementType.Value);
            }

            if (dateFrom.HasValue) {
                var from = dateFrom.Value.Date;
                query = query.Where(m => m.MovementDate >= from);
            }

            if (dateTo.HasValue) {
                var to = dateTo.Value.Date.AddDays(1);
                query = query.Where(m => m.MovementDate < to);
            }

            var movements = await query
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.Id)
                .ToListAsync();

            ViewBag.Items = new SelectList(
                await _context.Items
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                itemId);

            ViewBag.Warehouses = new SelectList(
                await _context.Warehouses
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                warehouseId);

            ViewBag.MovementTypes = Enum.GetValues<MovementType>()
                .Select(type => new SelectListItem
                {
                    Value = ((int)type).ToString(),
                    Text = GetMovementTypeName(type),
                    Selected = movementType == type
                })
                .ToList();

            ViewBag.ItemId = itemId;
            ViewBag.WarehouseId = warehouseId;
            ViewBag.MovementType = movementType;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(movements);
        }

        private static string GetMovementTypeName(MovementType type)
        {
            return type switch
            {
                MovementType.InitialLoad => "Carga Inicial",
                MovementType.Purchase => "Compra",
                MovementType.Sale => "Venta",
                MovementType.ProductionConsumption => "Consumo Producción",
                MovementType.ProductionOutput => "Producción",
                MovementType.InventoryAdjustment => "Ajuste",
                MovementType.TransferIn => "Transferencia Entrada",
                MovementType.TransferOut => "Transferencia Salida",
                _ => "Desconocido"
            };
        }
    }
}