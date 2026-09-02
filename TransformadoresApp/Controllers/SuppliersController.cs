using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Purchasing;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.ShowInactive = showInactive;

            var query = _context.Suppliers.AsQueryable();

            if (!showInactive) {
                query = query.Where(s => s.IsActive);
            }

            var suppliers = await query
                .OrderBy(s => s.BusinessName)
                .ToListAsync();

            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            supplier.Code = supplier.Code.Trim();
            supplier.BusinessName = supplier.BusinessName.Trim();
            supplier.FantasyName = supplier.FantasyName?.Trim();
            supplier.TaxId = supplier.TaxId?.Trim();
            supplier.Email = supplier.Email?.Trim();
            supplier.Phone = supplier.Phone?.Trim();
            supplier.Address = supplier.Address?.Trim();
            supplier.City = supplier.City?.Trim();
            supplier.Province = supplier.Province?.Trim();
            supplier.Country = supplier.Country?.Trim();
            supplier.Notes = supplier.Notes?.Trim();

            var codeExists = await _context.Suppliers.AnyAsync(s => s.Code == supplier.Code);

            if (codeExists) {
                TempData["Error"] = "Ya existe un proveedor con ese código.";

                return RedirectToAction(nameof(Index));
            }

            supplier.IsActive = true;

            _context.Suppliers.Add(supplier);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();

            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            supplier.Code = supplier.Code.Trim();
            supplier.BusinessName = supplier.BusinessName.Trim();
            supplier.FantasyName = supplier.FantasyName?.Trim();
            supplier.TaxId = supplier.TaxId?.Trim();
            supplier.Email = supplier.Email?.Trim();
            supplier.Phone = supplier.Phone?.Trim();
            supplier.Address = supplier.Address?.Trim();
            supplier.City = supplier.City?.Trim();
            supplier.Province = supplier.Province?.Trim();
            supplier.Country = supplier.Country?.Trim();
            supplier.Notes = supplier.Notes?.Trim();

            var existingSupplier = await _context.Suppliers.FindAsync(id);

            if (existingSupplier == null) return NotFound();

            var codeExists = await _context.Suppliers.AnyAsync(s => s.Code == supplier.Code && s.Id != supplier.Id);

            if (codeExists) {
                TempData["Error"] = "Ya existe un proveedor con ese código.";

                return RedirectToAction(nameof(Index));
            }

            existingSupplier.Code = supplier.Code;
            existingSupplier.BusinessName = supplier.BusinessName;
            existingSupplier.FantasyName = supplier.FantasyName;
            existingSupplier.TaxId = supplier.TaxId;
            existingSupplier.Email = supplier.Email;
            existingSupplier.Phone = supplier.Phone;
            existingSupplier.Address = supplier.Address;
            existingSupplier.City = supplier.City;
            existingSupplier.Province = supplier.Province;
            existingSupplier.Country = supplier.Country;
            existingSupplier.Notes = supplier.Notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null) return NotFound();

            if (!supplier.IsActive) {
                TempData["Warning"] = "El proveedor ya se encuentra desactivado.";

                return RedirectToAction(nameof(Index));
            }

            supplier.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null) return NotFound();

            if (supplier.IsActive) {
                TempData["Warning"] = "El proveedor ya se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }

            supplier.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor restaurado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliersList()
        {
            var suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.BusinessName)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.BusinessName
                })
                .ToListAsync();

            return Json(suppliers);
        }
    }
}