using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Catalogs;

namespace TransformadoresApp.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.ShowInactive = showInactive;

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.UnitOfMeasure)
                .AsQueryable();

            if (!showInactive) {
                query = query.Where(i => i.IsActive);
            }

            var items = await query
                .OrderBy(i => i.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == item.CategoryId);

            if (category == null) return RedirectToAction(nameof(Index));

            item.ItemType = category.ItemType ?? ItemType.CommercialProduct;
            item.Code = item.Code.Trim();
            item.Name = item.Name.Trim();
            item.Description = item.Description?.Trim();

            _context.Items.Add(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item)
        {
            if (id != item.Id) return NotFound();

            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            var existingItem = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == id);

            if (existingItem == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == item.CategoryId);

            if (category == null) return RedirectToAction(nameof(Index));

            existingItem.Code = item.Code.Trim();
            existingItem.Name = item.Name.Trim();
            existingItem.Description = item.Description?.Trim();
            existingItem.CategoryId = item.CategoryId;
            existingItem.ItemType = category.ItemType ?? ItemType.CommercialProduct;
            existingItem.UnitOfMeasureId = item.UnitOfMeasureId;
            existingItem.Cost = item.Cost;
            existingItem.Price = item.Price;
            existingItem.MinimumStock = item.MinimumStock;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null) return NotFound();

            item.IsActive = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null) return NotFound();

            item.IsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesList()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
                .ToListAsync();

            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitOfMeasuresList()
        {
            var units = await _context.UnitOfMeasures
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    unitOfMeasureId = u.UnitOfMeasureId,
                    name = u.Name,
                    abbreviation = u.Abbreviation
                })
                .ToListAsync();

            return Json(units);
        }

        [HttpGet]
        public IActionResult GetItemTypesList()
        {
            var types = Enum.GetValues<ItemType>()
                .Select(t => new
                {
                    id = (int)t,
                    name = t.ToString()
                });

            return Json(types);
        }
    }
}