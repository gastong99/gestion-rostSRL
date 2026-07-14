using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;

namespace TransformadoresApp.Controllers
{
    public class StockMovementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockMovementsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            var movements = await _context.StockMovements
                .Include(m => m.Item)
                    .ThenInclude(i => i.Category)
                .Include(m => m.Warehouse)
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.Id)
                .ToListAsync();

            return View(movements);
        }
    }
}
