using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Catalogs;
using TransformadoresApp.Helpers;

namespace TransformadoresApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Categorías", null, null)
            };

            IQueryable<Category> query = _db.Categories
                .Include(c => c.Parent);

            if (!showInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.ShowInactive = showInactive;

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Info = TempData["Info"];

            return View(categories);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (await _db.Categories.AnyAsync(c =>
                c.IsActive &&
                c.Name.ToLower() == category.Name.ToLower()))
            {
                TempData["Error"] =
                    "Ya existe una categoría con ese nombre.";

                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);

                if (category.ItemType == null)
                {
                    ModelState.AddModelError(nameof(category.ItemType),
                        "Debe seleccionar un tipo.");

                    return RedirectToAction(nameof(Index));
                }

                await _db.SaveChangesAsync();

                TempData["Success"] =
                    $"Categoría '{category.Name}' creada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
                return NotFound();

            var existing = await _db.Categories.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.Name = category.Name;
            existing.ParentId = category.ParentId;

            if (category.ItemType == null)
            {
                ModelState.AddModelError(nameof(category.ItemType),
                    "Debe seleccionar un tipo.");

                if (category.ParentId == category.Id) {
                    TempData["Error"] = "Una categoría no puede ser su propia categoría padre.";

                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }

            await _db.SaveChangesAsync();

            TempData["Info"] =
                $"Categoría '{existing.Name}' actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            category.IsActive = false;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Categoría '{category.Name}' desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            category.IsActive = true;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Categoría '{category.Name}' reactivada correctamente.";

            return RedirectToAction(nameof(Index),
                new { showInactive = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesList(int? excludeId)
        {
            var query = _db.Categories
                 .Where(c => c.IsActive);

            if (excludeId.HasValue) {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var categories = await query
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
        public IActionResult GetItemTypesList()
        {
            var itemTypes = Enum.GetValues<ItemType>()
                .Select(t => new
                {
                    id = (int)t,
                    name = t.GetDisplayName()
                });

            return Json(itemTypes);
        }
    }
}