using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models.Customers;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool showInactive = false)
        {
            ViewBag.ShowInactive = showInactive;

            var query = _context.Customers.AsQueryable();

            if (!showInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var customers = await query
                .OrderBy(c => c.CustomerType)
                .ThenBy(c => c.CustomerType == CustomerType.Person
                    ? c.LastName
                    : c.BusinessName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();

            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, bool showInactive = false)
        {

            NormalizeCustomerType(customer, CustomerType.Person);

            PrepareCustomer(customer);

            RemoveIrrelevantCustomerModelState(customer);

            TryValidateModel(customer);

            if (!ValidateCustomer(customer))
            {
                return await ReturnToIndexWithValidation(
                    customer,
                    "create",
                    showInactive);
            }

            var codeExists = await _context.Customers
                .AnyAsync(c => c.Code == customer.Code);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(customer.Code),
                    "Ya existe un cliente con ese código.");

                return await ReturnToIndexWithValidation(
                    customer,
                    "create",
                    showInactive);
            }

            customer.Id = 0;
            customer.IsActive = true;

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer, bool showInactive = false)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            var existingCustomer = await _context.Customers.FindAsync(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            NormalizeCustomerType(customer, existingCustomer.CustomerType);

            PrepareCustomer(customer);

            RemoveIrrelevantCustomerModelState(customer);

            TryValidateModel(customer);

            if (!ValidateCustomer(customer))
            {
                return await ReturnToIndexWithValidation(
                    customer,
                    "edit",
                    showInactive);
            }

            var codeExists = await _context.Customers
                .AnyAsync(c => c.Code == customer.Code && c.Id != id);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(customer.Code),
                    "Ya existe un cliente con ese código.");

                return await ReturnToIndexWithValidation(
                    customer,
                    "edit",
                    showInactive);
            }

            existingCustomer.Code = customer.Code;
            existingCustomer.CustomerType = customer.CustomerType;
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.BusinessName = customer.BusinessName;
            existingCustomer.FantasyName = customer.FantasyName;
            existingCustomer.TaxId = customer.TaxId;
            existingCustomer.DocumentNumber = customer.DocumentNumber;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.ContactName = customer.ContactName;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;
            existingCustomer.Province = customer.Province;
            existingCustomer.Country = customer.Country;
            existingCustomer.Notes = customer.Notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            if (!customer.IsActive)
            {
                TempData["Warning"] = "El cliente ya se encuentra desactivado.";

                return RedirectToAction(nameof(Index));
            }

            customer.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            if (customer.IsActive)
            {
                TempData["Warning"] = "El cliente ya se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }

            customer.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cliente restaurado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomersList()
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerType)
                .ThenBy(c => c.CustomerType == CustomerType.Person
                    ? c.LastName
                    : c.BusinessName)
                .ThenBy(c => c.FirstName)
                .Select(c => new
                {
                    c.Id,
                    c.Code,
                    c.CustomerType,
                    DisplayName = c.CustomerType == CustomerType.Person
                        ? ((c.FirstName ?? "") + " " + (c.LastName ?? "")).Trim()
                        : (c.FantasyName ?? c.BusinessName ?? "")
                })
                .ToListAsync();

            return Json(customers);
        }

        private void NormalizeCustomerType(Customer customer, CustomerType fallbackType)
        {
            var typeKeys = ModelState.Keys
                .Where(key => key.Equals(nameof(Customer.CustomerType), StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith("." + nameof(Customer.CustomerType), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var hasBindingError = typeKeys.Any(key =>
                ModelState[key]?.Errors.Count > 0);

            var hasInvalidEnumValue = !Enum.IsDefined(
                typeof(CustomerType),
                customer.CustomerType);

            if (!hasBindingError && !hasInvalidEnumValue)
            {
                return;
            }

            foreach (var key in typeKeys)
            {
                ModelState.Remove(key);
            }

            customer.CustomerType = fallbackType;

            ModelState.AddModelError(
                nameof(Customer.CustomerType),
                "Seleccioná un tipo de cliente válido.");
        }

        private static void PrepareCustomer(Customer customer)
        {
            customer.Code = customer.Code?.Trim() ?? string.Empty;

            customer.FirstName = customer.FirstName?.Trim();
            customer.LastName = customer.LastName?.Trim();
            customer.BusinessName = customer.BusinessName?.Trim();
            customer.FantasyName = customer.FantasyName?.Trim();
            customer.TaxId = customer.TaxId?.Trim();
            customer.DocumentNumber = customer.DocumentNumber?.Trim();
            customer.Email = customer.Email?.Trim();
            customer.Phone = customer.Phone?.Trim();
            customer.ContactName = customer.ContactName?.Trim();
            customer.Address = customer.Address?.Trim();
            customer.City = customer.City?.Trim();
            customer.Province = customer.Province?.Trim();
            customer.Country = customer.Country?.Trim();
            customer.Notes = customer.Notes?.Trim();

            if (customer.CustomerType == CustomerType.Person)
            {
                customer.BusinessName = null;
                customer.FantasyName = null;
            }
            else if (customer.CustomerType == CustomerType.Company)
            {
                customer.FirstName = null;
                customer.LastName = null;
            }
        }

        private bool ValidateCustomer(Customer customer)
        {
            if (!Enum.IsDefined(typeof(CustomerType), customer.CustomerType))
            {
                ModelState.AddModelError(
                    nameof(customer.CustomerType),
                    "El tipo de cliente seleccionado no es válido.");

                return false;
            }

            if (customer.CustomerType == CustomerType.Person)
            {
                if (string.IsNullOrWhiteSpace(customer.FirstName))
                {
                    ModelState.AddModelError(
                        nameof(customer.FirstName),
                        "El nombre es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(customer.LastName))
                {
                    ModelState.AddModelError(
                        nameof(customer.LastName),
                        "El apellido es obligatorio.");
                }
            }
            else if (customer.CustomerType == CustomerType.Company)
            {
                if (string.IsNullOrWhiteSpace(customer.BusinessName))
                {
                    ModelState.AddModelError(
                        nameof(customer.BusinessName),
                        "La razón social es obligatoria.");
                }
            }

            return ModelState.IsValid;
        }

        private async Task<IActionResult> ReturnToIndexWithValidation(Customer customer, string modal, bool showInactive)
        {
            ViewBag.ShowInactive = showInactive;
            ViewBag.ValidationModal = modal;

            if (modal == "create")
            {
                ViewData["CustomerCreateModel"] = customer;
            }
            else if (modal == "edit")
            {
                ViewData["CustomerEditModel"] = customer;
            }

            var query = _context.Customers.AsQueryable();

            if (!showInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var customers = await query
                .OrderBy(c => c.CustomerType)
                .ThenBy(c => c.CustomerType == CustomerType.Person
                    ? c.LastName
                    : c.BusinessName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();

            return View("Index", customers);
        }

        private void RemoveIrrelevantCustomerModelState(Customer customer)
        {
            if (customer.CustomerType == CustomerType.Person)
            {
                RemoveModelStateForProperty(nameof(Customer.BusinessName));
                RemoveModelStateForProperty(nameof(Customer.FantasyName));
                RemoveModelStateForProperty(nameof(Customer.ContactName));
            }
            else if (customer.CustomerType == CustomerType.Company)
            {
                RemoveModelStateForProperty(nameof(Customer.FirstName));
                RemoveModelStateForProperty(nameof(Customer.LastName));
                RemoveModelStateForProperty(nameof(Customer.DocumentNumber));
            }
        }

        private void RemoveModelStateForProperty(string propertyName)
        {
            var keys = ModelState.Keys
                .Where(key => key.Equals(propertyName, StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith("." + propertyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keys)
            {
                ModelState.Remove(key);
            }
        }
    }
}