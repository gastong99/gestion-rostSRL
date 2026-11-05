
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransformadoresApp.Models;

namespace TransformadoresApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // =====================
        // LOGIN
        // =====================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Debe ingresar correo y contraseña.";

                return RedirectToAction("Index", "Home");
            }

            email = email.Trim();

            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                isPersistent: true,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                TempData["Error"] = "La cuenta se encuentra temporalmente bloqueada. Intente más tarde.";

                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Credenciales inválidas.";

            return RedirectToAction("Index", "Home");
        }

        // =====================
        // REGISTER
        // Registro público deshabilitado
        // =====================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register()
        {
            TempData["Error"] = "El registro público está deshabilitado. Solicite la creación de su cuenta al Administrador.";

            return RedirectToAction("Index", "Home");
        }

        // =====================
        // LOGOUT
        // =====================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}