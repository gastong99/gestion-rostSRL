using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Inventory;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReceiptsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceiptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var receipts = await _context.StockMovements
                .AsNoTracking()
                .Include(sm => sm.Item)
                .Include(sm => sm.Warehouse)
                .Where(sm => sm.MovementType == MovementType.Purchase)
                .OrderByDescending(sm => sm.MovementDate)
                .ThenByDescending(sm => sm.Id)
                .ToListAsync();

            return View(receipts);
        }
    }
}