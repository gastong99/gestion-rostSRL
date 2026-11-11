
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UnitOfMeasuresController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UnitOfMeasuresController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =====================
        // INDEX
        // =====================

        // GET: /UnitOfMeasures
        public async Task<IActionResult> Index()
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Panel de Administración", "Index", "Admin"),
                ("Unidades de Medida", null, null)
            };

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Info = TempData["Info"];

            var units = await _db.UnitOfMeasures
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View(units);
        }

        // =====================
        // CREATE
        // =====================

        // GET: /UnitOfMeasures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /UnitOfMeasures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Abbreviation")] UnitOfMeasure unit)
        {
            if (!ModelState.IsValid)
            {
                return View(unit);
            }

            var name = unit.Name.Trim();
            var abbreviation = unit.Abbreviation.Trim();

            var alreadyExists = await _db.UnitOfMeasures.AnyAsync(u =>
                u.Name.ToLower() == name.ToLower() ||
                u.Abbreviation.ToLower() == abbreviation.ToLower());

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    nameof(unit.Name),
                    "Ya existe una unidad con ese nombre o abreviatura.");

                return View(unit);
            }

            unit.Name = name;
            unit.Abbreviation = abbreviation;

            _db.UnitOfMeasures.Add(unit);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Unidad '{unit.Name}' ({unit.Abbreviation}) creada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================
        // EDIT
        // =====================

        // GET: /UnitOfMeasures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _db.UnitOfMeasures.FindAsync(id);

            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        // POST: /UnitOfMeasures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("UnitOfMeasureId,Name,Abbreviation")] UnitOfMeasure unit)
        {
            if (id != unit.UnitOfMeasureId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(unit);
            }

            var name = unit.Name.Trim();
            var abbreviation = unit.Abbreviation.Trim();

            var alreadyExists = await _db.UnitOfMeasures.AnyAsync(u =>
                u.UnitOfMeasureId != id &&
                (u.Name.ToLower() == name.ToLower() ||
                 u.Abbreviation.ToLower() == abbreviation.ToLower()));

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    nameof(unit.Name),
                    "Ya existe otra unidad con ese nombre o abreviatura.");

                return View(unit);
            }

            var existingUnit = await _db.UnitOfMeasures.FindAsync(id);

            if (existingUnit == null)
            {
                return NotFound();
            }

            existingUnit.Name = name;
            existingUnit.Abbreviation = abbreviation;

            await _db.SaveChangesAsync();

            TempData["Info"] =
                $"Unidad '{existingUnit.Name}' actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================
        // DELETE
        // =====================

        // GET: /UnitOfMeasures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _db.UnitOfMeasures
                .FirstOrDefaultAsync(u => u.UnitOfMeasureId == id);

            if (unit == null)
            {
                return NotFound();
            }

            var usage = await GetUnitUsageAsync(id.Value);

            ViewBag.IsUsed = usage.IsUsed;
            ViewBag.ItemsUsingUnit = usage.Items;

            return View(unit);
        }

        // POST: /UnitOfMeasures/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _db.UnitOfMeasures.FindAsync(id);

            if (unit == null)
            {
                return NotFound();
            }

            var usage = await GetUnitUsageAsync(id);

            if (usage.IsUsed)
            {
                TempData["Error"] =
                    $"No se puede eliminar la unidad '{unit.Name}' porque está en uso por artículos.";

                return RedirectToAction(nameof(Index));
            }

            _db.UnitOfMeasures.Remove(unit);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Unidad '{unit.Name}' eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // =====================
        // UNIT USAGE
        // =====================

        // GET: /UnitOfMeasures/CheckUnitUsage?unitId=5
        [HttpGet]
        public async Task<IActionResult> CheckUnitUsage(int unitId)
        {
            if (unitId <= 0)
            {
                return BadRequest();
            }

            var usage = await GetUnitUsageAsync(unitId);

            return Json(new
            {
                isUsed = usage.IsUsed,
                itemsCount = usage.Items.Count,
                items = usage.Items
            });
        }

        // =====================
        // PRIVATE HELPERS
        // =====================

        private async Task<UnitUsageResult> GetUnitUsageAsync(int unitId)
        {
            var items = await _db.Items
                .AsNoTracking()
                .Where(item => item.UnitOfMeasureId == unitId)
                .OrderBy(item => item.Name)
                .Select(item => item.Name)
                .ToListAsync();

            return new UnitUsageResult
            {
                Items = items
            };
        }

        private sealed class UnitUsageResult
        {
            public List<string> Items { get; init; } = new();

            public bool IsUsed => Items.Count > 0;
        }
    }
}
