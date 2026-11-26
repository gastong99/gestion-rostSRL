using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models;
using TransformadoresApp.ViewModels;

namespace TransformadoresApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 10;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Panel de Administración", null, null)
            };

            return View();
        } 

        public async Task<IActionResult> UserManagement(int page = 1)
        {
            ViewData["Breadcrumbs"] = new List<(string, string?, string?)>
            {
                ("Inicio", "Index", "Home"),
                ("Panel de Administración", "Index", "Admin"),
                ("Gestión de Usuarios", null, null)
            };

            page = Math.Max(page, 1);

            var usersQuery = _userManager.Users.OrderBy(user => user.Email);
            var totalUsers = await usersQuery.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalUsers / (double)PageSize));
            page = Math.Min(page, totalPages);

            var pagedUsers = await usersQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var list = new List<UserWithRolesViewModel>();
            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            var vm = new UserListViewModel
            {
                Users = list,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message)));
                return RedirectToAction(nameof(UserManagement));
            }

            model.Email = model.Email.Trim();

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                TempData["Error"] = "Ya existe un usuario con ese correo electrónico.";
                return RedirectToAction(nameof(UserManagement));
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                TempData["Error"] = "No se pudo crear el usuario: " +
                    string.Join("; ", createResult.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(UserManagement));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                TempData["Error"] = "El usuario no pudo recibir el rol y se canceló su creación: " +
                    string.Join("; ", roleResult.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(UserManagement));
            }

            TempData["Success"] = $"Usuario '{model.Email}' creado correctamente con el rol '{model.Role}'.";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToRole(string? userId, string? role)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                role is not ("Administrador" or "Operador"))
            {
                TempData["Error"] = "El usuario o rol indicado no es válido.";
                return RedirectToAction(nameof(UserManagement));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "El usuario seleccionado no existe.";
                return RedirectToAction(nameof(UserManagement));
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Info"] = "El usuario ya tiene ese rol asignado.";
                return RedirectToAction(nameof(UserManagement));
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Rol '{role}' asignado correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo asignar el rol: " +
                    string.Join("; ", result.Errors.Select(error => error.Description));
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromRole(string? userId, string? role)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                role is not ("Administrador" or "Operador"))
            {
                TempData["Error"] = "El usuario o rol indicado no es válido.";
                return RedirectToAction(nameof(UserManagement));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "El usuario seleccionado no existe.";
                return RedirectToAction(nameof(UserManagement));
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                TempData["Info"] = "El usuario no tiene ese rol asignado.";
                return RedirectToAction(nameof(UserManagement));
            }

            if (role == "Administrador")
            {
                var administrators = await _userManager.GetUsersInRoleAsync("Administrador");
                if (administrators.Count <= 1)
                {
                    TempData["Error"] = "No se puede quitar el rol al último administrador.";
                    return RedirectToAction(nameof(UserManagement));
                }

                if (user.Id == _userManager.GetUserId(User))
                {
                    TempData["Error"] = "No podés quitarte tu propio rol de Administrador.";
                    return RedirectToAction(nameof(UserManagement));
                }
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Rol '{role}' removido correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo quitar el rol: " +
                    string.Join("; ", result.Errors.Select(error => error.Description));
            }

            return RedirectToAction(nameof(UserManagement));
        }

    }
}