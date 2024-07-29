using Core.Constants;
using Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Web.Config
{
    public static class SeedUserExtension
    {
        public static async Task SeedRoles(this RoleManager<Role> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RoleConstants.SuperAdmin))
            {
                _ = await roleManager.CreateAsync(new Role { Name = RoleConstants.SuperAdmin });
            }

            if (!await roleManager.RoleExistsAsync(RoleConstants.Director))
            {
                _ = await roleManager.CreateAsync(new Role { Name = RoleConstants.Director });
            }
            if (!await roleManager.RoleExistsAsync(RoleConstants.Coordinator))
            {
                _ = await roleManager.CreateAsync(new Role { Name = RoleConstants.Coordinator });
            }
        }

        public static async Task SeedUsers(this UserManager<Persona> userManager)
        {
            if (await userManager.FindByNameAsync("SuperAdmin") is not null)
                return;

            var superAdmin = new Persona
            {
                Email = Environment.GetEnvironmentVariable("SUPER_ADMIN_EMAIL"),
                FirstName = "Super",
                Surname = "Admin",
                PhoneNumber = Environment.GetEnvironmentVariable("SUPER_ADMIN_PHONE"),
                UserName = "SuperAdmin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var result = await userManager.CreateAsync(superAdmin,
                Environment.GetEnvironmentVariable("SUPER_ADMIN_PASSWORD") ?? string.Empty);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, RoleConstants.SuperAdmin);
            }
        }

    }
}
