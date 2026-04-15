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

        // GET: /UnitOfMeasures
        public async Task<IActionResult> Index()
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Panel de Administración", "Index", "Admin"),
                ("Unidades de Medidas", null, null)
            };

            ViewBag.Success = TempData["Success"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Info = TempData["Info"];

            var units = await _db.UnitOfMeasures.ToListAsync();
            return View(units);
        }

        // GET: /UnitOfMeasures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /UnitOfMeasures/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Abbreviation")] UnitOfMeasure unit)
        {
            if (ModelState.IsValid) {
                if (await _db.UnitOfMeasures.AnyAsync(u => u.Name == unit.Name)) {
                    TempData["Error"] = "Ya existe una unidad con ese nombre.";
                    return RedirectToAction(nameof(Index));
                }

                _db.UnitOfMeasures.Add(unit);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Unidad '{unit.Name}' ({unit.Abbreviation}) creada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(unit);
        }

        // GET: /UnitOfMeasures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _db.UnitOfMeasures.FindAsync(id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: /UnitOfMeasures/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UnitOfMeasureId,Name,Abbreviation")] UnitOfMeasure unit)
        {
            if (id != unit.UnitOfMeasureId) return NotFound();

            if (ModelState.IsValid) {
                try
                {
                    _db.Update(unit);
                    await _db.SaveChangesAsync();
                    TempData["Info"] = $"Unidad '{unit.Name}' actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.UnitOfMeasures.Any(e => e.UnitOfMeasureId == id))
                        return NotFound();
                    else throw;
                }
            }

            return View(unit);
        }

        // GET: /UnitOfMeasures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _db.UnitOfMeasures
                .FirstOrDefaultAsync(u => u.UnitOfMeasureId == id);

            if (unit == null) return NotFound();

            bool isUsed = await _db.Materials.AnyAsync(m => m.UnitOfMeasureId == id);
            ViewBag.IsUsed = isUsed;

            return View(unit);
        }

        // POST: /UnitOfMeasures/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _db.UnitOfMeasures.FindAsync(id);
            if (unit == null) return NotFound();

            bool isUsed = await _db.Materials.AnyAsync(m => m.UnitOfMeasureId == id);
            if (isUsed) {
                TempData["Error"] = $"No se puede eliminar la unidad '{unit.Name}' porque está en uso por materiales.";
                return RedirectToAction(nameof(Index));
            }

            _db.UnitOfMeasures.Remove(unit);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Unidad '{unit.Name}' eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Método para verificar si una unidad está en uso vía AJAX
        [HttpGet]
        public async Task<IActionResult> CheckUnitUsage(int unitId)
        {
            var materialsUsingUnit = await _db.Materials
                .Where(m => m.UnitOfMeasureId == unitId)
                .Select(m => m.Name)
                .ToListAsync();

            return Json(new
            {
                isUsed = materialsUsingUnit.Any(),
                materialsCount = materialsUsingUnit.Count,
                materials = materialsUsingUnit
            });
        }
    }
}

