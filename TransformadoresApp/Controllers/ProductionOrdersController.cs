using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models;
using TransformadoresApp.Services;
using TransformadoresApp.ViewModels;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador,Operario")]
    public class ProductionOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BomService _bomService;
        private readonly PdfService _pdfService;
        private const int PageSize = 10;

        public ProductionOrdersController(ApplicationDbContext db, BomService bomService, PdfService pdfService)
        {
            _db = db;
            _bomService = bomService;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index(string? search, string? estado, DateTime? desde, DateTime? hasta, int? productId, int page = 1)
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Órdenes de Producción", null, null)
            };

            var query = _db.ProductionOrders
                .Include(o => o.Product)
                .Where(o => !o.IsDeleted)
                .AsQueryable();

            // Búsqueda por texto
            if (!string.IsNullOrWhiteSpace(search)) {
                query = query.Where(o =>
                    o.ProductionOrderId.ToString().Contains(search) ||
                    o.Product!.Name.Contains(search)
                );
            }

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<ProductionStatus>(estado.Replace(" ", ""), out var parsed)) {
                query = query.Where(o => o.Status == parsed);
            }

            // Filtro por producto
            if (productId.HasValue && productId > 0) {
                query = query.Where(o => o.ProductId == productId);
            }

            // Filtro por rango de fechas
            if (desde.HasValue) {
                query = query.Where(o => o.OrderDate >= desde.Value);
            }

            if (hasta.HasValue) {
                query = query.Where(o => o.OrderDate <= hasta.Value.AddDays(1).AddTicks(-1));
            }

            int totalOrders = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["Products"] = new SelectList(_db.Products.OrderBy(p => p.Name), "ProductId", "Name", productId);

            var vm = new ProductionOrderListViewModel
            {
                Orders = orders,
                Estado = estado,
                Desde = desde,
                Hasta = hasta,
                ProductId = productId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalOrders / (double)PageSize)
            };

            return View(vm);
        }

        // GET: /ProductionOrders/Trash
        public async Task<IActionResult> Trash()
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Órdenes de Producción", "Index", "ProductionOrders"),
                ("Papelera de Órdenes de Producción", null, null)
            };

            var deletedOrders = await _db.ProductionOrders
                .Include(o => o.Product)
                .Where(o => o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(deletedOrders);
        }

        // GET: /ProductionOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _db.ProductionOrders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.ProductionOrderId == id);

            if (order == null) return NotFound();

            var tuples = await _bomService.CalculateForProductAsync(order.ProductId, order.Quantity);

            var vm = tuples.Select(t => new BomResultViewModel
            {
                MaterialName = t.material.Name,
                Unit = t.material.UnitOfMeasure?.Abbreviation ?? "",
                TotalQty = t.totalQty
            }).ToList();

            ViewData["BomResult"] = vm;

            return View(order);
        }

        // GET: /ProductionOrders/PreviewBom?productId=1&quantity=2
        [HttpGet] public async Task<IActionResult> PreviewBom(int productId, decimal quantity) 
        { 
            if (productId <= 0 || quantity <= 0) return BadRequest(); 

            var tuples = await _bomService.CalculateForProductAsync(productId, quantity); 
            var vm = tuples.Select(t => new BomResultViewModel 
            { 
                MaterialName = t.material.Name, 
                Unit = t.material.UnitOfMeasure?.Abbreviation ?? "", 
                TotalQty = t.totalQty 
            }).ToList(); 
            
            return PartialView("~/Views/Shared/_BomTable.cshtml", vm); 
        }

        // GET: /ProductionOrders/Create
        public IActionResult Create()
        {
            var products = _db.Products.OrderBy(p => p.Name).ToList();
            products.Insert(0, new Product { ProductId = 0, Name = "Seleccionar..." });
            ViewData["ProductId"] = new SelectList(products, "ProductId", "Name", 0);

            return View();
        }

        // POST: /ProductionOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Quantity")] ProductionOrder order)
        {
            if (order.ProductId <= 0) {
                ModelState.AddModelError("ProductId", "Debe seleccionar un transformador válido.");
                TempData["Error"] = "Debe seleccionar un transformador para crear la orden.";
            }

            if (order.Quantity <= 0) {
                ModelState.AddModelError("Quantity", "La cantidad debe ser mayor que cero.");
                TempData["Error"] = TempData["Error"] != null
                    ? TempData["Error"] + " Además, la cantidad debe ser mayor que cero."
                    : "La cantidad debe ser mayor que cero.";
            }

            if (!ModelState.IsValid) {
                var products = _db.Products.OrderBy(p => p.Name).ToList();
                products.Insert(0, new Product { ProductId = 0, Name = "Seleccionar..." });
                ViewData["ProductId"] = new SelectList(products, "ProductId", "Name", order.ProductId);
                return View(order);
            }

            order.OrderDate = DateTime.UtcNow;
            order.Status = ProductionStatus.Pendiente;

            _db.Add(order);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Orden de producción creada con éxito.";
            return RedirectToAction(nameof(Details), new { id = order.ProductionOrderId });
        }


        // GET: /ProductionOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _db.ProductionOrders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.ProductionOrderId == id);

            if (order == null) return NotFound();

            if (order.Status == ProductionStatus.Completada || order.Status == ProductionStatus.Cancelada) {
                TempData["Error"] = "No se puede modificar una orden Completada o Cancelada.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StatusList = GetStatusSelectList(order.Status);
            return View(order);
        }

        // POST: /ProductionOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductionOrderId,ProductId,Quantity,Status")] ProductionOrder order)
        {
            if (id != order.ProductionOrderId) return NotFound();

            var original = await _db.ProductionOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.ProductionOrderId == id);

            if (original == null) return NotFound();

            if (!ModelState.IsValid) {
                ViewBag.StatusList = GetStatusSelectList(original.Status);
                return View(order);
            }

            _db.Update(order);
            await _db.SaveChangesAsync();

            TempData["Info"] = "Orden actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Mover a papelera (soft delete)
        [HttpPost]
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var order = await _db.ProductionOrders.FindAsync(id);

            if (order == null) return NotFound();

            order.IsDeleted = true;
            await _db.SaveChangesAsync();

            TempData["Info"] = "Orden movida a papelera.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Restaurar desde papelera
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var order = await _db.ProductionOrders.FindAsync(id);

            if (order == null) return NotFound();

            order.IsDeleted = false;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Orden restaurada correctamente.";
            return RedirectToAction(nameof(Trash));
        }

        // POST: Eliminación definitiva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _db.ProductionOrders.FindAsync(id);

            if (order == null) return NotFound();

            _db.ProductionOrders.Remove(order);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Orden eliminada definitivamente.";
            return RedirectToAction(nameof(Trash));
        }

        private List<SelectListItem> GetStatusSelectList(ProductionStatus current)
        {
            var list = new List<SelectListItem>();

            switch (current)
            {
                case ProductionStatus.Pendiente:
                    list.AddRange(new[]
                    {
                        new SelectListItem("Pendiente", "Pendiente"),
                        new SelectListItem("En Proceso", "EnProceso"),
                        new SelectListItem("Cancelada", "Cancelada")
                    });
                    break;

                case ProductionStatus.EnProceso:
                    list.AddRange(new[]
                    {
                        new SelectListItem("En Proceso", "EnProceso"),
                        new SelectListItem("Completada", "Completada"),
                        new SelectListItem("Cancelada", "Cancelada")
                    });
                    break;
            }

            return list;
        }

        // Validar transiciones de estado
        private bool IsValidStatusTransition(ProductionStatus current, string newStatusStr, out string errorMessage)
        {
            errorMessage = "";

            if (!Enum.TryParse<ProductionStatus>(newStatusStr.Replace(" ", ""), out var newStatus)) {
                errorMessage = "Estado inválido";
                return false;
            }

            // No se puede cambiar desde estados finales
            if (current == ProductionStatus.Completada || current == ProductionStatus.Cancelada) {
                errorMessage = "No se puede cambiar el estado de una orden Completada o Cancelada";
                return false;
            }

            // Validar transiciones permitidas
            switch (current)
            {
                case ProductionStatus.Pendiente:
                    // Desde Pendiente puede ir a EnProceso o Cancelada
                    if (newStatus != ProductionStatus.EnProceso && newStatus != ProductionStatus.Cancelada && newStatus != ProductionStatus.Pendiente) {
                        errorMessage = "Desde Pendiente solo se puede pasar a En Proceso o Cancelada";
                        return false;
                    }
                    break;

                case ProductionStatus.EnProceso:
                    // Desde EnProceso puede ir a Completada o Cancelada
                    if (newStatus != ProductionStatus.Completada && newStatus != ProductionStatus.Cancelada && newStatus != ProductionStatus.EnProceso) {
                        errorMessage = "Desde En Proceso solo se puede pasar a Completada o Cancelada";
                        return false;
                    }
                    break;
            }

            return true;
        }

        // Cambiar estado vía AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _db.ProductionOrders.FindAsync(id);
            if (order == null) return Json(new { success = false, message = "Orden no encontrada" });

            // Validar transiciones de estado
            if (!IsValidStatusTransition(order.Status, newStatus, out string errorMessage)) {
                return Json(new { success = false, message = errorMessage });
            }

            if (Enum.TryParse<ProductionStatus>(newStatus.Replace(" ", ""), out var parsedStatus)) {
                order.Status = parsedStatus;
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Estado actualizado correctamente" });
            }

            return Json(new { success = false, message = "Estado inválido" });
        }

        // Obtener lista de productos para AJAX
        [HttpGet]
        public async Task<IActionResult> GetProductsList()
        {
            var products = await _db.Products
                .OrderBy(p => p.Name)
                .Select(p => new {
                    productId = p.ProductId,
                    name = p.Name
                })
                .ToListAsync();

            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePdf(int id, bool download = false)
        {
            var order = await _db.ProductionOrders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.ProductionOrderId == id);

            if (order == null) return NotFound();

            var tuples = await _bomService.CalculateForProductAsync(order.ProductId, order.Quantity);
            var bomList = tuples.Select(t => new BomResultViewModel
            {
                MaterialName = t.material.Name,
                Unit = t.material.UnitOfMeasure?.Abbreviation ?? "",
                TotalQty = t.totalQty
            }).ToList();

            var pdfBytes = await _pdfService.GenerateProductionOrderPdfAsync(order, bomList);
            var fileName = $"Orden_{order.ProductionOrderId}.pdf";

            var contentType = "application/pdf";
            var disposition = download ? "attachment" : "inline";

            Response.Headers.Append("Content-Disposition", $"{disposition}; filename={fileName}");

            return File(pdfBytes, contentType);
        }
    }
}
