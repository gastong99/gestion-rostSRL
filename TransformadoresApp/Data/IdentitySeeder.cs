using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TransformadoresApp.Models;

namespace TransformadoresApp.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Administrador", "Operador" };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el rol '{role}': " +
                    string.Join(", ", result.Errors.Select(error => error.Description)));
            }
        }

        // El administrador inicial se provisiona mediante configuración secreta.
        // No se incluye una contraseña conocida en el código fuente.
        var adminEmail = configuration["BootstrapAdmin:Email"]?.Trim();
        var adminPassword = configuration["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "No se configuró BootstrapAdmin:Email/Password. " +
                "Se omitió la creación automática del administrador inicial.");
        }
        else
        {
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

                var createResult = await userManager.CreateAsync(
                    adminUser,
                    adminPassword);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo crear el administrador inicial: " +
                        string.Join(", ", createResult.Errors.Select(error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Administrador"))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    adminUser,
                    "Administrador");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "No se pudo asignar el rol Administrador al usuario inicial: " +
                        string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                }
            }
        }

        // Migrar usuarios del rol legado Operario a Operador.
        if (await roleManager.RoleExistsAsync("Operario"))
        {
            var oldRoleUsers = await userManager.GetUsersInRoleAsync("Operario");

            foreach (var user in oldRoleUsers)
            {
                if (!await userManager.IsInRoleAsync(user, "Operador"))
                {
                    var addResult = await userManager.AddToRoleAsync(user, "Operador");

                    if (!addResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"No se pudo asignar el rol Operador al usuario '{user.Email}': " +
                            string.Join(", ", addResult.Errors.Select(error => error.Description)));
                    }
                }

                var removeResult = await userManager.RemoveFromRoleAsync(user, "Operario");

                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"No se pudo retirar el rol Operario al usuario '{user.Email}': " +
                        string.Join(", ", removeResult.Errors.Select(error => error.Description)));
                }
            }
        }
    }
}
