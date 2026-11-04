using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models;

namespace TransformadoresApp.Data
{
    public static class IdentitySeeder
    {
        // =====================
        // ROLES Y USUARIO ADMINISTRADOR
        // =====================

        public static async Task SeedRolesAndAdminAsync(
            IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // =====================
            // CREAR ROLES VIGENTES
            // =====================

            string[] roles = { "Administrador", "Operador" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"No se pudo crear el rol '{role}': " +
                            string.Join(", ",
                                result.Errors.Select(error => error.Description)));
                    }
                }
            }

            // =====================
            // USUARIO ADMINISTRADOR PRINCIPAL
            // =====================

            const string adminEmail = "admin@rostagno.com";

            var adminUser = await userManager.Users
                .FirstOrDefaultAsync(user => user.Email == adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    adminUser, "Admin123!");

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el usuario administrador: " +
                        string.Join(", ",
                            result.Errors.Select(error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                    adminUser, "Administrador"))
            {
                var result = await userManager.AddToRoleAsync(
                    adminUser, "Administrador");

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo asignar el rol Administrador: " +
                        string.Join(", ",
                            result.Errors.Select(error => error.Description)));
                }
            }

            // =====================
            // MIGRAR OPERARIO A OPERADOR
            // =====================

            if (await roleManager.RoleExistsAsync("Operario"))
            {
                var oldRoleUsers = await userManager
                    .GetUsersInRoleAsync("Operario");

                foreach (var user in oldRoleUsers)
                {
                    // Primero garantizamos la nueva asignación.
                    if (!await userManager.IsInRoleAsync(user, "Operador"))
                    {
                        var addResult = await userManager.AddToRoleAsync(
                            user, "Operador");

                        if (!addResult.Succeeded)
                        {
                            throw new InvalidOperationException(
                                $"No se pudo asignar el rol Operador al usuario " +
                                $"'{user.Email}': " +
                                string.Join(", ",
                                    addResult.Errors.Select(error => error.Description)));
                        }
                    }

                    // Quitamos el rol anterior únicamente después
                    // de confirmar la nueva asignación.
                    var removeResult = await userManager.RemoveFromRoleAsync(
                        user, "Operario");

                    if (!removeResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"No se pudo retirar el rol Operario al usuario " +
                            $"'{user.Email}': " +
                            string.Join(", ",
                                removeResult.Errors.Select(error => error.Description)));
                    }
                }
            }
        }
    }
}